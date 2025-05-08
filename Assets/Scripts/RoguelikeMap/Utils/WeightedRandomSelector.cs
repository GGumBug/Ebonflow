using System;
using System.Collections.Generic;

namespace RoguelikeMap
{
    /// <summary>
    /// T 타입에 대해 (weight, value) 쌍을 받아서
    /// 누적합 기반의 가중치 랜덤 추첨을 지원합니다.
    /// </summary>
    public class WeightedRandomSelector<T>
    {
        readonly struct Entry
        {
            public readonly T     Value;
            public readonly float CumulativeWeight;
            public Entry(T v, float cw) { Value = v; CumulativeWeight = cw; }
        }

        private readonly List<Entry> _entries = new();

        /// <summary>
        /// 생성 시 weightDictionary 의 키값 순서대로 가중치 누적합을 계산합니다.
        /// 합이 1.0이 아니면 내부적으로 정규화 합니다.
        /// </summary>
        public WeightedRandomSelector(IList<(T value, float weight)> valueWeights)
        {
            if (valueWeights == null || valueWeights.Count == 0)
                throw new ArgumentException("Must provide at least one weight.");

            float sum = 0f;
            foreach (var vw in valueWeights)
                sum += vw.weight;

            // 누적합 빌드
            float cum = 0f;
            for (int i = 0; i < valueWeights.Count; i++)
            {
                float w = valueWeights[i].weight / sum;
                cum += w;
                _entries.Add(new Entry(valueWeights[i].value, cum));
            }
        }

        /// <summary>
        /// (0,1] 범위의 랜덤에 따라 해당 값을 돌려줍니다.
        /// </summary>
        public T Next()
        {
            float r = UnityEngine.Random.value;
            // 첫 번째 누적합 >= r 인 항목 반환
            for (int i = 0; i < _entries.Count; i++)
            {
                if (r <= _entries[i].CumulativeWeight)
                    return _entries[i].Value;
            }
            // 안전망
            return _entries[_entries.Count - 1].Value;
        }
    }
}