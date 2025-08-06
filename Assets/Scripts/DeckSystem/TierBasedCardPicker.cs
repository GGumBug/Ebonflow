using System;
using System.Collections.Generic;
using System.Linq;

namespace DeckSystem
{
    public class TierBasedCardPicker
    {
        private readonly Deck _deck;
        private readonly Random _rng;
        private readonly Dictionary<int, TierProbability[]> _probabilitiesByLevel;
        private readonly int _maxLevel;

        /// <summary>
        /// 생성자에서 덱과 랜덤 시드를 주입받고, DB에서 모든 레벨별 확률 데이터를 로드합니다.
        /// </summary>
        public TierBasedCardPicker(Deck deck, int? seed = null)
        {
            _deck = deck ?? throw new ArgumentNullException(nameof(deck));
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();

            // DB에서 모든 레벨 확률 데이터 로드 및 정렬
            var allEntries = DB_LevelTierProbability.FindEntities(f => true)
                ?? throw new InvalidOperationException("티어 확률 데이터가 존재하지 않습니다.");
            allEntries = allEntries.OrderBy(e => e.f_Level).ToList();

            _maxLevel = allEntries.Count;
            _probabilitiesByLevel = new Dictionary<int, TierProbability[]>(_maxLevel);

            // 각 레벨별 TierProbability 배열을 미리 생성
            foreach (var entry in allEntries)
            {
                var probs = new[]
                {
                new TierProbability { Tier = UnitTier.SoulWisp,       Probability = entry.f_SoulWisp },
                new TierProbability { Tier = UnitTier.LostSoul,       Probability = entry.f_LostSoul },
                new TierProbability { Tier = UnitTier.DeathEnvoy,     Probability = entry.f_DeathEnvoy },
                new TierProbability { Tier = UnitTier.GhostGeneral,   Probability = entry.f_GhostGeneral },
                new TierProbability { Tier = UnitTier.UnderworldKing, Probability = entry.f_UnderworldKing }
            };
                _probabilitiesByLevel[entry.f_Level] = probs;
            }
        }

        /// <summary>
        /// 플레이어 레벨에 맞춰 미리 생성한 확률 데이터를 가져와,
        /// 해당 티어 풀에서 무작위 카드를 꺼냅니다.
        /// </summary>
        /// <param name="playerLevel">플레이어 레벨 (1이상)</param>
        public CardData DrawRandomCard(int playerLevel)
        {
            // 유효 레벨 범위 체크
            if (playerLevel < 1 || playerLevel > _maxLevel)
                throw new ArgumentOutOfRangeException(
                    nameof(playerLevel),
                    $"레벨은 1 ~ {_maxLevel} 사이여야 합니다. 요청된 레벨: {playerLevel}");

            // 정확 매칭된 확률 배열 조회, 없으면 최상위 레벨 사용
            if (!_probabilitiesByLevel.TryGetValue(playerLevel, out var probs))
                probs = _probabilitiesByLevel[_maxLevel];

            // 가중치 랜덤으로 티어 선택 후 카드 뽑기
            var chosenTier = PickTier(probs);
            return _deck.DrawCard(chosenTier);
        }

        /// <summary>
        /// 주어진 확률 배열에서 무작위 추첨하여 UnitTier를 반환합니다.
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

            // 부동소수점 오차 방지용: 마지막 티어 반환
            return probs[^1].Tier;
        }

        public struct TierProbability
        {
            public UnitTier Tier;
            public float Probability;
        }
    }
}