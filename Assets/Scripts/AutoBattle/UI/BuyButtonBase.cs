using System;
using UnityEngine;
using UnityEngine.UI;

public class BuyButtonBase : MonoBehaviour
{
    protected Button btnBuy;
    protected int price;
    protected Func<int, bool> requestSpendSoulCoin;
    protected Func<int, bool> requestCanBuy;

    protected virtual void Awake()
    {
        btnBuy = GetComponent<Button>();
    }

    public virtual void SetBuyButton(AutoBattlePlayerDataContext autoBattlePlayerDataContext)
    {
        requestCanBuy = autoBattlePlayerDataContext.CanBuy;
        requestSpendSoulCoin = autoBattlePlayerDataContext.SpendSoulCoin;
    }

    public bool CheckCanBuy()
    {
        bool canBuy = requestCanBuy(price);
        btnBuy.interactable = canBuy;
        return canBuy;
    }
}
