using UnityEngine;
using System;
using System.Linq;

namespace DeckSystem
{
        [Serializable]
        public struct TierProbability
        {
            [Tooltip("티어")]
            public CardTier Tier;
            [Range(0f, 1f), Tooltip("해당 레벨에서 이 티어가 선택될 확률 (0~1)")]
            public float Probability;
        }

        [Serializable]
        public struct LevelTierEntry
        {
            [Tooltip("적용할 플레이어 레벨")]
            public int Level;
            [Tooltip("이 레벨에서의 티어별 확률")]
            public TierProbability[] TierProbabilities;
        }

        [CreateAssetMenu(menuName = "Deck/LevelTierProbabilityConfig", fileName = "LevelTierProbabilityConfig")]
        public class LevelTierProbabilityConfig : ScriptableObject
        {
            [Tooltip("레벨별 티어 확률 설정 리스트")]
            public LevelTierEntry[] LevelTierEntries;

        private void OnValidate()
        {
            if (LevelTierEntries == null) return;

            // 모든 CardTier 값을 가져와 배열 생성
            var allTiers = Enum.GetValues(typeof(CardTier)).Cast<CardTier>().ToArray();

            for (int i = 0; i < LevelTierEntries.Length; i++)
            {
                var entry = LevelTierEntries[i];

                // TierProbabilities가 없거나 길이가 다르면 초기화
                if (entry.TierProbabilities == null || entry.TierProbabilities.Length != allTiers.Length)
                {
                    entry.TierProbabilities = new TierProbability[allTiers.Length];
                    for (int j = 0; j < allTiers.Length; j++)
                    {
                        entry.TierProbabilities[j] = new TierProbability
                        {
                            Tier = allTiers[j],
                            Probability = 0f
                        };
                    }
                    LevelTierEntries[i] = entry;
                }
            }
        }
    }
}