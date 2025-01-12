using Cysharp.Threading.Tasks;

public interface ILoadableScene
{
    /// <summary>
    /// 리소스 로드
    /// </summary>
    UniTask LoadAssets();

    /// <summary>
    /// 데이터 초기화
    /// </summary>
    UniTask InitializeData();

    /// <summary>
    /// 씬 설정
    /// </summary>
    UniTask SetupScene();

    /// <summary>
    /// 로딩 완료 후 최종 작업
    /// </summary>
    UniTask FinalizeLoading();
}