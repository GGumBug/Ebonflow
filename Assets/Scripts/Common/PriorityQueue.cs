using System;

public class PriorityQueue<T>
{
    private class PQNode
    {
        public T Data { get; private set; }
        public float Priority { get; private set; } = 0;

        public PQNode(T data, float priority)
        {
            Data = data;
            Priority = priority;
        }
    }

    private PQNode[] _heap;
    private int _size;
    private SortOrder _sortOrder = SortOrder.Ascending;

    public int Count { get { return _size; } }

    public PriorityQueue(int capacity, SortOrder order)
    {
        _heap = new PQNode[capacity];
        _sortOrder = order;
        _size = 0;
    }

    public void Clear()
    {
        _heap = new PQNode[_heap.Length];
        _size = 0;
    }

    public void Enqueue(T data, float priority)
    {
        PQNode node = new PQNode(data, priority);
        if (_size == _heap.Length)
        { Resize(); }

        _heap[_size] = node;
        HeapifyUp(_size);
        _size++;
    }

    public T Dequeue()
    {
        if (_size == 0)
            throw new InvalidOperationException("Priority queue is empty.");

        PQNode node = _heap[0];
        _heap[0] = _heap[_size - 1];
        _size--;
        HeapifyDown(0);

        return node.Data;
    }

    public T Peek()
    {
        if (_size == 0)
            throw new InvalidOperationException("Priority queue is empty.");

        return _heap[0].Data;
    }

    private void HeapifyUp(int idx)
    {
        while (idx > 0)
        {
            int idxParent = GetParent(idx);
            if (Compare(_heap[idx], _heap[idxParent]))
            {
                Swap(_heap, idx, idxParent);
                idx = idxParent;
            }
            else
            {
                break;
            }
        }
    }

    private bool Compare(PQNode a, PQNode b)
    {
        return _sortOrder == SortOrder.Ascending ? a.Priority < b.Priority : a.Priority > b.Priority;
    }

    private void HeapifyDown(int idx)
    {
        int idxLeftChild = (2 * idx) + 1;
        int idxRightChild = (2 * idx) + 2;
        int idxTarget = idx;

        if (idxLeftChild < _size && Compare(_heap[idxLeftChild], _heap[idxTarget]))
        {
            idxTarget = idxLeftChild;
        }

        if (idxRightChild < _size && Compare(_heap[idxRightChild], _heap[idxTarget]))
        {
            idxTarget = idxRightChild;
        }

        if (idxTarget != idx)
        {
            Swap(_heap, idx, idxTarget);
            HeapifyDown(idxTarget);
        }
    }

    private int GetParent(int idx)
    {
        return (idx - 1) / 2;
    }

    private void Resize()
    {
        var temp = new PQNode[_heap.Length * 2];
        for (int i = 0; i < _heap.Length; i++)
        { temp[i] = _heap[i]; }
        _heap = temp;
    }

    private void Swap(PQNode[] arr, int a, int b)
    {
        PQNode temp = arr[a];
        arr[a] = arr[b];
        arr[b] = temp;
    }
}

