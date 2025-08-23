using AutoBattle;
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

        // PlayerDataContext 참조
        private readonly AutoBattlePlayerDataContext _ctx;

        public Deck(
            int maxUnitId,
            Func<int, bool> existsUnitIdHandler,
            Func<int, int, UnitAggregate> getUnitStatDataFunc,
            AutoBattlePlayerDataContext ctx)
        {
            _maxUnitId = maxUnitId;
            _unitIdExists = existsUnitIdHandler;
            _getUnitStatDataFunc = getUnitStatDataFunc;
            _deck = new Dictionary<UnitTier, List<CardData>>();
            _ctx = ctx;

            var inventory = ctx.Data.deckInventory;

            if (inventory == null || inventory.Count == 0)
            {
                InitializeCardPools();
                SaveDeckToInventory();  // _deck → ctx.Data.deckInventory
            }
            else
            {
                LoadDeckFromInventory(); // ctx.Data.deckInventory → _deck
            }

            ShufflePools();
        }

        private bool HasUnit(int unitId) => _unitIdExists?.Invoke(unitId) ?? false;

        private void InitializeCardPools()
        {
            foreach (UnitTier tier in Enum.GetValues(typeof(UnitTier)))
                _deck[tier] = new List<CardData>();

            for (int unitId = 0; unitId <= _maxUnitId; unitId++)
            {
                if (!HasUnit(unitId)) continue;

                var agg = _getUnitStatDataFunc(unitId, 1);
                if (agg.Data.UnitTier == UnitTier.Creep) continue;

                var card = new CardData(
                    agg.Data.UnitTier,
                    agg.Price,
                    agg.Data.UnitId,
                    agg.Stat.StarLevel
                );

                for (int j = 0; j < CARD_COUNT; j++)
                    _deck[agg.Data.UnitTier].Add(card);
            }
        }

        private void LoadDeckFromInventory()
        {
            foreach (UnitTier tier in Enum.GetValues(typeof(UnitTier)))
                _deck[tier] = new List<CardData>();

            foreach (var entry in _ctx.Data.deckInventory)
            {
                foreach (var u in entry.units)
                {
                    if (u.remaining <= 0) continue;
                    if (!HasUnit(u.unitId)) continue;

                    var agg = _getUnitStatDataFunc(u.unitId, 1);
                    var tier = agg.Data.UnitTier;

                    var card = new CardData(
                        tier,
                        agg.Price,
                        u.unitId,
                        agg.Stat.StarLevel
                    );

                    for (int i = 0; i < u.remaining; i++)
                        _deck[tier].Add(card);
                }
            }
        }

        private void SaveDeckToInventory()
        {
            _ctx.Data.deckInventory.Clear();

            foreach (UnitTier tier in Enum.GetValues(typeof(UnitTier)))
            {
                var entry = new DeckTierEntry(tier);

                if (_deck.TryGetValue(tier, out var cards) && cards.Count > 0)
                {
                    var groups = cards
                        .GroupBy(c => c.unitID)
                        .Select(g => new DeckUnitRemain { unitId = g.Key, remaining = g.Count() })
                        .OrderBy(x => x.unitId);

                    entry.units.AddRange(groups);
                }

                _ctx.Data.deckInventory.Add(entry);
            }
            _ctx.Save();
        }

        private void ShufflePools()
        {
            foreach (var kv in _deck)
            {
                var list = kv.Value;
                for (int i = list.Count - 1; i > 0; i--)
                {
                    int j = _rng.Next(i + 1);
                    (list[i], list[j]) = (list[j], list[i]);
                }
            }
        }

        public CardData DrawCard(UnitTier requestedTier)
        {
            ShufflePools();

            if (!_deck.TryGetValue(requestedTier, out var pool))
                throw new KeyNotFoundException($"티어 '{requestedTier}'에 대한 카드 풀이 없습니다.");

            if (pool.Count == 0)
                throw new InvalidOperationException($"티어 '{requestedTier}'의 카드 풀이 모두 소진되었습니다.");

            var card = pool[^1];
            pool.RemoveAt(pool.Count - 1);

            // Context에 위임: 남은 장수 1장 소비
            if (!_ctx.TryConsumeCard(card.tier, card.unitID))
                throw new InvalidOperationException($"인벤토리 불일치: unitId={card.unitID} 소비 실패");

            return card;
        }

        public void ReturnCard(CardData card)
        {
            if (!_deck.TryGetValue(card.tier, out var pool))
                throw new KeyNotFoundException($"티어 '{card.tier}'에 대한 카드 풀이 없습니다.");

            int current = _ctx.GetDeckRemaining(card.tier, card.unitID);
            if (current >= CARD_COUNT)
                throw new InvalidOperationException(
                    $"카드(UnitID={card.unitID}, Tier={card.tier})가 최대 복제 개수({CARD_COUNT})를 초과할 수 없습니다.");

            int insertIndex = _rng.Next(pool.Count + 1);
            pool.Insert(insertIndex, card);

            // Context에 위임: 남은 장수 +1
            _ctx.AddCard(card.tier, card.unitID);
        }

        public void RebuildInventoryFromDeck()
        {
            SaveDeckToInventory();
        }
    }
}
