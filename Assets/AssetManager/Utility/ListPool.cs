using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    internal static class ListPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, l => l.Clear());

        public static List<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }

    internal static class StackPool<T>
    {
        private static readonly ObjectPool<Stack<T>> m_StackPool = new ObjectPool<Stack<T>>(null,l=>l.Clear());

        public static Stack<T> Get()
        {
            return m_StackPool.Get();
        }

        public static void Release(Stack<T> toRelease)
        {
            m_StackPool.Release(toRelease);
        }
    }

    internal static class QueuePool<T>
    {
        private static readonly ObjectPool<Queue<T>> m_QueuePool = new ObjectPool<Queue<T>>(null, l => l.Clear());

        public static Queue<T> Get()
        {
            return m_QueuePool.Get();
        }

        public static void Release(Queue<T> toRelease)
        {
            m_QueuePool.Release(toRelease);
        }
    }

    internal static class LinkedListPool<T>
    {
        private static readonly ObjectPool<LinkedList<T>> m_LinkedListPool = new ObjectPool<LinkedList< T>>(null, l => l.Clear());

        public static LinkedList<T> Get()
        {
            return m_LinkedListPool.Get();
        }

        public static void Release(LinkedList<T> toRelease)
        {
            m_LinkedListPool.Release(toRelease);
        }
    }
}
