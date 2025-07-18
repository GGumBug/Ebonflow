using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIBase : MonoBehaviour
{
    private UIManager _uiManager;
    protected UIManager UI => _uiManager;

    void Awake()
    {
        _uiManager = UIManager.Instance;
        AddListener();
        Init();
    }

    /// <summary> AddListener -> Init </summary>
    protected virtual void Init()
    {
    }

    /// <summary> AddListener -> Init </summary>
    protected virtual void AddListener()
    {
    }

    protected async UniTask<T> OpenUI<T>() where T : UIBase
    {
        return await UI.OpenUIAsync<T>();
    }

    public void OpenUI()
    {
        gameObject.SetActive(true);
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
    }
}
