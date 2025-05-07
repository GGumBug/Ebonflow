using UnityEngine;
using System.Collections.Generic;

namespace RoguelikeMap
{
    public class LocationWeightUtil
    {
        [Header("Ascension Level")]
        [Tooltip("현재 Act(Ascension) 레벨 (1부터 시작)")]
        [Min(1)]
        public int actLevel = 1;

        [Tooltip("최대 Act(Ascension) 레벨")]
        [Min(1)]
        public int maxActLevel = 20;

        private bool _dirty = true;
        private List<LocationWeight> _locationWeights;
        private WeightedRandomSelector<LocationType> _picker;

        public LocationWeightUtil(List<LocationWeight> locationWeights)
        {
            _locationWeights = locationWeights;
            RebuildPicker();
        }

        /// <summary>
        /// Act 레벨에 따라 baseW/peakW를 보간해 가중치 리스트를 만들고
        /// WeightedRandomSelector를 재생성합니다.
        /// </summary>
        private void RebuildPicker()
        {
            // 보간 계수 (0부터 1)
            float t = maxActLevel > 1
                ? (actLevel - 1) / (float)(maxActLevel - 1)
                : 0f;

            var list = new List<(LocationType, float)>(_locationWeights.Count);
            foreach (var lw in _locationWeights)
            {
                // baseW와 peakW 사이를 t 만큼 보간
                float w = Mathf.Lerp(lw.baseW, lw.peakW, t);
                list.Add((lw.type, w));
            }

            _picker = new WeightedRandomSelector<LocationType>(list);
            _dirty = false;
        }

        /// <summary>
        /// 보간된 가중치에 따라 무작위 LocationType을 반환합니다.
        /// </summary>
        public LocationType GetRandomLocation()
        {
            if (_dirty || _picker == null)
                RebuildPicker();
            return _picker.Next();
        }
    }
}
