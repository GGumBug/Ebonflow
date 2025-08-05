using BansheeGz.BGDatabase;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StageEditor
{
    public class StageSaveLoad
    {
        public event Action<int, int, Vector2Int> RequestSpawnStageSaveUnit;

        public void SaveStageData(List<StageSaveUnit> stageSaveUnits, int id, int act, int min, int max)
        {
            if (stageSaveUnits.Count == 0)
            {
                Debug.LogWarning("저장할 스테이지 데이터가 없습니다.");
                return;
            }

            // 1) Stages 테이블: 기존 레코드 찾기
            var existingStage = DB_Stages.FindEntity(s => s.f_StageID == id);

            if (existingStage != null)
            {
                // 덮어쓰기: 필드 업데이트
                existingStage.f_StageAct = act;
                existingStage.f_MinFloor = min;
                existingStage.f_MaxFloor = max;
                existingStage.f_name = $"Stage_{id}";
            }
            else
            {
                // 새로 생성
                DB_Stages.NewEntity(stage =>
                {
                    stage.f_StageID = id;
                    stage.f_StageAct = act;
                    stage.f_MinFloor = min;
                    stage.f_MaxFloor = max;
                    stage.f_name = $"Stage_{id}";
                });
            }

            // 2) StageEnemys 테이블: 기존 같은 stageID 레코드 모두 삭제
            var existingEnemys = DB_StageEnemys.FindEntities(e => e.f_StageID == id);
            foreach (var enemy in existingEnemys)
            {
                enemy.Delete();
            }

            // 3) StageEnemys 테이블: 새 적 정보 추가
            foreach (var unit in stageSaveUnits)
            {
                var info = unit.GetStageEditorUnitInfo();
                DB_StageEnemys.NewEntity(enemy =>
                {
                    enemy.f_StageID = id;
                    enemy.f_UnitID = info.unitID;
                    enemy.f_GridX = info.gridX;
                    enemy.f_GridY = info.gridY;
                    enemy.f_StarLevel = info.starLevel;
                });
            }

            Debug.Log($"Stage({id}) 메타 {(existingStage != null ? "업데이트" : "생성")} 및 {stageSaveUnits.Count}개의 적 정보가 DB에 저장되었습니다.");
        }

        public void LoadStageData(List<StageSaveUnit> stageSaveUnits, int id, int act, int min, int max)
        {
            // 1) Stages 테이블에서 메타 조회
            var stageMeta = DB_Stages.FindEntity(s => s.f_StageID == id);
            if (stageMeta == null)
            {
                Debug.LogError($"Stage ID {id} 데이터가 DB에 없습니다.");
                return;
            }

            // 2) 기존 Spawn된 유닛 모두 제거
            for (int i = stageSaveUnits.Count - 1; i >= 0; --i)
                stageSaveUnits[i].Destroyed();
            stageSaveUnits.Clear();

            // 3) StageEnemys 테이블에서 해당 StageID의 적 정보 조회
            var enemyMetas = DB_StageEnemys.FindEntities(e => e.f_StageID == id);
            foreach (var enemy in enemyMetas)
            {
                // 드래그 앤 드롭용 Spawn 요청
                RequestSpawnStageSaveUnit.Invoke(
                    enemy.f_UnitID,
                    enemy.f_StarLevel,
                    new Vector2Int(enemy.f_GridX, enemy.f_GridY)
                );
            }

            Debug.Log($"Stage({id}) 데이터 로드 완료: Act={stageMeta.f_StageAct}, Floors={stageMeta.f_MinFloor}~{stageMeta.f_MaxFloor}, Enemys={enemyMetas.Count}");
        }
    }
}