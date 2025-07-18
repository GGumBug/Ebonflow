using System;
using System.Linq;

namespace DeckSystem
{
    public class TierBasedCardPicker
    {
        private readonly LevelTierProbabilityConfig _config;
        private readonly Deck _deck;
        private readonly System.Random _rng;

        public TierBasedCardPicker(LevelTierProbabilityConfig config, Deck deck, int? seed = null)    // 테스트 편의를 위해 시드 주입 가능
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _deck = deck ?? throw new ArgumentNullException(nameof(deck));
            _rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        /// <summary>
        /// 플레이어 레벨에 맞춰 티어를 확률 선택하고, Deck에서 카드를 꺼냅니다.
        /// </summary>
        public CardData DrawRandomCard(int playerLevel)
        {
            if (playerLevel < 0 || playerLevel >= _config.LevelTierEntries.Length)
                throw new ArgumentOutOfRangeException(
                    nameof(playerLevel),
                    $"레벨 {playerLevel}은 설정 가능한 범위를 벗어났습니다. 유효 범위: 0 ~ {_config.LevelTierEntries.Length - 1}");

            // 1) 레벨별 설정 가져오기 (정확 매칭 없으면 낮은 레벨 중 최대값으로 대체)
            var entry = _config.LevelTierEntries.FirstOrDefault(f => f.Level == playerLevel);

            // 2) 가중치 랜덤으로 티어 결정
            var tier = PickTier(entry.TierProbabilities);

            // 3) 선택된 티어에서 비복원추출로 카드 뽑기
            return _deck.DrawCard(tier);
        }

        /// <summary>
        /// TierProbability 배열에서 가중치(Random)를 이용해 UnitTier 하나를 반환합니다.
        /// </summary>
        private UnitTier PickTier(TierProbability[] probs)
        {
            float total = probs.Sum(p => p.Probability);
            if (total <= 0f)
                throw new InvalidOperationException("티어 확률 합이 0 이하입니다.");

            float roll = (float)_rng.NextDouble() * total;
            float accum = 0f;

            foreach (var p in probs)
            {
                accum += p.Probability;
                if (roll <= accum)
                    return p.Tier;
            }

            // 부동소수점 오차 방지용: 마지막 티어 리턴
            return probs[^1].Tier;
        }
    }
}