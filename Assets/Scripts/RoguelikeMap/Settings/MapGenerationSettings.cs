using System;
using System.Collections.Generic;
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

        [Header("Location Assignment")]
        [Tooltip("맵 룸에 부여할 LocationType 별 가중치 리스트 (base→peak)")]
        public List<LocationWeight> locationWeights = new();

        [Header("Debug")]
        [Tooltip("에디터에서 기즈모를 사용할 지 여부")]
        public bool isDrawGizmo = false;
    }

    /// <summary>
    /// A1에서 baseW, A20에서 peakW가 되도록 Act 레벨별 가중치를 정의합니다.
    /// </summary>
    [Serializable]
    public struct LocationWeight
    {
        [Tooltip("맵 위치(로케이션) 타입")]
        public LocationType type;

        [Tooltip("A1 (첫 번째 Act)에서 선택될 가중치")]
        public float baseW;

        [Tooltip("A20 (최대 Ascension)에서 선택될 가중치")]
        public float peakW;

        [Min(1), Tooltip("출현 가능한 최소 층 (1부터 적용)")]
        public int minFloor;

        [Min(0), Tooltip("출현 가능한 최대 층 (0이면 제한 없음)")]
        public int maxFloor;
    }
}