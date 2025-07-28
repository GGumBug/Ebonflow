using AutoBattle;
using AutoBattle.UI;
using Cysharp.Threading.Tasks;
using System;

namespace DeckSystem
{
    public class CardDrawManager : Singleton<CardDrawManager>
    {
        private AutoBattlePlayerDataContext _autoBattlePlayerDataContext;
        private LevelTierProbabilityConfig _levelTierProbabilityConfig;
        private Deck _deck;
        private TierBasedCardPicker _tierBasedCardPicker;
        private Action<int, CardData> requestSetCardView;

        public async UniTask SetUp(AutoBattleUnitManager autoBattleUnitManager, UIAutoBattleShop uIAutoBattleShop)
        {
            _autoBattlePlayerDataContext = AutoBattleDataManager.Instance.AutoBattlePlayerDataContext;

            _levelTierProbabilityConfig = await AddressableManager.Instance.Load<LevelTierProbabilityConfig>(AddressableKey.LevelTierProbabilityConfig);
            var unitStatRepository = autoBattleUnitManager.UnitStatRepository;

            _deck = new Deck(unitStatRepository.GetMaxId(), unitStatRepository.Exists, unitStatRepository.Get);
            _tierBasedCardPicker = new TierBasedCardPicker(_levelTierProbabilityConfig, _deck);

            requestSetCardView = uIAutoBattleShop.SetNewCardData;

            AutoBattleManager.Instance.StateController.PreparationEntered.Add(DrawFiveCard);
        }

        public void DrawFiveCard()
        {
            // CurrentHand 가 null 이아니라면 CardPool로 되돌린다.

            // 임시 상수 1레벨
            for (int i = 0; i < 5; i++)
            {
                CardData newCardData = DrawCard();
                requestSetCardView(i, newCardData);
            }
        }

        public void ResetIndexCard(int cardIndex)
        {
            CardData newCardData = DrawCard();
            requestSetCardView(cardIndex, newCardData);
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