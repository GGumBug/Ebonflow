using UnityEngine;

public class RewardService : ISoulCoinRewardService
{
    // 상수 정의 (대문자 언더바 컨벤션)
    private const int DEFAULT_INTEREST_AMOUNT = 5;   // 기본 지급 SOUL COIN
    private const int COINS_PER_INTEREST = 10;       // 동적 이자 산정 기준 SOUL COIN
    private const int MAX_INTEREST_DYNAMIC = 5;      // 최대 동적 이자 상한

    // 연승·연패 스테이크 보상 상수
    private const int STREAK_THRESHOLD_3 = 3;
    private const int STREAK_THRESHOLD_5 = 5;
    private const int STREAK_REWARD_3_4 = 1;
    private const int STREAK_REWARD_5 = 2;
    private const int STREAK_REWARD_6_PLUS = 3;

    private readonly AutoBattlePlayerDataContext _playerContext;

    /// <summary>
    /// 생성자에서 플레이어 데이터 컨텍스트를 주입받습니다.
    /// </summary>
    public RewardService(AutoBattlePlayerDataContext playerContext)
    {
        _playerContext = playerContext;
    }

    public int ApplyInterest()
    {
        int current = _playerContext.GetSoulCoin();
        int streak = _playerContext.GetWinLossStreak();

        // 보유량 기반 동적 이자 계산 (10 SOUL COIN당 1, 최대 MAX_INTEREST_DYNAMIC)
        int dynamicInterest = current / COINS_PER_INTEREST;
        dynamicInterest = Mathf.Min(dynamicInterest, MAX_INTEREST_DYNAMIC);

        // 연승·연패 스테이크 보상 계산 (절댓값 기준)
        int absStreak = Mathf.Abs(streak);
        int streakReward = 0;
        if (absStreak >= STREAK_THRESHOLD_3 && absStreak < STREAK_THRESHOLD_5)
            streakReward = STREAK_REWARD_3_4;
        else if (absStreak == STREAK_THRESHOLD_5)
            streakReward = STREAK_REWARD_5;
        else if (absStreak > STREAK_THRESHOLD_5)
            streakReward = STREAK_REWARD_6_PLUS;

        // 총 이자 합산
        int totalInterest = DEFAULT_INTEREST_AMOUNT + dynamicInterest + streakReward;

        int total = _playerContext.AddSoulCoin(totalInterest);
        Debug.Log($"이자 지급: 기본 {DEFAULT_INTEREST_AMOUNT} + 동적 {dynamicInterest} + 스테이크 보상 {streakReward} = {totalInterest} SOUL COIN, 현재 잔액: {total}");
        return total;
    }
}
