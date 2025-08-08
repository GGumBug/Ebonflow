using UnityEngine;
using System.Collections.Generic;
using System;
using AutoBattle;

public class AStarAlgorithmManager : Singleton<AStarAlgorithmManager>
{
    [SerializeField] private bool allowDiagonal = true;
    [SerializeField] private bool dontCrossCorner = true;

    private const int COST_STRAIGHT = 10;   // D
    private const int COST_DIAGONAL = 14;   // D2
    private const int TARGET_COUNT_THRESHOLD = 20;

    private bool _allowDiagonal, _dontCrossCorner;

    private AStarNode _startNode, _targetNode, _currentNode;
    private HashSet<AStarNode> _closedSet;
    private PriorityQueue<AStarNode> _openNodeQueue;

    // 탐색 중 변경된 노드만 리셋해 성능/안전 확보
    private readonly List<AStarNode> _touchedNodes = new();
    private readonly HashSet<AStarNode> _initializedThisSearch = new();

    public Func<IReadOnlyCollection<Unit>> OnRequestAllyUnits;
    public Func<IReadOnlyCollection<Unit>> OnRequestEnemyUnits;

    public AStarGrid Grid { get; private set; }

    public void InitializeGrid(IAStarGridSettings gridSettings)
    {
        if (Grid == null)
            Grid = gameObject.AddComponent<AStarGrid>();

        Grid.Init(gridSettings);
    }

    public List<AStarNode> GetPath(AStarAgent startPoint, AStarAgent targetPoint)
    {
        return FindPath(startPoint, targetPoint, out _); // 비용은 필요 시 out 사용
    }

    public List<AStarNode> GetPath(AStarAgent startPoint)
    {
        if (OnRequestAllyUnits == null || OnRequestEnemyUnits == null)
        {
            Debug.LogWarning("GetPath(start): Unit 요청 델리게이트가 설정되지 않았습니다.");
            return null;
        }

        var targetUnits = startPoint.GetTeam() == TeamType.Ally
            ? OnRequestEnemyUnits.Invoke()
            : OnRequestAllyUnits.Invoke();

        return FindClosestTargetPath(startPoint, targetUnits);
    }

    // ========================= 내부 구현 =========================

    private List<AStarNode> FindPath(AStarAgent startAgent, AStarAgent targetAgent, out int finalCost)
    {
        finalCost = int.MaxValue;

        if (startAgent == null || targetAgent == null || Grid == null)
            return null;

        Vector2Int startWorld = startAgent.CurrentGridPosition;  // 월드좌표(정수)
        Vector2Int targetWorld = targetAgent.CurrentGridPosition;

        // 같은 칸이면 빈 경로 반환
        if (startWorld == targetWorld)
        {
            if (Grid.TryGetNodeAtWorld(startWorld, out var same)) // <-- Grid에 추가해주세요
            {
                finalCost = 0;
                return new List<AStarNode> { same };
            }
            return null;
        }

        // 탐색 옵션 적용
        _allowDiagonal = allowDiagonal;
        _dontCrossCorner = dontCrossCorner;

        // 이전 탐색에서 만진 노드들 리셋
        ResetTouchedNodes();

        // 엔드포인트 잠금 해제(탐색 중 start/target은 통과 가능)
        Grid.SetPathEndpointsLockState(false, startWorld, targetWorld);

        if (!Grid.TryGetNodeAtWorld(startWorld, out _startNode) ||
            !Grid.TryGetNodeAtWorld(targetWorld, out _targetNode))
        {
            Debug.LogWarning("FindPath: 시작 또는 목표 노드를 가져올 수 없습니다.");
            Grid.SetPathEndpointsLockState(true, startWorld, targetWorld);
            return null;
        }

        // 시작 노드 초기화
        InitializeNodeForSearch(_startNode);
        _startNode.G = 0;
        _startNode.H = HeuristicCost(_startNode, _targetNode);
        _startNode.ParentNode = null;

        _openNodeQueue = new PriorityQueue<AStarNode>(5, SortOrder.Ascending);
        _closedSet = new HashSet<AStarNode>();

        _openNodeQueue.Enqueue(_startNode, _startNode.F);

        // A*
        while (_openNodeQueue.Count > 0)
        {
            _currentNode = _openNodeQueue.Dequeue();
            _closedSet.Add(_currentNode);

            if (_currentNode == _targetNode)
            {
                var path = ConstructFinalPath();
                finalCost = _targetNode.F;
                Grid.SetPathEndpointsLockState(true, startWorld, targetWorld);
                return path;
            }

            EvaluateAdjacentNodes(_currentNode);
        }

        // 경로 실패
        Grid.SetPathEndpointsLockState(true, startWorld, targetWorld);
        return null;
    }

    private List<AStarNode> FindClosestTargetPath(AStarAgent startPoint, IReadOnlyCollection<Unit> targetPoints)
    {
        if (startPoint == null || targetPoints == null || targetPoints.Count == 0)
            return null;

        // 타겟 수가 많으면 맨해튼 휴리스틱으로 상위 N개만
        if (targetPoints.Count > TARGET_COUNT_THRESHOLD)
            targetPoints = FilterTargetsByHeuristic(startPoint, targetPoints);

        var pathQueue = new PriorityQueue<List<AStarNode>>(5, SortOrder.Ascending);

        foreach (var target in targetPoints)
        {
            if (target?.Agent == null) continue;

            var path = FindPath(startPoint, target.Agent, out int cost);
            if (path != null && path.Count > 0)
                pathQueue.Enqueue(path, cost);
        }

        if (pathQueue.Count == 0)
        {
            Debug.Log($"{startPoint.name}의 경로를 찾을 수 없습니다.");
            return null;
        }

        return pathQueue.Dequeue();
    }

    private HashSet<Unit> FilterTargetsByHeuristic(AStarAgent startPoint, IReadOnlyCollection<Unit> targets)
    {
        var q = new PriorityQueue<Unit>(targets.Count, SortOrder.Ascending);
        foreach (var t in targets)
        {
            if (t?.Agent == null) continue;
            int dist = Mathf.Abs(startPoint.PathPoint.x - t.Agent.PathPoint.x) +
                       Mathf.Abs(startPoint.PathPoint.y - t.Agent.PathPoint.y);
            q.Enqueue(t, dist);
        }

        var picked = new HashSet<Unit>();
        int count = Mathf.Min(TARGET_COUNT_THRESHOLD, q.Count);
        for (int i = 0; i < count; i++)
            picked.Add(q.Dequeue());

        return picked;
    }

    private void EvaluateAdjacentNodes(AStarNode node)
    {
        // 대각선 먼저(선택), 그 다음 직선
        if (_allowDiagonal)
        {
            foreach (var dir in Constants.DIAGONAL_DIRECTIONS)
                TryAddToOpenQueue(node.X + dir.x, node.Y + dir.y);
        }
        foreach (var dir in Constants.ORTHOGONAL_DIRECTIONS)
            TryAddToOpenQueue(node.X + dir.x, node.Y + dir.y);
    }

    private void TryAddToOpenQueue(int worldX, int worldY)
    {
        // 바운드/노드 조회 — Grid에 TryGetNodeAtWorld을 하나 추가하는 것을 권장
        if (!Grid.TryGetNodeAtWorld(new Vector2Int(worldX, worldY), out var neighborNode))
        {
            // 만약 TryAPI 없이 GetNodeAt만 있다면:
            // try { neighborNode = Grid.GetNodeAt(worldX, worldY); } catch { return; }
            return;
        }

        // 블록/닫힘 검사
        if (neighborNode.GetBlock || _closedSet.Contains(neighborNode))
            return;

        // 코너 처리: 양옆 모두 막혀 있으면 금지 (dontCrossCorner와 무관하게 흔히 막음)
        if (_allowDiagonal && IsDiagonal(worldX, worldY, _currentNode))
        {
            if (!TryGetOrthogonalNeighbors(worldX, worldY, _currentNode, out var adjY, out var adjX))
                return;

            // 양옆 모두 막힘 → 금지
            if (adjY.GetBlock && adjX.GetBlock) return;

            // dontCrossCorner 옵션이면 하나만 막혀도 금지
            if (_dontCrossCorner && (adjY.GetBlock || adjX.GetBlock)) return;
        }

        InitializeNodeForSearch(neighborNode);

        int moveCost = _currentNode.G + CalculateMoveCost(_currentNode.X, _currentNode.Y, worldX, worldY);

        if (moveCost < neighborNode.G || !_openNodeQueue.Contains(neighborNode))
        {
            neighborNode.G = moveCost;
            neighborNode.H = HeuristicCost(neighborNode, _targetNode);
            neighborNode.ParentNode = _currentNode;

            if (!_openNodeQueue.Contains(neighborNode))
                _openNodeQueue.Enqueue(neighborNode, neighborNode.F);
        }
    }

    private bool IsDiagonal(int x, int y, AStarNode from) => !(from.X == x || from.Y == y);

    private bool TryGetOrthogonalNeighbors(int worldX, int worldY, AStarNode from, out AStarNode adjY, out AStarNode adjX)
    {
        adjY = null; adjX = null;

        // (from.X, worldY), (worldX, from.Y) 두 노드
        if (!Grid.TryGetNodeAtWorld(new Vector2Int(from.X, worldY), out adjY)) return false;
        if (!Grid.TryGetNodeAtWorld(new Vector2Int(worldX, from.Y), out adjX)) return false;

        return true;
    }

    private void InitializeNodeForSearch(AStarNode node)
    {
        if (_initializedThisSearch.Add(node))
        {
            node.G = int.MaxValue;
            node.H = 0;
            node.ParentNode = null;
            _touchedNodes.Add(node);
        }
    }

    private int HeuristicCost(AStarNode a, AStarNode b)
    {
        int dx = Mathf.Abs(a.X - b.X);
        int dy = Mathf.Abs(a.Y - b.Y);

        if (_allowDiagonal)
        {
            // 옥타일 거리: D*(dx+dy) + (D2 - 2D)*min(dx,dy)
            int min = Mathf.Min(dx, dy);
            return COST_STRAIGHT * (dx + dy) + (COST_DIAGONAL - 2 * COST_STRAIGHT) * min;
        }
        else
        {
            // 맨해튼
            return COST_STRAIGHT * (dx + dy);
        }
    }

    // 이동 비용 계산 (상하좌우/대각선)
    private int CalculateMoveCost(int fromX, int fromY, int toX, int toY)
    {
        return (fromX == toX || fromY == toY) ? COST_STRAIGHT : COST_DIAGONAL;
    }

    private List<AStarNode> ConstructFinalPath()
    {
        var path = new List<AStarNode>();
        var node = _targetNode;
        while (node != null && node != _startNode)
        {
            path.Add(node);
            node = node.ParentNode;
        }
        if (_startNode != null) path.Add(_startNode);
        path.Reverse();
        return path;
    }

    private void ResetTouchedNodes()
    {
        if (_touchedNodes.Count == 0) { _initializedThisSearch.Clear(); return; }

        foreach (var n in _touchedNodes)
        {
            n.G = int.MaxValue;
            n.H = 0;
            n.ParentNode = null;
        }
        _touchedNodes.Clear();
        _initializedThisSearch.Clear();
    }

    public void RegisteBattleRoster(IBattleRoster battleRoster) => Grid?.RegisterBattleRoster(battleRoster);
}
