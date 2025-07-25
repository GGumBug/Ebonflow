using System;
using UnityEngine;
using UnityEngine.UI;

public class BuyButtonBase : MonoBehaviour
{
    [SerializeField] protected Button btnBuy;

    protected int price;
    protected Func<int> getSoulCoin;
    protected Func<int, bool> requestSpendSoulCoin;

    public virtual void SetBuyButton(AutoBattlePlayerDataContext autoBattlePlayerDataContext)
    {
        getSoulCoin = autoBattlePlayerDataContext.GetSoulCoin;
        requestSpendSoulCoin += autoBattlePlayerDataContext.SpendSoulCoin;
    }

    public void CheckCanBuy()
    {
        bool canBuy = price <= getSoulCoin.Invoke();
        btnBuy.interactable = canBuy;
    }
}
