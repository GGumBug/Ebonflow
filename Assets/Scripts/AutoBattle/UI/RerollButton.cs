using DeckSystem;

public class RerollButton : BuyButtonBase
{
    private const int REROLL_PRICE = 2;

    public override void SetBuyButton(AutoBattlePlayerDataContext autoBattlePlayerDataContext)
    {
        base.SetBuyButton(autoBattlePlayerDataContext);

        price = REROLL_PRICE;
        CheckCanBuy();
    }

    public void SetEvent(CardDrawManager cardDrawManager)
    {
        btnBuy.onClick.AddListener(() => HandleReroll(cardDrawManager));
    }

    public void HandleReroll(CardDrawManager cardDrawManager)
    {
        if (requestCanBuy(REROLL_PRICE))
        {
            requestSpendSoulCoin(REROLL_PRICE);
            cardDrawManager.DrawFiveCard();
        }
    }
}
