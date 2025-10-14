using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoBattle
{
    public class AutoBattleDataManager : Singleton<AutoBattleDataManager>, IDonDestroy
    {
        public AutoBattleStageDataContext AutoBattleSceneDataContext { get; private set; }
        public AutoBattlePlayerDataContext AutoBattlePlayerDataContext { get; private set; }

        private readonly Dictionary<AddressableKey, AnimatorOverrideController> _unitModelAnimatorDict =
        new Dictionary<AddressableKey, AnimatorOverrideController>();

        public async Task Setup()
        {
            AutoBattleSceneDataContext = new AutoBattleStageDataContext();
            AutoBattlePlayerDataContext = new AutoBattlePlayerDataContext();
            await LoadAsyncUnitModelAnimators();
        }

        public bool DeleteData()
        {
            bool sceneDataResult = AutoBattleSceneDataContext.Delete();
            bool battleDataResult = AutoBattlePlayerDataContext.Delete();

            if (sceneDataResult && battleDataResult)
                return true;

            return false;
        }

        private async UniTask LoadAsyncUnitModelAnimators()
        {
            var allEntities = DB_Units.FindEntities(e => true);
            var loadTasks = new List<UniTask>();

            foreach (var entity in allEntities)
            {
                // 1. 순차적 로드 대신, 로드 작업(Task)만 목록에 추가 (병렬 로드 준비)
                if (_unitModelAnimatorDict.ContainsKey(entity.f_ModelKey)) continue;

                // Addressable 로드 시작 및 UniTask로 변환
                AnimatorOverrideController controller = null;

                controller = await AddressableManager.Instance.Load<AnimatorOverrideController>(entity.f_ModelKey);

                if (controller == null)
                {
                    Debug.Log($"{entity.f_ModelKey} 유닛 애니메이션 컨트롤러 로드 실패");
                }
                else
                {
                    if (!_unitModelAnimatorDict.ContainsKey(entity.f_ModelKey))
                    {
                        _unitModelAnimatorDict.Add(entity.f_ModelKey, controller);
                    }
                }
            }
        }

        public AnimatorOverrideController GetUnitModelAnimator(AddressableKey modelKey)
        {
            if (_unitModelAnimatorDict.ContainsKey(modelKey))
            {
                return _unitModelAnimatorDict[modelKey];
            }
            else
            {
                Debug.LogError($"'{modelKey}' 애니메이터 컨트롤러 Get 실패");
                return null;
            }
        }
    }
}