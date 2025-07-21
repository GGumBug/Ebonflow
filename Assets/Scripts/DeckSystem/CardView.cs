using AutoBattle;
using DeckSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private Button btnBuy;
    [SerializeField] private TextMeshProUGUI txtUnitTier;
    [SerializeField] private TextMeshProUGUI txtPrice;
    [SerializeField] private TextMeshProUGUI txtUnitID;

    public CardData Data { get; private set; }

    private void Awake()
    {
        btnBuy.onClick.AddListener(BuyCardUnit);
    }

    public void SetData(CardData cardData)
    {
        Data = cardData;
        txtUnitTier.text = Data.tier.ToString();
        txtPrice.text = "Price" + Data.price.ToString();
        txtUnitID.text = "UnitID" + Data.unitID.ToString();
    }

    private void BuyCardUnit()
    {
        AutoBattleUnitManager.Instance.SpawnToBench(Data.unitID, Data.starLevel);
    }
}
