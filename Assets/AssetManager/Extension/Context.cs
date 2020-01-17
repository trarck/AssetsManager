using UnityEngine;
using UnityEngine.SceneManagement;

namespace YH.AssetManage.Extension
{
    public class Context
    {
        public System.Action onDisable;

        protected bool m_Enable=true;

        protected void TriggerDisable()
        {
            if (onDisable != null)
            {
                onDisable();
            }
        }

        public virtual bool enable
        {
            get
            {
                return m_Enable;
            }
            set
            {
#if ASSETMANAGER_LOG
                Debug.LogFormat("[Context] Set enable={0}.",value);
#endif
                if (m_Enable != value)
                {
                    m_Enable = value;

                    if (value == false)
                    {
                        TriggerDisable();
                    }
                }
            }
        }

        public virtual void Clean()
        {
            m_Enable = true;
        }
    }

    public class MonoContext : Context
    {
        GameObject m_Target;

        public GameObject target
        {
            get
            {
                return m_Target;
            }
            set
            {
                m_Target = value;
            }
        }

        public override bool enable
        {
            get
            {
                return m_Enable && m_Target != null;
            }

            set => base.enable = value;
        }

        public override void Clean()
        {
            base.Clean();
            m_Target = null;
        }
    }

    public class MonoEventContext : Context
    {
        protected class MonoDistroyComponent : MonoBehaviour
        {
            public System.Action onDestroy;

            void OnDestroy()
            {
                if (onDestroy != null)
                {
                    onDestroy();
                }
            }
        }

        GameObject m_Target;

        MonoDistroyComponent m_MonoDistroyComponent;

        public GameObject target
        {
            get
            {
                return m_Target;
            }
            set
            {
                m_Target = value;
                SetTarget(value);
            }
        }

        void SetTarget(GameObject target)
        {
            if (target)
            {
                m_MonoDistroyComponent = target.GetComponent<MonoDistroyComponent>();

                if (!m_MonoDistroyComponent)
                {
                    m_MonoDistroyComponent = target.AddComponent<MonoDistroyComponent>();
                }
                m_MonoDistroyComponent.onDestroy += OnMonoDestory;
            }
        }

        void OnMonoDestory()
        {
            m_MonoDistroyComponent = null;
            enable = false;
        }

        public override void Clean()
        {
            base.Clean();
            m_Target = null;
            m_MonoDistroyComponent = null;
        }
    }

    public class SceneContext:Context
    {
        Scene m_Scene;
    }
}
