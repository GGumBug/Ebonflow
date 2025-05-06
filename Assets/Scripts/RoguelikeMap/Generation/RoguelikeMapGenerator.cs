using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoguelikeMap
{
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
        /// 외부에서 랜덤 시드를 주입할 수 있습니다. 
        /// seed 가 null 이면 시드 없이(시간 기반) 랜덤 생성.
        /// </summary>
        public RoguelikeMapGenerator(int? seed = null)
        {
            _rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        /// <summary>생성된 모든 Path(세대) 리스트</summary>
        public List<List<MapEdge>> Paths => _paths;

        public List<List<MapNode>> CreateMap(int rowCount, int colCount, bool crossCheck = true)
        {
            _crossCheck = crossCheck;
            _gridTemplate = GenerateEmptyMapTemplate(rowCount, colCount);
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
                    Debug.LogError($"[MapGen] Generation {gen} failed after {_rng} attempts");
            }

            PruneEmptyRows();
            return _gridTemplate;
        }

        private bool TryGenerateSinglePath(int generationId)
        {
            var path = new List<MapEdge>();
            var startRow = _gridTemplate[0];
            var startNode = startRow[_rng.Next(startRow.Count)];
            var current = startNode;

            for (int floor = 1; floor < _gridTemplate.Count; floor++)
            {
                // 거리순 상위 NEAREST_CANDIDATE_COUNT 추출
                var candidates = _gridTemplate[floor]
                    .OrderBy(n => Vector2.Distance(n.position, current.position))
                    .Take(NEAREST_CANDIDATE_COUNT)
                    .ToList();

                // 교차 허용/금지에 따른 필터링
                var valid = !_crossCheck
                    ? candidates
                    : candidates.Where(c => !IsCrossingExistingEdges(floor, current, c)).ToList();

                if (valid.Count == 0) return false;

                // 랜덤 선택
                var chosen = valid[_rng.Next(valid.Count)];
                var edge = new MapEdge
                {
                    From = current,
                    To = chosen,
                    Generation = generationId
                };

                current.Edges.Add(edge);
                path.Add(edge);
                current = chosen;
            }

            _paths.Add(path);
            return true;
        }

        private void RollbackGeneration(int generationId)
        {
            _paths.RemoveAll(p => p.Any(e => e.Generation == generationId));
            foreach (var row in _gridTemplate)
                foreach (var node in row)
                    node.Edges.RemoveAll(e => e.Generation == generationId);
        }

        private void PruneEmptyRows()
        {
            foreach (var row in _gridTemplate)
                row.RemoveAll(n => n.Edges.Count == 0);
            _gridTemplate.RemoveAll(r => r.Count == 0);
        }

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

        private bool IsCrossingExistingEdges(int floor, MapNode from, MapNode to)
        {
            var nodes = _gridTemplate[floor];
            var a = from.position;
            var b = to.position;

            foreach (var node in nodes)
            {
                foreach (var edge in node.Edges)
                {
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

        private bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
             => CCW(a, c, d) != CCW(b, c, d)
             && CCW(a, b, c) != CCW(a, b, d);

        private bool CCW(Vector2 p, Vector2 q, Vector2 r)
             => (r.y - p.y) * (q.x - p.x) > (q.y - p.y) * (r.x - p.x);
    }
}
