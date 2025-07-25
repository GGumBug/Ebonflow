using DeckSystem;
using System;
using UnityEngine;

public class UIAutoBattleShop : UIBase
{
    [SerializeField] private RectTransform cardsPanelRect;

    private CardView[] cardViews;

    public event Func<int> RequestSoulCoin;

    public void SetUp(CardDrawManager cardDrawManager, AutoBattlePlayerDataContext autoBattlePlayerDataContext)
    {
        CashCards(cardDrawManager, autoBattlePlayerDataContext);
    }

    public void CashCards(CardDrawManager cardDrawManager, AutoBattlePlayerDataContext autoBattlePlayerDataContext)
    {
        cardViews = cardsPanelRect.GetComponentsInChildren<CardView>();
        for (int i = 0; i < cardViews.Length; i++)
            cardViews[i].SetCardView(i, cardDrawManager, autoBattlePlayerDataContext);
    }

    public void SetNewCardData(int index, CardData cardData)
    {
        if (index >= cardViews.Length)
        {
            throw new System.Exception("카드 뷰의 범위를 벗어난 Index입니다.");
        }

        var TargetCard = cardViews[index];
        int currentSoulCoin = RequestSoulCoin.Invoke();
        bool canBuy = currentSoulCoin >= cardData.price;
        TargetCard.SetData(cardData, canBuy);
    }

    public void CheckCanBuyCards(int soulCoin)
    {
        foreach (var cardView in cardViews)
            cardView.CheckCanBuy(soulCoin);
    }
}
