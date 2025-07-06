using System;
using System.Collections.Generic;

namespace DeckSystem
{
    public class Deck
    {
        private int[] _counts;
        private Dictionary<CardTier, List<CardData>> _deck;

        public Deck()
        {
            InitializeCounts();
            InitializeCardPools();
        }

        private void InitializeCounts()
        {
            // 총 티어 수
            int tierCount = Enum.GetValues(typeof(CardTier)).Length;
            _counts = new int[tierCount];

            // Deck size 고정 예시: 총 50장 기준
            _counts[(int)CardTier.SoulWisp - 1] = 9;
            _counts[(int)CardTier.LostSoul - 1] = 9;
            _counts[(int)CardTier.DeathEnvoy - 1] = 9;
            _counts[(int)CardTier.GhostGeneral - 1] = 9;
            _counts[(int)CardTier.UnderworldKing - 1] = 9;
        }

        private void InitializeCardPools()
        {
            _deck = new Dictionary<CardTier, List<CardData>>();
            foreach (CardTier tier in Enum.GetValues(typeof(CardTier)))
                _deck[tier] = new List<CardData>();

            // UnitID 별로 덱에 카드 넣는 로직 추가
        }
    }
}
