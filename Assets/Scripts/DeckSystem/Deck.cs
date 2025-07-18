using System;
using System.Collections.Generic;

namespace DeckSystem
{
    public class Deck
    {
        private const int CARD_COUNT = 9;
        private int _maxUnitId;
        private Dictionary<UnitTier, List<CardData>> _deck;
        private Func<int, bool> _unitIdExists;
        private Func<int, int, UnitStatData> _getUnitStatDataFunc;

        public Deck(int maxUnitId, Func<int, bool> existsUnitIdHandler, Func<int, int, UnitStatData> getUnitStatDataFunc)
        {
            _maxUnitId = maxUnitId;
            _unitIdExists = existsUnitIdHandler;
            _getUnitStatDataFunc = getUnitStatDataFunc;

            _deck = new Dictionary<UnitTier, List<CardData>>();
            InitializeCardPools();
        }

        private bool HasUnit(int unitId)
        {
            return _unitIdExists?.Invoke(unitId) ?? false;
        }

        private void InitializeCardPools()
        {
            foreach (UnitTier tier in Enum.GetValues(typeof(UnitTier)))
                _deck[tier] = new List<CardData>();

            for (int i = 0; i <= _maxUnitId; i++)
            {
                if (HasUnit(i))
                {
                    UnitStatData unitStatData = _getUnitStatDataFunc?.Invoke(i, 1);
                    //유닛 티어 받아서 카드 데이터 추가
                }
            }
        }
    }
}
