# Ebonflow

오토배틀러와 로그라이크 덱빌더를 결합한 개인 프로젝트입니다. 엔진이 제공하는 기능이나 외부 에셋에 기대지 않고 핵심 시스템을 직접 구현하는 것을 목표로 삼았습니다.

| 항목 | 내용 |
|---|---|
| 개발 기간 | 2025년 1월부터 10월까지 |
| 커밋 | 322건, 1인 개발 |
| 규모 | C# 파일 161개, 디렉토리 59개, 인터페이스 20개 |
| 엔진 | Unity 6000.0.68f1, URP 2D |
| 언어 | C# |

이 저장소의 코드는 전부 직접 작성했습니다. 회사 프로젝트의 코드나 자산은 포함되어 있지 않습니다.

## 먼저 봐주셨으면 하는 코드

직접 구현한 시스템 네 가지입니다. 파일을 바로 열어보실 수 있게 경로를 링크로 걸어두었습니다.

| 시스템 | 핵심 파일 | 한 줄 |
|---|---|---|
| A* 길찾기 | [AStarAgent.cs](Assets/Scripts/AStarAlgorithm/AStarAgent.cs), [AStarGrid.cs](Assets/Scripts/AStarAlgorithm/AStarGrid.cs), [AStarAlgorithmManager.cs](Assets/Scripts/AStarAlgorithm/AStarAlgorithmManager.cs) | 격자 기반 경로 탐색을 인터페이스 3개로 분리해 구현 |
| 로그라이크 맵 생성 | [RoguelikeMapGenerator.cs](Assets/Scripts/RoguelikeMap/Generation/RoguelikeMapGenerator.cs) | 경로 생성에 실패하면 되돌리고 다시 시도하는 분기 맵 생성기 |
| 오브젝트 풀 | [PoolManager.cs](Assets/Scripts/Managers/PoolManager.cs), [Poolable.cs](Assets/Scripts/Common/Poolable.cs) | Unity Pool API 대신 직접 만든 풀 관리자 |
| 인게임 스테이지 에디터 | [StageEditorManager.cs](Assets/Scripts/StageEditor/Manager/StageEditorManager.cs), [StageSaveLoad.cs](Assets/Scripts/StageEditor/Config/StageSaveLoad.cs) | 게임 안에서 유닛 배치와 스테이지 데이터를 편집하는 도구 |

구조 설계를 보시려면 다음 세 파일을 권합니다.

| 주제 | 파일 | 한 줄 |
|---|---|---|
| 컴포넌트 합성 | [Unit.cs](Assets/Scripts/Unit/Entities/Unit.cs) | 상속 대신 인터페이스 3개 구현과 컴포넌트 5개 합성으로 구성한 유닛 |
| 상태 머신 | [IUnitState.cs](Assets/Scripts/Unit/StateMachines/IUnitState.cs), [UnitStateMachine.cs](Assets/Scripts/Unit/StateMachines/UnitStateMachine.cs) | Enter, Execute, Exit로 나눈 유닛 상태 6종 |
| 싱글톤 | [Singleton.cs](Assets/Scripts/Patterns/Singleton.cs) | 마커 인터페이스로 씬 전환 시 파괴 여부를 결정 |

## 목차

- [게임 소개](#게임-소개)
- [기술 스택](#기술-스택)
- [직접 구현한 시스템](#직접-구현한-시스템)
- [구조 설계](#구조-설계)
- [프로젝트 구조](#프로젝트-구조)
- [개발 기록](#개발-기록)
- [아쉬운 점과 다음 계획](#아쉬운-점과-다음-계획)
- [실행 방법](#실행-방법)

## 게임 소개

체스판 위에 유닛을 배치하면 자동으로 전투가 진행되는 오토배틀러에, 갈라지는 노드 맵을 따라 진행하는 로그라이크 구조를 얹었습니다. Team Fight Tactics와 Slay the Spire를 참고했습니다.

한 판의 흐름은 다음과 같습니다.

1. 노드 맵에서 다음에 갈 곳을 고릅니다
2. 상점에서 카드를 뽑아 유닛을 사고 배치합니다
3. 자동 전투가 진행됩니다
4. 승패에 따라 보상을 받고 다시 맵으로 돌아옵니다

## 기술 스택

| 분류 | 사용 기술 |
|---|---|
| 엔진 | Unity 6000.0.68f1, URP 2D |
| 언어 | C# |
| 비동기 | UniTask, 코루틴은 사용하지 않았습니다 |
| 연출 | DOTween |
| 저장 | Easy Save 3, 직렬화 래퍼를 따로 만들어 사용 |
| 데이터 | BGDatabase |
| 에셋 로드 | Addressables, 키를 추상화한 관리자를 따로 구현 |
| 입력 | New Input System, 플레이용과 에디터용 두 벌 |

## 직접 구현한 시스템

### 1. A* 길찾기

**왜 직접 만들었나**

유닛이 격자 칸 단위로 움직이는 오토배틀러입니다. NavMesh는 연속된 공간을 전제로 하고 격자 칸 점유 개념이 없어서, 같은 칸에 두 유닛이 들어가는 상황을 막기 어려웠습니다. 격자를 직접 다루는 편이 맞다고 판단했습니다.

**어떻게 만들었나**

책임을 셋으로 나눴습니다. 격자와 막힌 칸을 관리하는 부분, 개별 유닛이 경로를 따라가는 부분, 노드 데이터입니다. 그리고 알고리즘이 게임 규칙을 모르게 하려고 인터페이스 세 개를 두었습니다.

- `IAStarGridSettings` 격자 크기 같은 설정을 알고리즘에 전달합니다
- `IAStarPathPoint` 좌표를 가진 대상이라는 뜻입니다
- `IAStarPathFollower` 경로를 따라 움직일 수 있는 대상이라는 뜻입니다

유닛은 자신의 팀이나 현재 좌표를 알고리즘에게 직접 알려주지 않고, 이벤트로 상위에 물어봅니다. 그래서 길찾기 코드에는 팀이나 전투 규칙이 등장하지 않습니다.

```csharp
public class AStarAgent : MonoBehaviour, IAStarPathPoint, IAStarPathFollower
{
    public event Action<Vector2Int> OnMove;
    public event Func<TeamType> OnRequestTeamType;
    public event Action CrushOtherTeamAgent;
    public event Action OnPathCompleteAction;
    public event Func<Vector2Int> GetCurrentGridPositionAction;
    public event Action<Vector2Int> SetCurrentGridPositionAction;
    public event Action RequestEnterWaitState;
    public event Action RequestExitWaitState;

    public TeamType GetTeam() => OnRequestTeamType.Invoke();
}
```

**결과**

길찾기 모듈이 유닛이나 전투 시스템을 참조하지 않습니다. 나중에 스테이지 에디터에서 같은 격자를 재사용할 때 수정 없이 붙일 수 있었습니다.

파일: [AStarAlgorithm](Assets/Scripts/AStarAlgorithm)

### 2. 로그라이크 맵 생성기

**왜 직접 만들었나**

Slay the Spire 계열의 갈라지는 노드 맵은 생성 규칙이 공개되어 있지 않습니다. 에셋 스토어의 결과물은 분기 모양을 원하는 대로 조정하기 어려워서, 레퍼런스를 관찰해 규칙을 추론하고 직접 설계했습니다.

**어떻게 만들었나**

빈 격자를 만든 뒤 아래에서 위로 경로를 정해진 횟수만큼 그립니다. 경로를 그리다 막히면 그 회차에 만든 것만 되돌리고 다시 시도합니다. 정해진 횟수까지 실패하면 그 경로는 포기합니다. 마지막에 아무 노드도 없는 행을 정리해 최종 배치를 확정합니다.

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
PruneEmptyRows(grid);
```

행과 열 개수, 경로 수, 최대 재시도 횟수, 노드 종류별 가중치는 ScriptableObject로 빼서 코드를 고치지 않고 조정할 수 있게 했습니다.

**결과**

전체를 다시 만들지 않고 실패한 회차만 되돌리기 때문에, 조건이 까다로운 설정에서도 생성이 끝까지 진행됩니다. 맵 모양을 바꾸는 작업이 에디터에서 값 몇 개 고치는 일이 됐습니다.

파일: [RoguelikeMapGenerator.cs](Assets/Scripts/RoguelikeMap/Generation/RoguelikeMapGenerator.cs), [MapGenerationSettings.cs](Assets/Scripts/RoguelikeMap/Settings/MapGenerationSettings.cs)

### 3. 오브젝트 풀 관리자

**왜 직접 만들었나**

투사체와 이펙트가 짧은 시간에 많이 생겼다 사라집니다. Unity가 제공하는 풀 API 대신 직접 만든 이유는, 풀에서 꺼내고 되돌리는 시점에 초기화 규칙을 강제하고 싶었기 때문입니다.

**어떻게 만들었나**

`Poolable`을 구현한 대상만 풀에 들어갈 수 있게 하고, 종류별로 부모 오브젝트를 따로 두어 하이어라키에서 무엇이 몇 개 살아 있는지 바로 보이게 했습니다.

파일: [PoolManager.cs](Assets/Scripts/Managers/PoolManager.cs), [Poolable.cs](Assets/Scripts/Common/Poolable.cs)

### 4. 인게임 스테이지 에디터

**왜 만들었나**

전투 밸런스를 확인하려면 적 조합을 여러 번 바꿔봐야 하는데, 그때마다 유니티 에디터에서 프리팹을 고치고 다시 실행하는 것이 느렸습니다.

**어떻게 만들었나**

게임 안에서 유닛을 배치하고 저장하는 별도 씬을 만들었습니다. 저장한 스테이지 데이터는 실제 전투에서 그대로 불러옵니다. 입력도 플레이용과 에디터용을 나눠 두 벌로 관리합니다.

**결과**

적 조합을 바꾸는 데 걸리는 시간이 재실행 없이 몇 초로 줄었습니다. 게임 콘텐츠가 아니라 만드는 사람을 위한 도구를 처음 만들어 본 작업이었습니다.

파일: [StageEditor](Assets/Scripts/StageEditor)

## 구조 설계

### 유닛은 상속하지 않고 합성했습니다

유닛 종류가 늘어날 때 상속으로 가면 클래스가 계속 갈라집니다. 대신 하나의 유닛 클래스가 역할 인터페이스를 구현하고, 기능은 컴포넌트로 나눠 붙였습니다.

```csharp
public class Unit : MonoBehaviour, IUpdateObserver, IAttacker, IVictim
{
    public HealthComponent Health { get; private set; }
    public ManaComponent Mana { get; private set; }
    public UnitSaleComponent SaleComponent { get; private set; }
    private CombatComponent _combatComponent;
    private MovementComponent _movementComponent;
    private UnitStateMachine _stateMachine;
}
```

전투 시스템은 유닛 클래스를 모르고 `IAttacker`와 `IVictim`만 압니다.

파일: [Unit.cs](Assets/Scripts/Unit/Entities/Unit.cs), [Unit/Components](Assets/Scripts/Unit/Components)

### 상태는 진입, 유지, 이탈로 나눴습니다

대기, 정지, 이동, 공격, 스킬, 사망 여섯 상태를 각각의 클래스로 만들고 세 시점을 분리했습니다. 애니메이션 전환처럼 한 번만 일어나야 하는 일을 Enter와 Exit에 두어, 매 프레임 조건을 다시 검사하는 코드를 없앴습니다.

```csharp
public interface IUnitState
{
    void Enter(Unit unit);
    void Execute(Unit unit);
    void Exit(Unit unit);
}
```

파일: [Unit/StateMachines](Assets/Scripts/Unit/StateMachines)

### 싱글톤의 생존 범위를 마커 인터페이스로 정했습니다

씬이 바뀌어도 남아야 하는 관리자와 씬마다 새로 만들어야 하는 관리자가 섞여 있었습니다. 각 관리자가 스스로 `DontDestroyOnLoad`를 부르면 규칙이 흩어지므로, 빈 인터페이스 하나를 표식으로 두고 생성 시점에 한 곳에서 판단하게 했습니다.

```csharp
public interface IDonDestroy { }

private static T CreateInstance()
{
    var go = new GameObject() { name = $"[{typeof(T).Name}]" };
    var instance = go.AddComponent<T>();
    if (instance.GetComponent<IDonDestroy>() != null)
        DontDestroyOnLoad(go);
    return instance;
}
```

파일: [Singleton.cs](Assets/Scripts/Patterns/Singleton.cs)

## 프로젝트 구조

```
Assets/Scripts/
  AStarAlgorithm/     격자 기반 A 스타 길찾기
  AutoBattle/         오토배틀 진행, 벤치와 로스터, 보상
  CombatSystem/       공격과 피격, 사거리 판정, 스킬 실행
  Common/             우선순위 큐, 풀 대상 표식, 상수
  DeckSystem/         카드 덱, 티어별 뽑기 확률
  EasySave3/          저장 직렬화 래퍼
  Input/              플레이용과 에디터용 입력
  Interface/          업데이트 옵저버, 격자 관리자 계약
  Managers/           풀, UI, 에셋 로드, 씬 로드, 업데이트
  Patterns/           싱글톤과 마커 인터페이스
  Projectile/         투사체
  RoguelikeMap/       맵 생성, 데이터 변환, 저장, 화면 표시
  SceneLoadSystem/    씬 전환
  SkillSystem/        스킬 정의와 저장소
  StageEditor/        인게임 스테이지 편집 도구
  Unit/               유닛 엔티티, 컴포넌트, 상태 머신, 스탯
  UI/                 화면
```

## 개발 기록

2025년 1월에 시작해 10월까지 316건을 커밋했습니다. 이후에는 회사 프로젝트에 집중하면서 산발적으로만 손을 댔습니다.

| 시기 | 커밋 |
|---|---|
| 2025년 1월 | 16 |
| 2025년 2월 | 34 |
| 2025년 3월 | 68 |
| 2025년 4월 | 26 |
| 2025년 5월 | 40 |
| 2025년 6월 | 9 |
| 2025년 7월 | 51 |
| 2025년 8월 | 37 |
| 2025년 9월 | 15 |
| 2025년 10월 | 20 |

## 아쉬운 점과 다음 계획

**미완성입니다.** 핵심 시스템은 동작하지만 콘텐츠 분량과 밸런스가 부족하고, 2025년 10월 이후 개발이 멈춰 있습니다. 완성된 게임이 아니라 시스템 구현 기록으로 봐주시면 좋겠습니다.

**테스트 코드가 없습니다.** 혼자 만들면서 실행해 보는 것으로 검증을 대신했습니다. 맵 생성기처럼 입력과 출력이 분명한 부분은 테스트를 붙일 수 있었는데 하지 않았습니다.

**브랜치 운영이 정리되지 않았습니다.** 오랫동안 develop에서만 작업하고 main을 비워둔 채 두었습니다. 혼자 하는 프로젝트라 문제가 없었지만, 기본 브랜치가 최신이 아닌 상태는 다른 사람이 볼 때 혼란스럽습니다.

이 세 가지는 현재 개발 중인 다음 개인 프로젝트에서 도메인과 응용, 표현 계층을 나누고 테스트를 붙이는 방식으로 고쳐 나가고 있습니다.

## 실행 방법

1. Unity Hub에서 6000.0.68f1을 설치합니다
2. 이 저장소를 클론하고 Unity Hub에 프로젝트로 추가합니다
3. `Assets/Scenes/IntroScene.unity`를 엽니다

스테이지 에디터만 보시려면 `Assets/Scenes/StageEditorScene.unity`를 여시면 됩니다.

## 연락처

- 이메일: alsrl7538@gmail.com
- GitHub: [GGumBug](https://github.com/GGumBug)
- 포트폴리오: [minki-portfolio](https://github.com/GGumBug/minki-portfolio)
