# Ebonflow

> **Auto Chess + Roguelike Deckbuilder** 하이브리드 (TFT/HSBG + Slay the Spire 류) 개인 프로토타입.
> Unity 6 / 2D Mobile / 자체 A* + Roguelike Map + Pool Manager + Stage Editor.

[한국어 README](#한국어) · [English README](#english)

---

## 한국어

### 게임 컨셉

- **장르**: 오토배틀러 + 로그라이크 덱빌더
- **메인 루프**: Map 노드 선택 → AutoBattle 진입 → 자동 전투 (Victory/Defeat) → 보상 (이자/연승) → Map 복귀
- **레퍼런스**: TFT (Team Fight Tactics) · Hearthstone Battlegrounds · Slay the Spire (노드맵)
- **시그니처**: 자체 A* path finding · 자체 Roguelike Map Generator (시드 + 가중치 + 경로 검증·롤백) · 자체 인게임 Stage Editor

### 기술 스택

- **Engine**: Unity **6000.0.49f1** (Unity 6, Mobile 2D Template, URP 17.0.3)
- **언어**: C# (모든 게임 로직)
- **Async**: [Cysharp UniTask](https://github.com/Cysharp/UniTask) — 코루틴 사용 X
- **Tweening**: [DOTween](http://dotween.demigiant.com/) (Demigiant)
- **Save/Load**: [Easy Save 3](https://moodkie.com/easysave/) — 자체 `ES3SerializerBase` 래퍼
- **Data**: [BGDatabase](https://www.bansheegz.com/BGDatabase/) (BansheeGz) + 자체 `BGCodeGenerate`
- **Asset Loading**: Unity Addressables — 자체 `AddressableManager` (UniTask + AddressableKey 추상화)
- **Input**: New Input System — `PlayerInput` (16KB) + `StageEditorInput` (16KB) 이중 input map

### 아키텍처

DDD-ish 계층 분리 + Component 합성 + Interface 다중 구현 + State Machine.

```
Assets/Scripts/
├── AStarAlgorithm/        # 자체 A* (Manager + Grid + Agent + Interface)
├── AutoBattle/            # 오토체스 전투
│   ├── Context/           (PlayerDataContext, SceneDataContext)
│   ├── Controllers/       (AutoBattleStateController)
│   ├── Domain/            (UnitBench, BattleRoster — DDD)
│   ├── Input/             (UnitDragController, DefaultPlacementService)
│   ├── Interface/         (IBattleRoster, ISoulCoinRewardService)
│   ├── Managers/          (AutoBattleManager, UnitManager, DataManager)
│   ├── SaveLoad/
│   └── UI/
├── CombatSystem/
│   ├── Casting/           (ICastValidator)
│   ├── Domain/            (RangeDetector)
│   ├── Executor/          (AreaSkillExecutor + AreaDebugDrawer)
│   ├── Interface/         (IAttacker, IVictim, IRangeDetector, ISkillExecutor)
│   └── Manager/           (CombatManager)
├── Common/                (PriorityQueue 자체 구현)
├── DeckSystem/            (Deck 9장 + CardData + TierBasedCardPicker + CardDrawManager)
├── EasySave3/             (ES3SerializerBase 커스텀 래퍼)
├── Input/                 (PlayerInput · StageEditorInput)
├── Interface/             (IUpdateObserver, ILateUpdateObserver, IGridManager)
├── Managers/              (PoolManager, UIManager, AddressableManager, SceneLoadManager)
├── Patterns/              (Singleton<T> + IDonDestroy 마커)
├── Player/
├── Projectile/            (Projectile, ProjectileManager)
├── RoguelikeMap/
│   ├── Generation/        (RoguelikeMapGenerator — 시드 + 가중치 + 검증·롤백)
│   ├── Models/            (MapDataMapper, MapDataSerializer, MapDataContext)
│   ├── Settings/          (MapGenerationSettings ScriptableObject)
│   ├── Utils/             (LocationWeightUtil)
│   └── Views/             (UIMapView, NodeView)
├── Scenes/                (AutoBattleScene)
├── SkillSystem/Config/
├── StageEditor/           # 자체 인게임 스테이지 디자인 툴
│   ├── Config/            (StageSaveLoad)
│   ├── Manager/
│   └── UI/                (StageSpawnUnitPanel)
├── Unit/
│   ├── Combat/
│   ├── Components/        (HealthComponent, ManaComponent, CombatComponent, MovementComponent, UnitSaleComponent)
│   ├── Entities/          (Unit 12KB — 핵심 엔티티)
│   ├── Enums/             (TeamType, UnitClass, UnitOrigin, UnitTier)
│   ├── Model/             (UnitModel, UnitAnimationController, UnitModelOffsetAdjuster)
│   ├── Spawners/
│   ├── StateMachines/     (UnitStateMachine + IUnitState — 6 상태)
│   └── Stats/             (UnitStats, UnitStatData, UnitStatRepository)
└── Debug/
```

**규모**: 163 .cs 파일 / 50+ 디렉토리 / 20+ 인터페이스

### 핵심 코드 하이라이트

#### 1. 자체 Singleton<T> + IDonDestroy 마커 인터페이스
```csharp
public interface IDonDestroy { }  // 마커 인터페이스

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<T>() ?? CreateInstance();
            return _instance;
        }
    }

    static T CreateInstance()
    {
        var go = new GameObject() { name = $"[{typeof(T).Name}]" };
        var instance = go.AddComponent<T>();
        if (instance.GetComponent<IDonDestroy>() != null)
            DontDestroyOnLoad(go);
        return instance;
    }
}
```
→ 마커 인터페이스로 `DontDestroyOnLoad` 자동 결정.

#### 2. Unit — 인터페이스 다중 구현 + Component 합성
```csharp
public class Unit : MonoBehaviour, IUpdateObserver, IAttacker, IVictim
{
    public HealthComponent Health { get; private set; }
    public ManaComponent Mana { get; private set; }
    public UnitSaleComponent SaleComponent { get; private set; }
    private CombatComponent _combatComponent;
    private MovementComponent _movementComponent;
    private UnitStateMachine _stateMachine;
    // ...
}
```
→ 단일 Unit 클래스가 3 인터페이스 + 5+ Component. **느슨한 결합**.

#### 3. UnitStateMachine — 6 상태 (Wait·Idle·Walk·Attack·ActiveSkill·Dead)
```csharp
public interface IUnitState
{
    void Enter(Unit unit);    // 진입 시 1회
    void Execute(Unit unit);  // 매 프레임
    void Exit(Unit unit);     // 이탈 시 1회
}
```
6 State 클래스 (`WaitState`, `IdleState`, `WalkState`, `AttackState`, `ActiveSkillState`, `DeadState`). 각 Enter/Execute/Exit 분리.

#### 4. AutoBattleManager — Victory/Defeat 후처리 우선순위 큐
```csharp
StateController.VictoryEntered.Add(() => ctx.UpdateStreak(true), 0);
StateController.VictoryEntered.Add(() => _rewardService.ApplyInterest(), 0);
StateController.VictoryEntered.Add(() => ctx.Save(), 1);  // priority 1
StateController.VictoryEntered.Add(async () => await SceneLoadManager.LoadSceneAsync<MapScene>(), 2);
```
→ 우선순위 기반 후처리 hook. 연승 → 이자 보상 → 저장 → 씬 전환.

#### 5. AStarAgent — Event 기반 의존성 역전
6 event 노출 (`OnMove`, `OnRequestTeamType`, `CrushOtherTeamAgent`, `OnPathCompleteAction`, `RequestEnterWaitState`, `RequestExitWaitState`). Manager 가 Agent 를 직접 참조 안 하고 event 로 통신.

#### 6. RoguelikeMapGenerator — 시드 + 검증 + 롤백
```csharp
for (int gen = 0; gen < _settings.pathGenerationCount; gen++)
{
    bool success = false;
    int tries = 0;
    while (!success && tries++ < _settings.maxAttemptsPerPath)
    {
        success = TryGenerateSinglePath(grid, paths, gen);
        if (!success)
            RollbackGeneration(grid, paths, gen);
    }
}
```
→ 경로 생성 실패 시 롤백 + 재시도. 결정론적 (시드 주입 가능).

### 개발 상황

- **시작**: 2025-01-12
- **활발한 개발**: 2025-09 ~ 2025-10 (스킬·투사체·모델·애니메이션·마나)
- **마지막 commit**: 2026-03-15 (`feature/skill` 브랜치, "버젼 체인지")
- **상태**: 현재 미완 (다른 프로젝트 전환). 핵심 시스템 (A* / Map Generator / AutoBattle Loop / Skill System) 구현 완료.
- **브랜치 워크플로우**: `main` (템플릿만) · `develop` (활발) · `feature/modelupdate` · `feature/skill` (가장 최신)

### 설치

1. Unity Hub 에서 **Unity 6000.0.49f1** 설치
2. 본 레포 clone → Unity Hub 에서 프로젝트 추가
3. **`develop` 브랜치 체크아웃** (게임 코드는 develop 에 있음, main 은 빈 템플릿)
4. Unity 에서 `Assets/Scenes/Intro.unity` 열기

### 외부 의존성

| 패키지 | 버전 | 용도 |
|---|---|---|
| `com.unity.addressables` | 2.2.2 | 에셋 동적 로드 |
| `com.unity.inputsystem` | 1.11.2 | 입력 |
| `com.unity.render-pipelines.universal` | 17.0.3 | URP 2D |
| `com.unity.feature.2d` | 2.0.1 | 2D 패키지 묶음 |
| `com.unity.feature.mobile` | 1.0.0 | 모바일 패키지 묶음 |
| `com.unity.timeline` | 1.8.7 | 타임라인 |
| `com.unity.visualscripting` | 1.9.5 | 비주얼 스크립팅 |
| `com.unity.multiplayer.center` | 1.0.0 | 멀티플레이 |

추가 (Asset Store / Plugin):
- Cysharp UniTask
- Demigiant DOTween
- Moodkie Easy Save 3
- BansheeGz BGDatabase

### Author

[**민기 (Minki)**](https://github.com/GGumBug) — 게임 클라이언트 엔지니어 (302Lab 재직중)
- Email: alsrl7538@gmail.com
- Portfolio: [github.com/GGumBug/minki-portfolio](https://github.com/GGumBug/minki-portfolio) (private — 이직 시 public 전환 예정)

---

## English

**Auto Chess + Roguelike Deckbuilder** hybrid (TFT/HSBG + Slay the Spire-style) personal prototype.
Unity 6 / 2D Mobile / Custom A\* + Roguelike Map Generator + Pool Manager + In-game Stage Editor.

### Genre & Loop
Auto-battler combined with roguelike node-map progression. Main loop: Map node → AutoBattle → Victory/Defeat → Reward (interest/streak) → return to Map.

### Tech Stack
Unity 6000.0.49f1 (URP 2D Mobile) · C# · UniTask (no Coroutines) · DOTween · Easy Save 3 · BGDatabase · Addressables · New Input System.

### Architecture
DDD-ish layered separation + component composition + multi-interface implementation + state machine. 163 C# files / 50+ directories / 20+ interfaces.

Key custom systems:
- **Custom A\* path finding** with Manager/Grid/Agent + interface segregation + 6 events for inversion of control
- **Custom Roguelike Map Generator** with seed + weighted node placement + path validation & rollback
- **Custom Pool Manager** (no Unity Pool API, custom Stack + Singleton + Poolable + Root grouping)
- **Custom in-game Stage Editor** for designing maps within the game itself

### Status
Active development Sep–Oct 2025. Last commit Mar 2026 on `feature/skill` branch. Currently paused — core systems complete (A\* / Map Gen / AutoBattle loop / Skill system).

### Setup
1. Install Unity 6000.0.49f1 via Unity Hub
2. Clone this repo, add to Unity Hub
3. **Checkout `develop` branch** (game code lives on develop, main has only the template)
4. Open `Assets/Scenes/Intro.unity`

---

*Repo currently private. Will be made public when actively job-hunting.*
