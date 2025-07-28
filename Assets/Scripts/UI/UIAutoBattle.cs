using AutoBattle;
using AutoBattle.UI;
using DeckSystem;
using UnityEngine;

public class UIAutoBattle : UIBase
{
    [SerializeField] private SoulCoinPanel soulCoinPanel;

    [field:SerializeField] public UIAutoBattleShop AutoBattleShopUI { get; private set; }
    [field: SerializeField] public SellZonePanel SellZonePanel { get; private set; }

    public void SetUp(CardDrawManager cardDrawManager, AutoBattlePlayerDataContext autoBattlePlayerDataContext)
    {
        AutoBattleShopUI.SetUp(cardDrawManager, autoBattlePlayerDataContext);
        soulCoinPanel.Setup();

        AutoBattleUnitManager.Instance.DragController.OnDragStartedAction += OpenSellPanel;
        AutoBattleUnitManager.Instance.DragController.OnDragEndedAction += CloseSellPanel;
    }

    public void OpenSellPanel()
    {
        AutoBattleShopUI.gameObject.SetActive(false);
        SellZonePanel.gameObject.SetActive(true);
    }

    public void CloseSellPanel()
    {
        AutoBattleShopUI.gameObject.SetActive(true);
        SellZonePanel.gameObject.SetActive(false);
    }
}
