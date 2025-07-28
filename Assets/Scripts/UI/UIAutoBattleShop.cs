using DeckSystem;
using UnityEngine;

namespace AutoBattle.UI
{
    public class UIAutoBattleShop : MonoBehaviour
    {
        [SerializeField] private RectTransform cardsPanelRect;
        [SerializeField] private RerollButton btnReroll;
        [SerializeField] private LevelUpButton btnLevelUp;

        private CardView[] cardViews;

        public void SetUp(CardDrawManager cardDrawManager, AutoBattlePlayerDataContext autoBattlePlayerDataContext)
        {
            CashCards(cardDrawManager);
            SetBuyButtons(autoBattlePlayerDataContext);
            SetButtonEvents(cardDrawManager);
        }

        private void CashCards(CardDrawManager cardDrawManager)
        {
            cardViews = cardsPanelRect.GetComponentsInChildren<CardView>();
            for (int i = 0; i < cardViews.Length; i++)
                cardViews[i].SetCardView(i, cardDrawManager);
        }

        private void SetBuyButtons(AutoBattlePlayerDataContext autoBattlePlayerDataContext)
        {
            foreach (var cardView in cardViews)
                cardView.SetBuyButton(autoBattlePlayerDataContext);

            btnReroll.SetBuyButton(autoBattlePlayerDataContext);
            btnLevelUp.SetBuyButton(autoBattlePlayerDataContext);
        }

        private void SetButtonEvents(CardDrawManager cardDrawManager)
        {
            btnReroll.SetEvent(cardDrawManager);
        }

        public void SetNewCardData(int index, CardData cardData)
        {
            if (index >= cardViews.Length)
            {
                throw new System.Exception("카드 뷰의 범위를 벗어난 Index입니다.");
            }

            var TargetCard = cardViews[index];
            TargetCard.SetData(cardData);
        }

        public void CheckCanBuyCards()
        {
            foreach (var cardView in cardViews)
                cardView.CheckCanBuy();

            btnLevelUp.CheckCanBuy();
            btnReroll.CheckCanBuy();
        }
    }
}