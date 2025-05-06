using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RoguelikeMap
{
    /// <summary>
    /// 불규칙 등각 격자(삼각형) 기반으로 6개의 연속된 경로를 생성합니다.
    /// “교차 금지” 옵션을 켜면, 최대 재시도 횟수만큼 back-off 후 재생성하여 절대 교차가 일어나지 않도록 보장합니다.
    /// </summary>
    public class RoguelikeMapGenerator
    {
        private const int PATH_GENERATION_COUNT = 6;
        private const int NEAREST_CANDIDATE_COUNT = 3;
        private const int MAX_ATTEMPTS_PER_PATH = 5;

        private List<List<MapNode>> _gridTemplate;
        private List<List<MapEdge>> _paths;
        private Dictionary<int, List<MapEdge>> _edgesByFloor;
        private bool _crossCheck;

        /// <summary>생성된 모든 엣지 경로들(세대별) 반환</summary>
        public List<List<MapEdge>> Paths => _paths;

        /// <summary>
        /// rowCount×colCount 크기의 맵 템플릿을 만들고,
        /// 지정된 세대 수만큼 교차 금지 옵션을 지켜가며 Path를 생성합니다.
        /// </summary>
        public List<List<MapNode>> CreateMap(int rowCount, int colCount, bool crossCheck = true)
        {
            _gridTemplate = GenerateEmptyMapTemplate(rowCount, colCount);
            _paths = new List<List<MapEdge>>();
            _edgesByFloor = new Dictionary<int, List<MapEdge>>();
            _crossCheck = crossCheck;

            // 층마다 빈 리스트 준비
            for (int f = 1; f < rowCount; f++)
                _edgesByFloor[f] = new List<MapEdge>();

            for (int gen = 0; gen < PATH_GENERATION_COUNT; gen++)
            {
                bool success = false;
                int attempts = 0;

                // 최대 재시도 횟수만큼 back-off 후 재시도
                while (!success && attempts++ < MAX_ATTEMPTS_PER_PATH)
                {
                    success = TryGenerateSinglePath(gen);
                    if (!success)
                        RollbackGeneration(gen);
                }

                if (!success)
                    Debug.LogError($"[MapGen] Generation {gen} failed after {MAX_ATTEMPTS_PER_PATH} attempts.");
            }

            PruneEmptyRows();
            return _gridTemplate;
        }

        /// <summary>
        /// 단일 세대(generationId)에 대한 Path를 “교차 없이” 생성 시도.
        /// 성공 시 _paths 에 추가 후 true, 실패 시 false.
        /// </summary>
        private bool TryGenerateSinglePath(int generationId)
        {
            var singlePath = new List<MapEdge>();
            var startCandidates = _gridTemplate[0];
            var start = startCandidates[Random.Range(0, startCandidates.Count)];
            var current = start;

            for (int floor = 1; floor < _gridTemplate.Count; floor++)
            {
                // 상위 NEAREST_CANDIDATE_COUNT 후보
                var candidates = GetFlowNodes(floor)
                    .OrderBy(n => Vector2.Distance(n.position, current.position))
                    .Take(NEAREST_CANDIDATE_COUNT)
                    .ToList();

                // 교차 금지 옵션이 꺼져 있으면 모두 유효, 켜져 있으면 교차 없는 것만
                var valid = !_crossCheck
                    ? candidates
                    : candidates.Where(c => !IsCrossingExistingEdges(floor, current, c)).ToList();

                if (valid.Count == 0)
                    return false; // 이 층에서 연결 실패 → 전체 실패

                // 유효 후보 중 랜덤 선택
                var chosen = valid[Random.Range(0, valid.Count)];
                var edge = new MapEdge
                {
                    From = current,
                    To = chosen,
                    Generation = generationId
                };

                // 노드, 층별 엣지, 세대별 경로에 모두 추가
                current.Edges.Add(edge);
                _edgesByFloor[floor].Add(edge);
                singlePath.Add(edge);
                current = chosen;
            }

            _paths.Add(singlePath);
            return true;
        }

        /// <summary>
        /// 실패한 세대(generationId)에 대해, 노드와 _paths 에 추가된 모든 엣지를 롤백합니다.
        /// </summary>
        private void RollbackGeneration(int generationId)
        {
            // _paths에서 해당 세대 제거
            _paths.RemoveAll(p => p.Count > 0 && p[0].Generation == generationId);
            // 그리드 노드와 _edgesByFloor 에서 해당 세대 제거
            foreach (var row in _gridTemplate)
                foreach (var node in row)
                    node.Edges.RemoveAll(e => e.Generation == generationId);
            foreach (var floorEdges in _edgesByFloor.Values)
                floorEdges.RemoveAll(e => e.Generation == generationId);
        }

        /// <summary>
        /// 모든 행(row)에서 엣지를 하나도 갖지 않은 노드를 제거하고,
        /// 빈 행은 통째로 제거합니다.
        /// </summary>
        private void PruneEmptyRows()
        {
            foreach (var row in _gridTemplate)
                row.RemoveAll(node => node.Edges == null || node.Edges.Count == 0);
            _gridTemplate.RemoveAll(row => row.Count == 0);
        }

        /// <summary>
        /// 지정 크기로 RoomType.None 노드로 채워진 그리드 템플릿 생성
        /// </summary>
        private List<List<MapNode>> GenerateEmptyMapTemplate(int rowCount, int colCount)
        {
            var template = new List<List<MapNode>>(rowCount);
            for (int r = 0; r < rowCount; r++)
            {
                var row = new List<MapNode>(colCount);
                for (int c = 0; c < colCount; c++)
                    row.Add(new MapNode(r, c, RoomType.None));
                template.Add(row);
            }
            return template;
        }

        private List<MapNode> GetFlowNodes(int floor) => _gridTemplate[floor];

        /// <summary>
        /// 지정 층(floor)의 기존 엣지들과 교차하는지 검사
        /// </summary>
        private bool IsCrossingExistingEdges(int floor, MapNode from, MapNode to)
        {
            var a = from.position;
            var b = to.position;
            foreach (var edge in _edgesByFloor[floor])
            {
                if (edge.From == from || edge.To == from ||
                    edge.From == to || edge.To == to)
                    continue;

                var c = edge.From.position;
                var d = edge.To.position;
                if (SegmentsIntersect(a, b, c, d))
                    return true;
            }
            return false;
        }

        /// <summary>두 선분 AB와 CD의 교차 여부를 orientation + bounding-box 로 엄밀 검사</summary>
        private bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            int o1 = Orientation(a, b, c);
            int o2 = Orientation(a, b, d);
            int o3 = Orientation(c, d, a);
            int o4 = Orientation(c, d, b);

            // 일반 교차
            if (o1 * o2 < 0 && o3 * o4 < 0)
                return true;

            // colinear + 끝점이 겹치는 경우
            if (o1 == 0 && OnSegment(a, b, c)) return true;
            if (o2 == 0 && OnSegment(a, b, d)) return true;
            if (o3 == 0 && OnSegment(c, d, a)) return true;
            if (o4 == 0 && OnSegment(c, d, b)) return true;

            return false;
        }

        /// <summary>세 점 p→q→r 의 회전 방향 (0=colinear, 1=ccw, -1=cw)</summary>
        private int Orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            float val = (q.x - p.x) * (r.y - p.y) - (q.y - p.y) * (r.x - p.x);
            if (Mathf.Approximately(val, 0f)) return 0;
            return val > 0 ? 1 : -1;
        }

        /// <summary>선분 p–q 구간 위에 r이 포함되는지 검사</summary>
        private bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
        {
            return Mathf.Min(p.x, q.x) <= r.x && r.x <= Mathf.Max(p.x, q.x)
                && Mathf.Min(p.y, q.y) <= r.y && r.y <= Mathf.Max(p.y, q.y);
        }
    }
}
