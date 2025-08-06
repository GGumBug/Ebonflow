using AutoBattle;
using AutoBattle.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace DeckSystem
{
    public class CardDrawManager : Singleton<CardDrawManager>
    {
        private readonly List<CardData> _currentHand = new List<CardData>();

        private AutoBattlePlayerDataContext _autoBattlePlayerDataContext;
        private Deck _deck;
        private TierBasedCardPicker _tierBasedCardPicker;
        private Action<int, CardData> requestSetCardView;

        public IReadOnlyList<CardData> GetCurrentHand() => _currentHand;

        public void SetUp(AutoBattleUnitManager autoBattleUnitManager, UIAutoBattleShop uIAutoBattleShop)
        {
            _currentHand.Clear();
            _autoBattlePlayerDataContext = AutoBattleDataManager.Instance.AutoBattlePlayerDataContext;

            var unitStatRepository = autoBattleUnitManager.UnitStatRepository;

            _deck = new Deck(unitStatRepository.GetMaxId(), unitStatRepository.Exists, unitStatRepository.Get);
            _tierBasedCardPicker = new TierBasedCardPicker(_deck);

            requestSetCardView = uIAutoBattleShop.SetNewCardData;

            AutoBattleManager.Instance.StateController.PreparationEntered.Add(DrawFiveCard);
        }

        public void DrawFiveCard()
        {
            // 1. 이전 손에 든 카드들 덱으로 복귀
            foreach (var card in _currentHand)
                _deck.ReturnCard(card);

            _currentHand.Clear();

            // 2. 카드 5장 뽑아서 저장 및 UI 갱신
            for (int i = 0; i < 5; i++)
            {
                var newCard = DrawCard();
                _currentHand.Add(newCard);
                requestSetCardView(i, newCard);
            }
        }

        public void ResetIndexCard(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= _currentHand.Count)
                throw new ArgumentOutOfRangeException(nameof(cardIndex));

            var oldCard = _currentHand[cardIndex];

            var newCard = DrawCard();

            _currentHand[cardIndex] = newCard;
            requestSetCardView(cardIndex, newCard);
        }

        public CardData DrawCard()
        {
            if (_tierBasedCardPicker == null)
                throw new InvalidOperationException(
                    "CardDrawManager가 초기화되지 않았습니다. SetUp() 호출 후 사용하세요.");

            return _tierBasedCardPicker.DrawRandomCard(_autoBattlePlayerDataContext.GetLevel());
        }

        public void ReturnCardToDeck(CardData cardData) => _deck.ReturnCard(cardData);
    }
}