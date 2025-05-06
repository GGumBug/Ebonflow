using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoguelikeMap
{
    /// <summary>
    /// 불규칙 등각 격자 기반으로 6개의 연속된 경로를 생성합니다.
    /// 교차 금지 옵션을 켜면, 모든 이전에 생성된 간선과 비교하여 절대 교차가 발생하지 않도록 보장합니다.
    /// </summary>
    public class RoguelikeMapGenerator
    {
        private const int PATH_GENERATION_COUNT = 6;
        private const int NEAREST_CANDIDATE_COUNT = 3;
        private const int MAX_ATTEMPTS_PER_PATH = 5;

        private readonly System.Random _rng;
        private bool _crossCheck;
        private List<List<MapNode>> _gridTemplate;
        private List<List<MapEdge>> _paths;

        /// <summary>
        /// 생성된 모든 Path(세대) 리스트에 접근합니다.
        /// </summary>
        public IReadOnlyList<List<MapEdge>> Paths => _paths;

        /// <summary>
        /// 외부에서 랜덤 시드를 주입할 수 있습니다.
        /// seed 가 null 이면 시간 기반 Random 사용.
        /// </summary>
        public RoguelikeMapGenerator(int? seed = null)
        {
            _rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        /// <summary>
        /// row×col 크기의 맵을 생성하고, 
        /// PATH_GENERATION_COUNT 만큼 경로를 뽑아냅니다.
        /// crossCheck=true 면 간선 교차를 완전히 방지합니다.
        /// </summary>
        public List<List<MapNode>> CreateMap(int rows, int cols, bool crossCheck = true)
        {
            _crossCheck = crossCheck;
            _gridTemplate = GenerateEmptyMapTemplate(rows, cols);
            _paths = new List<List<MapEdge>>();

            for (int gen = 0; gen < PATH_GENERATION_COUNT; gen++)
            {
                bool success = false;
                int tries = 0;

                while (!success && tries++ < MAX_ATTEMPTS_PER_PATH)
                {
                    success = TryGenerateSinglePath(gen);
                    if (!success) RollbackGeneration(gen);
                }

                if (!success)
                    Debug.LogError($"[MapGen] Generation {gen} failed after {tries} attempts.");
            }

            PruneEmptyRows();
            return _gridTemplate;
        }

        /// <summary>
        /// 한 세대에 대한 경로를 시도 생성합니다.
        /// 교차 금지 옵션이 켜져 있으면,
        /// 기존에 _paths 에 쌓인 모든 간선들과 비교합니다.
        /// </summary>
        private bool TryGenerateSinglePath(int generationId)
        {
            var singlePath = new List<MapEdge>();
            var startRow = _gridTemplate[0];
            var current = startRow[_rng.Next(startRow.Count)];

            for (int floor = 1; floor < _gridTemplate.Count; floor++)
            {
                // 거리순 상위 후보 추출
                var candidates = _gridTemplate[floor]
                    .OrderBy(n => Vector2.Distance(n.position, current.position))
                    .Take(NEAREST_CANDIDATE_COUNT)
                    .ToList();

                // 교차 검사: 꺼져 있으면 모두 유효, 켜져 있으면 교차 없는 것만
                var valid = !_crossCheck
                    ? candidates
                    : candidates.Where(c => !IsCrossingAnyExistingEdge(current, c)).ToList();

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

            _paths.Add(singlePath);
            return true;
        }

        /// <summary>
        /// 기존에 생성된 모든 간선(Paths)과 비교하여
        /// from→to 간선이 교차하는지 검사합니다.
        /// </summary>
        private bool IsCrossingAnyExistingEdge(MapNode from, MapNode to)
        {
            var a = from.position;
            var b = to.position;

            foreach (var path in _paths)
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
        private void RollbackGeneration(int generationId)
        {
            // _paths에서 제거
            _paths.RemoveAll(p => p.Any(e => e.Generation == generationId));
            // 각 노드의 Edges에서도 제거
            foreach (var row in _gridTemplate)
                foreach (var node in row)
                    node.Edges.RemoveAll(e => e.Generation == generationId);
        }

        /// <summary>
        /// 간선을 하나도 갖지 않은 노드를 제거하고,
        /// 빈 행도 함께 제거합니다.
        /// </summary>
        private void PruneEmptyRows()
        {
            foreach (var row in _gridTemplate)
                row.RemoveAll(n => n.Edges == null || n.Edges.Count == 0);

            _gridTemplate.RemoveAll(r => r.Count == 0);
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
                    row.Add(new MapNode(r, c, RoomType.None));
                template.Add(row);
            }
            return template;
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
