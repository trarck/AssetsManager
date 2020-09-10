using UnityEngine;
using System.Collections;

namespace YH.AssetManage
{
    public class UnitySingleton<T> : MonoBehaviour
        where T : Component
    {
        private static T m_Instance;

        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindObjectOfType(typeof(T)) as T;
                    if (m_Instance == null)
                    {
						//AMDebug.Log("create instance");

						GameObject singletonObj = new GameObject();
						singletonObj.name = "(singleton)" + typeof(T).ToString();
						//方法一
						DontDestroyOnLoad(singletonObj);

						//方法二
						//DontSave标志表示不会在加载新场景删除，所以不用DontDestroyOnLoad
						//singletonObj.hideFlags = HideFlags.HideAndDontSave;

						//AMDebug.Log("add instance before");
						m_Instance = singletonObj.AddComponent<T>();

						//AMDebug.Log("add instance after");
					}
				}
                return m_Instance;
            }
        }

        public static void DestroyInstance()
        {
            Destroy(m_Instance);
            m_Instance = null;
        }
    }
}