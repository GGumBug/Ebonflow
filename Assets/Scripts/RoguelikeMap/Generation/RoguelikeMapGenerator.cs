using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace RoguelikeMap
{
    /// <summary>
    /// 불규칙 등각 격자 기반으로 6개의 연속된 경로를 생성합니다.
    /// 교차 금지 옵션을 켜면, 모든 이전에 생성된 간선과 비교하여 절대 교차가 발생하지 않도록 보장합니다.
    /// </summary>
    public class RoguelikeMapGenerator
    {
        private readonly System.Random _rng;
        private MapGenerationSettings _settings;
        private LocationWeightUtil _locationWeightUtil;

        /// <summary>
        /// 외부에서 랜덤 시드를 주입할 수 있습니다.
        /// seed 가 null 이면 시간 기반 Random 사용.
        /// </summary>
        public RoguelikeMapGenerator(MapGenerationSettings settings)
        {
            _settings = settings;
            _locationWeightUtil = new LocationWeightUtil(_settings.locationWeights, _settings.rowCount);
            int? seed = _settings.useSeed ? _settings.seed : null;
            _rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        /// <summary>
        /// 그리드와 경로들을 생성해서 MapLayout 으로 반환합니다.
        /// </summary>
        public MapLayout CreateMap()
        {
            var grid  = GenerateEmptyMapTemplate(_settings.rowCount, _settings.colCount);
            var paths = new List<List<MapEdge>>();

            // 3) Path 생성
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
        /// RoomType.None 로 채워진 빈 템플릿을 만듭니다.
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
            return template;
        }
        
        /// <summary>
        /// 한 세대에 대한 경로를 시도 생성합니다.
        /// 교차 금지 옵션이 켜져 있으면,
        /// 기존에 _paths 에 쌓인 모든 간선들과 비교합니다.
        /// </summary>
        private bool TryGenerateSinglePath( List<List<MapNode>> grid, List<List<MapEdge>> paths, int generationId)
        {
            var singlePath = new List<MapEdge>();
            var startRow = grid[0];
            var current = startRow[_rng.Next(startRow.Count)];

            for (int floor = 1; floor < grid.Count; floor++)
            {
                // 거리순 상위 후보 추출
                var candidates = grid[floor]
                    .OrderBy(n => Vector2.Distance(n.position, current.position))
                    .Take(_settings.nearestCandidateCount)
                    .ToList();

                // 교차 검사: 꺼져 있으면 모두 유효, 켜져 있으면 교차 없는 것만
                var valid = !_settings.crossCheck
                    ? candidates
                    : candidates.Where(c => !IsCrossingAnyExistingEdge(paths,current, c)).ToList();

                if (valid.Count == 0)
                    return false;  // 이 층에서 연결 불가 → 실패

                // 랜덤 선택
                var chosen = valid[_rng.Next(valid.Count)];
                var edge = new MapEdge
                {
                    From = current,
                    To = chosen,
                    Generation = generationId
                };

                current.Edges.Add(edge);
                singlePath.Add(edge);
                current = chosen;
            }

            paths.Add(singlePath);
            return true;
        }

        /// <summary>
        /// 기존에 생성된 모든 간선(Paths)과 비교하여
        /// from→to 간선이 교차하는지 검사합니다.
        /// </summary>
        private bool IsCrossingAnyExistingEdge(List<List<MapEdge>> paths, MapNode from, MapNode to)
        {
            var a = from.position;
            var b = to.position;

            foreach (var path in paths)
            {
                foreach (var edge in path)
                {
                    // 공유 endpoint는 교차로 보지 않음
                    if (edge.From == from || edge.To == from ||
                        edge.From == to || edge.To == to)
                        continue;

                    var c = edge.From.position;
                    var d = edge.To.position;
                    if (SegmentsIntersect(a, b, c, d))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 실패한 세대의 모든 간선을 롤백합니다.
        /// </summary>
        private void RollbackGeneration( List<List<MapNode>> grid, List<List<MapEdge>> paths, int generationId)
        {
            // _paths에서 제거
            paths.RemoveAll(p => p.Any(e => e.Generation == generationId));
            // 각 노드의 Edges에서도 제거
            foreach (var row in grid)
                foreach (var node in row)
                    node.Edges.RemoveAll(e => e.Generation == generationId);
        }

        /// <summary>
        /// 간선을 하나도 갖지 않은 노드를 제거하고,
        /// 빈 행도 함께 제거합니다.
        /// </summary>
        private void PruneEmptyRows(List<List<MapNode>> grid)
        {
            foreach (var row in grid)
                row.RemoveAll(n => n.Edges == null || n.Edges.Count == 0);

            grid.RemoveAll(r => r.Count == 0);
        }

        private void AssignRandomFloorLocations(List<List<MapNode>> grid)
        {
            for (int floor = 0; floor < grid.Count; floor++)
            {
                int actLevel = floor + 1;

                foreach (var node in grid[floor])
                {
                    if (node.type != LocationType.None)
                        continue;

                    LocationType pick;
                    int tries = 0;
                    do
                    {
                        pick = _locationWeightUtil.GetRandomLocation(actLevel);
                        tries++;
                        if (tries > 10) break; // 무한 루프 방지
                    }
                    while (HasAdjacentSameNonMonster(grid, node, pick, floor));

                    node.type = pick;
                }
            }
        }

        /// <summary>
        /// node의 인접 노드(이전 & 다음 층) 중에
        /// pick 타입(단 Monster 제외)이 있으면 true.
        /// 이전 층은 floor-1 행만 검사합니다.
        /// </summary>
        private bool HasAdjacentSameNonMonster(List<List<MapNode>> grid, MapNode node, LocationType pick, int floor)
        {
            // Monster 타입은 언제나 허용
            if (pick == LocationType.Monster)
                return false;

            // 1) 다음 층(자식) 검사: node.Edges 에서 바로 확인
            foreach (var edge in node.Edges)
            {
                if (edge.To.type == pick)
                    return true;
            }

            // 2) 이전 층(부모) 검사: 바로 위 row[floor-1] 만 뒤집니다.
            if (floor > 0)
            {
                var prevRow = grid[floor - 1];
                foreach (var parent in prevRow)
                {
                    // parent.Edges 에서 나가는 엣지 중 this node 로 향하는 것이 있는지
                    foreach (var edge in parent.Edges)
                    {
                        if (edge.To == node && parent.type == pick)
                            return true;
                    }
                }
            }

            return false;
        }

        private void AssignFixedFloorLocations(List<List<MapNode>> grid)
        {
            foreach (var node in grid[0])
                node.type = LocationType.Monster;

            foreach (var node in grid[_settings.rowCount - 2])
                node.type = LocationType.Camp;
        }

        /// <summary>
        /// 두 선분 AB와 CD가 엄밀히 교차하는지 검사합니다.
        /// 공유점(엔드포인트)은 교차로 처리하지 않음.
        /// </summary>
        private bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            return CCW(a, c, d) != CCW(b, c, d)
                && CCW(a, b, c) != CCW(a, b, d);
        }

        /// <summary>
        /// p→q→r 가 반시계(왼쪽) 회전하는지 검사합니다.
        /// </summary>
        private bool CCW(Vector2 p, Vector2 q, Vector2 r)
        {
            return (r.y - p.y) * (q.x - p.x)
                 > (q.y - p.y) * (r.x - p.x);
        }
    }
}
