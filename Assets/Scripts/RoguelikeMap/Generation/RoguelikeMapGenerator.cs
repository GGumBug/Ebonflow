using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoguelikeMap
{
    /// <summary>
    /// 불규칙 등각 격자 기반으로 맵과 경로를 생성합니다.
    /// </summary>
    public class RoguelikeMapGenerator
    {
        #region Fields
        private readonly System.Random _rng;
        private readonly MapGenerationSettings _settings;
        private readonly LocationWeightUtil _locationWeightUtil;
        #endregion

        #region Constructor
        /// <summary>
        /// 외부에서 랜덤 시드를 주입할 수 있습니다.
        /// seed 가 null 이면 시간 기반 Random 사용.
        /// </summary>
        public RoguelikeMapGenerator(MapGenerationSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _locationWeightUtil = new LocationWeightUtil(_settings.locationWeights, _settings.rowCount);

            int? seed = _settings.useSeed ? (int?)_settings.seed : null;
            _rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 그리드와 경로들을 생성하여 MapLayout으로 반환합니다.
        /// </summary>
        public MapLayout CreateMap()
        {
            var grid = GenerateEmptyMapTemplate(_settings.rowCount, _settings.colCount);
            var paths = new List<List<MapEdge>>();

            // 경로 생성
            for (int gen = 0; gen < _settings.pathGenerationCount; gen++)
            {
                bool success = false;
                int tries = 0;

                while (!success && tries++ < _settings.maxAttemptsPerPath)
                {
                    success = TryGenerateSinglePath(grid, paths, gen);
                    if (!success)
                        RollbackGeneration(grid, paths, gen);
                }

                if (!success)
                    Debug.LogWarning($"[MapGen] Generation {gen} failed after {tries} attempts.");
            }

            PruneEmptyRows(grid);
            AssignFixedFloorLocations(grid);
            AssignRandomFloorLocations(grid);

            return new MapLayout(_settings.rowCount, _settings.colCount, grid, paths);
        }

        /// <summary>
        /// 저장된 MapData를 기반으로 MapLayout을 재구성합니다.
        /// </summary>
        public MapLayout ReconstructLayout(MapData data, MapGenerationSettings settings)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var grid = BuildGrid(data);
            var paths = BuildPaths(data, grid);
            return new MapLayout(data.maxRow, data.maxCol, grid, paths);
        }
        #endregion

        #region Generation Steps
        /// <summary>
        /// 빈 템플릿 그리드를 생성하고 보스 룸을 할당합니다.
        /// </summary>
        private List<List<MapNode>> GenerateEmptyMapTemplate(int rows, int cols)
        {
            var template = new List<List<MapNode>>(rows);
            for (int r = 0; r < rows; r++)
            {
                var row = new List<MapNode>(cols);
                for (int c = 0; c < cols; c++)
                    row.Add(new MapNode(r, c, LocationType.None));

                template.Add(row);
            }

            AssignBossRoom(template);
            return template;
        }

        /// <summary>
        /// 보스 룸을 최상단 중앙에 지정합니다.
        /// </summary>
        private void AssignBossRoom(List<List<MapNode>> grid)
        {
            int bossRow = grid.Count - 1;
            int bossCol = grid[bossRow].Count / 2;
            grid[bossRow][bossCol].type = LocationType.Boss;
        }

        /// <summary>
        /// 단일 경로 생성을 시도합니다.
        /// </summary>
        private bool TryGenerateSinglePath(List<List<MapNode>> grid, List<List<MapEdge>> paths, int generationId)
        {
            var singlePath = new List<MapEdge>();
            var current = grid[0][_rng.Next(grid[0].Count)];

            for (int floor = 1; floor < grid.Count; floor++)
            {
                MapNode chosen;
                MapEdge edge;

                if (floor == grid.Count - 1)
                {
                    chosen = grid[floor][grid[floor].Count / 2];
                    edge = new MapEdge { From = current, To = chosen, Generation = generationId };
                    current.Edges.Add(edge);
                    singlePath.Add(edge);
                    break;
                }

                var candidates = grid[floor]
                    .OrderBy(n => Vector2.Distance(n.position, current.position))
                    .Take(_settings.nearestCandidateCount)
                    .ToList();

                var valid = !_settings.crossCheck
                    ? candidates
                    : candidates.Where(c => !IsCrossingAnyExistingEdge(paths, current, c)).ToList();

                if (valid.Count == 0)
                    return false;

                chosen = valid[_rng.Next(valid.Count)];
                edge = new MapEdge { From = current, To = chosen, Generation = generationId };

                current.Edges.Add(edge);
                singlePath.Add(edge);
                current = chosen;
            }

            paths.Add(singlePath);
            return true;
        }

        /// <summary>
        /// 실패한 세대의 경로를 롤백합니다.
        /// </summary>
        private void RollbackGeneration(List<List<MapNode>> grid, List<List<MapEdge>> paths, int generationId)
        {
            paths.RemoveAll(p => p.Any(e => e.Generation == generationId));
            foreach (var row in grid)
                row.ForEach(n => n.Edges.RemoveAll(e => e.Generation == generationId));
        }

        /// <summary>
        /// 빈 노드 및 빈 행을 제거합니다.
        /// </summary>
        private void PruneEmptyRows(List<List<MapNode>> grid)
        {
            grid.ForEach(row => row.RemoveAll(n => n.type == LocationType.None && n.Edges.Count == 0));
            grid.RemoveAll(row => row.Count == 0);
        }

        /// <summary>
        /// 0층과 끝 전층에 고정된 타입을 할당합니다.
        /// </summary>
        private void AssignFixedFloorLocations(List<List<MapNode>> grid)
        {
            grid[0].ForEach(n => n.type = LocationType.Monster);
            grid[grid.Count - 2].ForEach(n => n.type = LocationType.Camp);
        }

        /// <summary>
        /// 가중치에 따른 랜덤 룸 타입을 할당합니다.
        /// </summary>
        private void AssignRandomFloorLocations(List<List<MapNode>> grid)
        {
            for (int floor = 0; floor < grid.Count; floor++)
            {
                int actLevel = floor + 1;
                var validWeights = _settings.locationWeights
                    .Where(lw => actLevel >= lw.minFloor && (lw.maxFloor == 0 || actLevel <= lw.maxFloor))
                    .ToList();

                if (validWeights.Count == 0)
                {
                    Debug.LogWarning($"Act {actLevel}에 할당 가능한 LocationType이 없습니다.");
                    continue;
                }

                foreach (var node in grid[floor])
                {
                    if (node.type != LocationType.None) continue;

                    LocationType pick;
                    int tries = 0;
                    do
                    {
                        pick = GetRandomLocationByWeight(actLevel, validWeights);
                    } while (tries++ < 10 && HasAdjacentSameNonMonster(grid, node, pick, floor));

                    node.type = pick;
                }
            }
        }
        #endregion

        #region Utilities
        private bool IsCrossingAnyExistingEdge(List<List<MapEdge>> paths, MapNode from, MapNode to)
        {
            var a = from.position;
            var b = to.position;
            return paths.SelectMany(p => p).Any(edge =>
                !(edge.From == from || edge.To == from || edge.From == to || edge.To == to)
                && SegmentsIntersect(a, b, edge.From.position, edge.To.position));
        }

        private LocationType GetRandomLocationByWeight(int actLevel, List<LocationWeight> weights)
        {
            const int maxActLevel = 20;
            var weighted = weights.Select(lw =>
            {
                float t = (actLevel - 1) / (float)(maxActLevel - 1);
                float w = Mathf.Max(0, Mathf.Lerp(lw.baseW, lw.peakW, t));
                return (lw.type, w);
            }).ToList();

            float total = weighted.Sum(x => x.w);
            float rnd = UnityEngine.Random.value * total;

            foreach (var (type, w) in weighted)
            {
                if (rnd < w) return type;
                rnd -= w;
            }
            return weighted.Last().type;
        }

        private bool HasAdjacentSameNonMonster(List<List<MapNode>> grid, MapNode node, LocationType pick, int floor)
        {
            if (pick == LocationType.Monster) return false;

            if (node.Edges.Any(e => e.To.type == pick)) return true;

            if (floor > 0)
            {
                foreach (var parent in grid[floor - 1])
                    if (parent.Edges.Any(e => e.To == node && parent.type == pick))
                        return true;
            }

            return false;
        }

        private bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
            => CCW(a, c, d) != CCW(b, c, d)
            && CCW(a, b, c) != CCW(a, b, d);

        private bool CCW(Vector2 p, Vector2 q, Vector2 r)
            => (r.y - p.y) * (q.x - p.x) > (q.y - p.y) * (r.x - p.x);
        #endregion

        #region Data Reconstruction
        private List<List<MapNode>> BuildGrid(MapData data)
        {
            var grid = new List<List<MapNode>>(data.nodes.Length);
            var all  = new List<MapNode>();

            foreach (var rowData in data.nodes)
            {
                var rowList = new List<MapNode>(rowData.row.Count);
                foreach (var nd in rowData.row)
                {
                    var node = new MapNode(nd.row, nd.col, nd.type, nd.isActive);
                    rowList.Add(node);
                    all.Add(node);
                }
                grid.Add(rowList);
            }
            return grid;
        }

        private List<List<MapEdge>> BuildPaths(MapData data, List<List<MapNode>> grid)
        {
            var allNodes = grid.SelectMany(r => r).ToList();
            var paths    = new List<List<MapEdge>>(data.edges.Length);

            foreach (var rowData in data.edges)
            {
                var edgeList = new List<MapEdge>(rowData.path.Count);
                foreach (var ed in rowData.path)
                {
                    var from = allNodes[ed.fromIndex];
                    var to   = allNodes[ed.toIndex];
                    var edge = new MapEdge
                    {
                        From       = from,
                        To         = to,
                        Generation = ed.generation,
                        IsActive   = ed.isActive,
                        HasPassed  = ed.hasPassed
                    };
                    from.Edges.Add(edge);
                    edgeList.Add(edge);
                }
                paths.Add(edgeList);
            }

            return paths;
        }
        #endregion
    }
}