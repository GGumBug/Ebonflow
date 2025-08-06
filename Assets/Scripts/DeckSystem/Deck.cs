using System;
using System.Collections.Generic;
using System.Linq;

namespace DeckSystem
{
    public class Deck
    {
        private const int CARD_COUNT = 9;
        private readonly int _maxUnitId;
        private readonly Dictionary<UnitTier, List<CardData>> _deck;
        private readonly Func<int, bool> _unitIdExists;
        private readonly Func<int, int, UnitAggregate> _getUnitStatDataFunc;
        private readonly Random _rng = new Random();

        public Deck(
            int maxUnitId,
            Func<int, bool> existsUnitIdHandler,
            Func<int, int, UnitAggregate> getUnitStatDataFunc)
        {
            _maxUnitId = maxUnitId;
            _unitIdExists = existsUnitIdHandler;
            _getUnitStatDataFunc = getUnitStatDataFunc;
            _deck = new Dictionary<UnitTier, List<CardData>>();

            InitializeCardPools();
            ShufflePools();
        }

        private bool HasUnit(int unitId)
            => _unitIdExists?.Invoke(unitId) ?? false;

        private void InitializeCardPools()
        {
            // 1) 티어별 빈 리스트 생성
            foreach (UnitTier tier in Enum.GetValues(typeof(UnitTier)))
                _deck[tier] = new List<CardData>();

            // 2) 카드 데이터 추가
            for (int i = 0; i <= _maxUnitId; i++)
            {
                if (!HasUnit(i))
                    continue;

                var aggregate = _getUnitStatDataFunc(i, 1);

                if (aggregate.Data.UnitTier == UnitTier.Creep)
                    continue;

                var card = new CardData(
                    aggregate.Data.UnitTier,
                    aggregate.Price,
                    aggregate.Data.UnitId,
                    aggregate.Stat.StarLevel
                );

                for (int j = 0; j < CARD_COUNT; j++)
                    _deck[aggregate.Data.UnitTier].Add(card);
            }
        }

        private void ShufflePools()
        {
            foreach (var tier in _deck.Keys)
            {
                var list = _deck[tier];
                int n = list.Count;
                for (int i = n - 1; i > 0; i--)
                {
                    int j = _rng.Next(i + 1);
                    (list[i], list[j]) = (list[j], list[i]);
                }
            }
        }

        /// <summary>
        /// 비복원추출 방식으로 한 장의 카드를 꺼내 반환합니다.
        /// 꺼낸 카드는 풀에서 제거되어 재출현하지 않습니다.
        /// </summary>
        public CardData DrawCard(UnitTier tier)
        {
            ShufflePools();

            if (!_deck.TryGetValue(tier, out var pool))
                throw new KeyNotFoundException($"티어 '{tier}'에 대한 카드 풀이 없습니다.");

            int count = pool.Count;
            if (count == 0)
                throw new InvalidOperationException($"티어 '{tier}'의 카드 풀이 모두 소진되었습니다.");

            // 마지막 요소를 꺼내고 제거
            CardData card = pool[count - 1];
            pool.RemoveAt(count - 1);
            return card;
        }

        /// <summary>
        /// 비복원 추출로 뽑힌 카드를 다시 해당 티어 풀에 반환합니다.
        /// 이미 풀에 존재하면 중복을 방지하기 위해 예외를 던집니다.
        /// </summary>
        public void ReturnCard(CardData card)
        {
            if (!_deck.TryGetValue(card.tier, out var pool))
                throw new KeyNotFoundException($"티어 '{card.tier}'에 대한 카드 풀이 없습니다.");

            int existingCopies = pool.Count(c => c.unitID == card.unitID && c.tier == card.tier);
            if (existingCopies >= CARD_COUNT)
                throw new InvalidOperationException(
                    $"카드(UnitID={card.unitID}, Tier={card.tier})가 최대 복제 개수({CARD_COUNT})를 초과할 수 없습니다.");

            // 랜덤 위치에 삽입 (0부터 Count까지 가능한 모든 인덱스)
            int insertIndex = _rng.Next(pool.Count + 1);
            pool.Insert(insertIndex, card);
        }
    }
}
