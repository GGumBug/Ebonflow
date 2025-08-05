using AutoBattle;
using StageEditor;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class AutoBattleStagePicker
{
    /// <summary>
    /// 컨텍스트에 맞는 StageData를 DB에서 랜덤으로 꺼내 반환합니다.
    /// </summary>
    public StageData PickStage(AutoBattleStageData context)
    {
        // 1) DB_Stages에서 Act와 Floor 조건에 맞는 메타 엔티티 조회
        var candidates = DB_Stages.FindEntities(s =>
            s.f_StageAct == context.StageNumber
            && context.Floor >= s.f_MinFloor
            && context.Floor <= s.f_MaxFloor
        );

        if (candidates == null || candidates.Count == 0)
            throw new InvalidOperationException(
                $"[{nameof(AutoBattleStagePicker)}] 조건에 맞는 스테이지를 찾을 수 없습니다. " +
                $"Act={context.StageNumber}, Floor={context.Floor}");

        // 2) 랜덤으로 하나 선택
        var selectedMeta = candidates[Random.Range(0, candidates.Count)];

        // 3) 선택된 stageID로 적(Enemy) 정보 로드
        var enemyEntities = DB_StageEnemys.FindEntities(e => e.f_StageID == selectedMeta.f_StageID);

        // 4) StageData 객체 생성
        var data = new StageData
        {
            stageID = selectedMeta.f_StageID,
            stageAct = selectedMeta.f_StageAct,
            min = selectedMeta.f_MinFloor,
            max = selectedMeta.f_MaxFloor,
            unitList = new List<StageEditorUnitInfo>()
        };

        foreach (var e in enemyEntities)
        {
            data.unitList.Add(new StageEditorUnitInfo(e.f_UnitID, e.f_StarLevel, e.f_GridX, e.f_GridY));
        }

        return data;
    }
}
