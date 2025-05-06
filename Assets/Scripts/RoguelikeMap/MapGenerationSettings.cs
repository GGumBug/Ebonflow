using UnityEngine;

namespace RoguelikeMap
{
    [CreateAssetMenu(
        menuName = "Roguelike/Map Generation Settings",
        fileName = "MapGenerationSettings"
    )]
    public class MapGenerationSettings : ScriptableObject
    {
        [Header("Grid Size")]
        [Tooltip("생성할 맵의 행(Row) 개수")]
        [Min(1)]
        public int rowCount = 15;

        [Tooltip("생성할 맵의 열(Col) 개수")]
        [Min(1)]
        public int colCount = 7;

        [Header("Path Generation")]
        [Tooltip("경로를 몇 세대까지 생성할지")]
        [Min(1)]
        public int pathGenerationCount = 6;

        [Tooltip("다음 층에서 검사할 후보 노드 개수")]
        [Min(1)]
        public int nearestCandidateCount = 3;

        [Tooltip("교차 금지 시도 최대 횟수")]
        [Min(1)]
        public int maxAttemptsPerPath = 5;

        [Header("Cross-Check")]
        [Tooltip("경로 교차 금지 여부")]
        public bool crossCheck = true;

        [Header("Random Seed")]
        [Tooltip("재현 가능한 맵 생성을 위해 시드를 사용할지 여부")]
        public bool useSeed = false;

        [Tooltip("사용할 랜덤 시드 값")]
        public int seed = 12345;
    }
}