using DeckSystem;
using System;
using UnityEngine;

public class UIAutoBattleShop : UIBase
{
    [SerializeField] private RectTransform cardsPanelRect;

    private CardView[] cardViews;

    public event Func<int> RequestSoulCoin;

    public void SetUp()
    {
        CashCards();
    }

    public void CashCards()
    {
        cardViews = cardsPanelRect.GetComponentsInChildren<CardView>();
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
}
