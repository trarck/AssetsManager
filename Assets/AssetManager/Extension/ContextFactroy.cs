using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage.Extension
{
    public class ContextFactroy
    {
        static ObjectPool<MonoContext> s_MonoContextPool = new ObjectPool<MonoContext>(null, (o) => { o.Clean(); });
        static ObjectPool<MonoEventContext> s_MonoEventContextPool = new ObjectPool<MonoEventContext>(null, (o) => { o.Clean(); });

        static Dictionary<GameObject, MonoContext> s_MonoContexts=new Dictionary<GameObject, MonoContext>();
        static Dictionary<GameObject, MonoEventContext> s_MonoEventContexts = new Dictionary<GameObject, MonoEventContext>();

        public static MonoContext GetMonoContext(GameObject gameObject)
        {
            MonoContext c = null;
            if(!s_MonoContexts.TryGetValue(gameObject,out c))
            {
                c = s_MonoContextPool.Get();
                c.target = gameObject;
                s_MonoContexts[gameObject] = c;
            }
            return c;
        }

        public static void RemoveMonoContext(GameObject gameObject)
        {
            MonoContext c = null;
            if (s_MonoContexts.TryGetValue(gameObject,out c))
            {
                c.enable = false;
                s_MonoContexts.Remove(gameObject);
                s_MonoContextPool.Release(c);
            }
        }

        public static MonoEventContext GetMonoEventContext(GameObject gameObject)
        {
            MonoEventContext c = null;
            if (!s_MonoEventContexts.TryGetValue(gameObject, out c))
            {
                c = s_MonoEventContextPool.Get();
                c.target = gameObject;
                s_MonoEventContexts[gameObject] = c;
            }
            return c;
        }

        public static void RemoveMonoEventContext(GameObject gameObject)
        {
            MonoEventContext c = null;
            if (s_MonoEventContexts.TryGetValue(gameObject, out c))
            {
                c.enable = false;
                s_MonoEventContexts.Remove(gameObject);
                s_MonoEventContextPool.Release(c);
            }
        }
    }
}
