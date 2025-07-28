using AutoBattle;
using AutoBattle.UI;
using DeckSystem;
using UnityEngine;

public class UIAutoBattle : UIBase
{
    [SerializeField] private SellZonePanel sellZonePanel;

    [field:SerializeField] public UIAutoBattleShop AutoBattleShopUI { get; private set; }

    public void SetUp(CardDrawManager cardDrawManager, AutoBattlePlayerDataContext autoBattlePlayerDataContext)
    {
        AutoBattleShopUI.SetUp(cardDrawManager, autoBattlePlayerDataContext);

        AutoBattleUnitManager.Instance.DragController.OnDragStartedAction += OpenSellPanel;
        AutoBattleUnitManager.Instance.DragController.OnDragEndedAction += CloseSellPanel;
    }

    public void OpenSellPanel()
    {
        AutoBattleShopUI.gameObject.SetActive(false);
        sellZonePanel.gameObject.SetActive(true);
    }

    public void CloseSellPanel()
    {
        AutoBattleShopUI.gameObject.SetActive(true);
        sellZonePanel.gameObject.SetActive(false);
    }
}
