using AutoBattle;
using Cysharp.Threading.Tasks;
using System;

namespace DeckSystem
{
    public class CardDrawManager
    {
        private LevelTierProbabilityConfig _levelTierProbabilityConfig;
        private Deck _deck;
        private TierBasedCardPicker _tierBasedCardPicker;
        private Action<int, CardData> requestSetCardView;

        public async UniTask SetUp(AutoBattleUnitManager autoBattleUnitManager, UIAutoBattleShop uIAutoBattleShop)
        {
            _levelTierProbabilityConfig = await AddressableManager.Instance.Load<LevelTierProbabilityConfig>(AddressableKey.LevelTierProbabilityConfig);
            var unitStatRepository = autoBattleUnitManager.UnitStatRepository;

            _deck = new Deck(unitStatRepository.GetMaxId(), unitStatRepository.Exists, unitStatRepository.Get);
            _tierBasedCardPicker = new TierBasedCardPicker(_levelTierProbabilityConfig, _deck);

            requestSetCardView = uIAutoBattleShop.SetNewCardData;

            AutoBattleManager.Instance.StateController.PreparationEntered.Add(DrawFiveCard);
        }

        private void DrawFiveCard()
        {
            // 임시 상수 1레벨
            for (int i = 0; i < 5; i++)
            {
                CardData newCardData = DrawCard(1);
                requestSetCardView(i, newCardData);
            }
        }

        public CardData DrawCard(int playerLevel)
        {
            if (_tierBasedCardPicker == null)
                throw new InvalidOperationException(
                    "CardDrawManager가 초기화되지 않았습니다. SetUp() 호출 후 사용하세요.");

            return _tierBasedCardPicker.DrawRandomCard(playerLevel);
        }
    }
}