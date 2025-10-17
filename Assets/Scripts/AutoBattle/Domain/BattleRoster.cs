using System.Collections.Generic;
using UnityEngine;

namespace AutoBattle
{
    public class BattleRoster : IBattleRoster
    {
        private readonly Dictionary<TeamType, HashSet<Unit>> _map = new()
        {
            { TeamType.Ally,  new HashSet<Unit>() },
            { TeamType.Enemy, new HashSet<Unit>() }
        };

        public IReadOnlyCollection<Unit> Allies => _map[TeamType.Ally];
        public IReadOnlyCollection<Unit> Enemies => _map[TeamType.Enemy];

        public void Register(Unit unit)
        {
            var team = unit.GetTeam();
            bool added = _map[team].Add(unit);
            unit.SubscribeBattleStateHandlers();

            if (added)
                Debug.Log($"[BattleRoster] Register: {unit.name} ({team})");
            else
                Debug.LogWarning($"[BattleRoster] Register: 이미 등록되어 있음 -> {unit.name} ({team})");
        }

        public void UnRegister(Unit unit)
        {
            var team = unit.GetTeam();
            bool removed = _map[team].Remove(unit);
            unit.UnsubscribeBattleStateHandlers();

            if (removed)
                Debug.Log($"[BattleRoster] Unregister: {unit.name} ({team})");
            else
                Debug.LogWarning($"[BattleRoster] Unregister: 목록에 없음 -> {unit.name} ({team})");
        }

        public bool Contains(Unit unit)
        {
            // 한 팀에만 속한다고 가정
            return _map[unit.GetTeam()].Contains(unit);
        }

        /// <summary>
        /// 주어진 팀의 상대 팀을 반환
        /// </summary>
        public IReadOnlyCollection<Unit> GetOpposingTeam(TeamType team)
        {
            TeamType opposingTeam = GetOpposingTeamType(team);

            return _map[opposingTeam];
        }

        /// <summary>
        /// 주어진 팀의 상대 팀 타입을 반환합니다.
        /// </summary>
        private TeamType GetOpposingTeamType(TeamType team)
        {
            return team switch
            {
                TeamType.Ally => TeamType.Enemy,
                TeamType.Enemy => TeamType.Ally,
                _ => throw new System.ArgumentException($"지원되지 않거나 알 수 없는 팀 타입이 전달되었습니다: {team}. TeamType Enum 정의를 확인하십시오.")
            };
        }

        // 팀 변경이 가능하다면 별도 메서드 제공
        public void ChangeTeam(Unit unit, TeamType newTeam)
        {
            UnRegister(unit);
            _map[newTeam].Add(unit);
        }
    }
}
