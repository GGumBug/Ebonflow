using System.Collections.Generic;
using System;

namespace BT
{
    /// <summary>
    /// 여러 자식 노드를 보유하는 행동트리의 추상 컴포지트 노드입니다.
    /// 구체 노드(Sequence, Selector)가 이 클래스를 상속하여 실행 방식을 정의합니다.
    /// </summary>
    /// <remarks>
    /// 자식 노드 컬렉션은 생성자 주입으로만 채워지며, 실행 순서는 추가된 순서를 따릅니다.
    /// </remarks>
    /// <seealso cref="IBehaviorNode"/>
    public abstract class CompositeNode : IBehaviorNode
    {
        /// <summary>이 노드가 관리하는 자식 노드 목록입니다.</summary>
        protected readonly List<IBehaviorNode> _children = new();

        /// <summary>
        /// 자식 노드들을 받아 컴포지트 노드를 초기화합니다.
        /// </summary>
        /// <param name="children">실행 대상 자식 노드들.</param>
        public CompositeNode(params IBehaviorNode[] children) => _children.AddRange(children);

        /// <summary>
        /// 노드를 실행합니다. 구체 컴포지트 노드에서 실행 정책을 구현합니다.
        /// </summary>
        /// <returns>노드의 실행 결과(<see cref="NodeState"/>).</returns>
        public abstract NodeState Tick();
    }

    /// <summary>
    /// 시퀀스 노드입니다. 자식들을 앞에서부터 차례로 실행하며,
    /// 하나라도 <see cref="NodeState.Failure"/> 또는 <see cref="NodeState.Running"/>을 반환하면 즉시 중단하고 그 상태를 반환합니다.
    /// 모든 자식이 <see cref="NodeState.Success"/>를 반환하면 <see cref="NodeState.Success"/>를 반환합니다.
    /// </summary>
    /// <seealso cref="CompositeNode"/>
    public sealed class SequenceNode : CompositeNode
    {
        /// <summary>
        /// 자식 노드들을 받아 시퀀스 노드를 초기화합니다.
        /// </summary>
        /// <param name="children">차례로 실행할 자식 노드들.</param>
        public SequenceNode(params IBehaviorNode[] children) : base(children) { }

        /// <inheritdoc/>
        public override NodeState Tick()
        {
            foreach (var child in _children)
            {
                var state = child.Tick();
                if (state != NodeState.Success) return state; // Failure/Running 전파
            }
            return NodeState.Success;
        }
    }

    /// <summary>
    /// 셀렉터 노드입니다. 자식들을 앞에서부터 차례로 실행하며,
    /// 첫 번째로 <see cref="NodeState.Success"/> 또는 <see cref="NodeState.Running"/>을 반환하는 자식의 상태를 즉시 반환합니다.
    /// 모든 자식이 <see cref="NodeState.Failure"/>를 반환하면 <see cref="NodeState.Failure"/>를 반환합니다.
    /// </summary>
    /// <seealso cref="CompositeNode"/>
    public sealed class SelectorNode : CompositeNode
    {
        /// <summary>
        /// 자식 노드들을 받아 셀렉터 노드를 초기화합니다.
        /// </summary>
        /// <param name="children">우선순위 순으로 평가할 자식 노드들.</param>
        public SelectorNode(params IBehaviorNode[] children) : base(children) { }

        /// <inheritdoc/>
        public override NodeState Tick()
        {
            foreach (var child in _children)
            {
                var state = child.Tick();
                if (state != NodeState.Failure) return state; // Success/Running 전파
            }
            return NodeState.Failure;
        }
    }

    /// <summary>
    /// 조건 노드입니다. 주어진 조건 델리게이트를 평가하여
    /// 참이면 <see cref="NodeState.Success"/>를, 거짓이면 <see cref="NodeState.Failure"/>를 반환합니다.
    /// 이 노드는 <see cref="NodeState.Running"/> 상태를 사용하지 않습니다.
    /// </summary>
    /// <remarks>
    /// 비싼 연산(레이캐스트 등)을 포함하는 조건은 캐싱/쿨다운 등을 고려하십시오.
    /// </remarks>
    public sealed class ConditionNode : IBehaviorNode
    {
        private readonly Func<bool> _predicate;

        /// <summary>
        /// 조건 델리게이트로 조건 노드를 초기화합니다.
        /// </summary>
        /// <param name="predicate">평가할 조건. null을 반환하지 않아야 합니다.</param>
        public ConditionNode(Func<bool> predicate) { _predicate = predicate; }

        /// <inheritdoc/>
        public NodeState Tick() => _predicate() ? NodeState.Success : NodeState.Failure;
    }

    /// <summary>
    /// 액션 노드입니다. 전달된 동작을 수행하고 <see cref="NodeState"/>를 반환합니다.
    /// 간단한 액션은 <see cref="Action"/> 생성자를 사용하여 항상 <see cref="NodeState.Success"/>를 반환하도록 만들 수 있습니다.
    /// 보다 정교한 제어가 필요하면 <see cref="Func{TResult}"/> 생성자를 사용해 Success/Failure/Running을 직접 반환하십시오.
    /// </summary>
    public sealed class ActionNode : IBehaviorNode
    {
        private readonly Func<NodeState> _action;

        /// <summary>
        /// 완료 시 항상 <see cref="NodeState.Success"/>를 반환하는 액션 노드를 생성합니다.
        /// </summary>
        /// <param name="action">실행할 동작.</param>
        public ActionNode(Action action) { _action = () => { action(); return NodeState.Success; }; }

        /// <summary>
        /// 실행 결과를 직접 반환하는 액션 노드를 생성합니다.
        /// </summary>
        /// <param name="action">실행할 동작. 실행 결과로 <see cref="NodeState"/>를 반환합니다.</param>
        public ActionNode(Func<NodeState> action) { _action = action; }

        /// <summary>
        /// 노드를 실행하고 결과 상태를 반환합니다.
        /// </summary>
        /// <returns><see cref="NodeState"/> 값.</returns>
        public NodeState Tick() => _action();
    }
}