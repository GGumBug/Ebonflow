using Cysharp.Threading.Tasks;
using DeckSystem;
using System;
using UnityEngine;

public class UIAutoBattleShop : UIBase
{
    [SerializeField] private RectTransform cardsPanelRect;

    private PoolManager _poolManager;
    private GameObject _cardViewOrigin;

    public async UniTask SetUp()
    {
        _poolManager = PoolManager.Instance;
        await LoadCardViewOrigin();
    }

    private async UniTask LoadCardViewOrigin()
    {
        _cardViewOrigin = await AddressableManager.Instance.Load<GameObject>(AddressableKey.CardView);
    }

    public void CreateCard(int index, CardData data)
    {
        var cardGo = _poolManager.GetFromPool<Poolable>(_cardViewOrigin, cardsPanelRect, default, default);
        CardView cardView = cardGo.GetComponent<CardView>();

        if (cardView == null)
            throw new InvalidOperationException("CardView 컴포넌트가 prefab에 없습니다.");

        cardView.transform.SetSiblingIndex(index);

        cardView.SetData(data);
    }
}
