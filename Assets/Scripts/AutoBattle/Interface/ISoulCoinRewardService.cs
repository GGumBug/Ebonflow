/// <summary>
/// 인터페이스: 소울코인 보상 및 이자 지급 서비스를 정의합니다.
/// </summary>
public interface ISoulCoinRewardService
{
    /// <summary>
    /// 현재 보유 소울코인에 이자를 계산해 지급합니다.
    /// </summary>
    /// <returns>지급 후 총 소울코인 수</returns>
    int ApplyInterest();
}
