using UnityEngine;
using System;
using System.Collections.Generic;

namespace RoguelikeMap
{
    public class LocationWeightUtil
    {
         private readonly int _maxActLevel;
        private readonly List<LocationWeight> _locationWeights;

        /// <summary>
        /// 생성자에 가중치 리스트와 최대 Act 레벨(예: 20)을 전달합니다.
        /// </summary>
        public LocationWeightUtil(List<LocationWeight> locationWeights, int maxActLevel)
        {
            if (locationWeights == null || locationWeights.Count == 0)
                throw new ArgumentException("가중치 리스트가 비어있습니다!");

            if (maxActLevel < 1)
                throw new ArgumentException("maxActLevel 은 1 이상이어야 합니다!");

            _locationWeights = locationWeights;
            _maxActLevel     = maxActLevel;
        }

        /// <summary>
        /// 지정한 actLevel (1 ~ maxActLevel)에 따라
        /// baseW ⇔ peakW 를 보간한 가중치로 랜덤 추첨합니다.
        /// </summary>
        /// <param name="actLevel">현재 Act 레벨 (1부터 시작)</param>
        public LocationType GetRandomLocation(int actLevel)
        {
            // clamp actLevel
            actLevel = Mathf.Clamp(actLevel, 1, _maxActLevel);

            // t ∈ [0,1] 계산 (1→0, maxActLevel→1)
            float t = (_maxActLevel > 1)
                ? (actLevel - 1) / (float)(_maxActLevel - 1)
                : 0f;

            // 보간된 (value, weight) 리스트 생성
            var weightedPairs = new List<(LocationType, float)>(_locationWeights.Count);
            foreach (var lw in _locationWeights)
            {
                // baseW, peakW 사이를 t만큼 보간
                float w = Mathf.Lerp(lw.baseW, lw.peakW, t);
                weightedPairs.Add((lw.type, w));
            }

            // 누적합 기반 랜덤 셀렉터로 추첨
            var picker = new WeightedRandomSelector<LocationType>(weightedPairs);
            return picker.Next();
        }
    }
}
