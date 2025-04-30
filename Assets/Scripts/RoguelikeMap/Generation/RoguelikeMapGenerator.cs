using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

namespace RoguelikeMap
{
    public class RoguelikeMapGenerator
    {
        private const int PATH_GENERATION_COUNT = 6;
        private const int NEAREST_CANDIDATE_COUNT = 3;

        private List<List<MapNode>> _gridTemplate;
        private List<List<MapEdge>> _paths;
        private Func<int, MapNode, MapNode, bool> _crossCheck;

        private int MaxFloor => _gridTemplate.Count;
        private List<MapNode> GetFlowNodes(int floor) => _gridTemplate[floor];
        public List<List<MapEdge>> GetPaths => _paths;

        public List<List<MapNode>> CreateMap(int rowCount, int colCount, bool crossCheck = true)
        {
            _gridTemplate = new();
            _paths = new();

            _gridTemplate = GenerateEmptyMapTemplate(rowCount, colCount);

            if (crossCheck)
                _crossCheck = IsCrossingExistingEdges;

            for (int i = 0; i < PATH_GENERATION_COUNT; i++)
                GenerateSinglePath(i);
                
            foreach (var row in _gridTemplate)
            {
                // MapNode.Edges는 GenerateSinglePath 내부에서 채워졌다고 가정
                row.RemoveAll(node => node.Edges == null || node.Edges.Count == 0);
            }
            // 혹시 비어버린 행(row)이 있다면 그것도 제거
            _gridTemplate.RemoveAll(row => row.Count == 0);

            return _gridTemplate;
        }

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

        private void GenerateSinglePath(int generationId)
        {
            List<MapNode> startCandidates = GetFlowNodes(0);
            int randomStartIndex = Random.Range(0, startCandidates.Count);
            MapNode start = startCandidates[randomStartIndex];

            // 2) 이후 각 층으로 이동
            MapNode current = start;
            List<MapEdge> singlePath = new();
            for (int floorIndex = 1; floorIndex < MaxFloor; floorIndex++)
            {
                // 2-1) 다음 층 노드들 중, 현 노드와의 거리로 정렬 후 상위 3개
                var next3 = GetFlowNodes(floorIndex)
                             .OrderBy(n => Vector2.Distance(n.position, current.position))
                             .Take(NEAREST_CANDIDATE_COUNT)
                             .ToList();

                // 2-2) 그 중 하나를 랜덤으로 픽, 단 “교차금지” 판정
                MapNode chosen = next3
                .OrderBy(_ => Random.value)
                .FirstOrDefault(candidate => !(bool)_crossCheck?.Invoke(floorIndex, current, candidate));

                if (chosen == null)
                {
                    // 교차가 모두 불가한 경우, 그냥 첫 번째로 강제 연결
                    chosen = next3[0];
                }

                // 2-3) 엣지 추가
                MapEdge result = new MapEdge { From = current, To = chosen, Generation = generationId };
                current.Edges.Add(result);
                singlePath.Add(result);
                current = chosen;
            }

            _paths.Add(singlePath);
        }

        private bool IsCrossingExistingEdges(int floorIndex, MapNode from, MapNode to)
        {
            var currentFloorNodes = GetFlowNodes(floorIndex);

            Vector2 a = from.position, b = to.position;

            foreach (var Node in currentFloorNodes)
            {
                if (Node.Edges.Count == 0)
                    continue;

                foreach (var edge in Node.Edges)
                {
                    // 공통 endpoint는 교차가 아님
                if (edge.From == from || edge.To == from ||
                    edge.From == to || edge.To == to)
                    continue;

                Vector2 c = edge.From.position;
                Vector2 d = edge.To.position;
                if (SegmentsIntersect(a, b, c, d))
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 두 선분 AB와 CD가 교차하는지 검사 (엄밀한 교차만: 공유점 제외).
        /// </summary>
        private bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            return CCW(a, c, d) != CCW(b, c, d)
                && CCW(a, b, c) != CCW(a, b, d);
        }

        /// <summary>
        /// 세 점 p, q, r 이 시계방향/반시계방향에 따라 True/False를 반환합니다.
        /// CCW(p, q, r) == true 이면 p→q→r 가 반시계(왼쪽) 회전.
        /// </summary>
        private bool CCW(Vector2 p, Vector2 q, Vector2 r)
        {
            // (r.y - p.y)*(q.x - p.x) > (q.y - p.y)*(r.x - p.x)
            return (r.y - p.y) * (q.x - p.x)
                 > (q.y - p.y) * (r.x - p.x);
        }
    }
}