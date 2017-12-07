using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    internal static class SetPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<Set<T>> s_SetPool = new ObjectPool<Set<T>>(null, l => l.Clear());

        public static Set<T> Get()
        {
            return s_SetPool.Get();
        }

        public static void Release(Set<T> toRelease)
        {
            s_SetPool.Release(toRelease);
        }
    }
}
