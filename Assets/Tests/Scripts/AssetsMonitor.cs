using UnityEngine;
using System.Collections.Generic;

namespace YH
{

    public class AssetsMonitor : AssetManager.UnitySingleton<AssetsMonitor>
    {

        public struct AssetItem
        {
            public string name;
            public string type;
            public int instanceId;
            public int hashCode;

            public AssetItem(string _name, string _type, int _instanceId, int _hashCode)
            {
                name = _name;
                type = _type;
                instanceId = _instanceId;
                hashCode = _hashCode;
            }
        }

        float m_Duration = 1.0f;

        float m_Elapsed = 0;

        List<AssetItem> m_SnapedAssets;

        public List<AssetItem> assets
        {
            get
            {
                return m_SnapedAssets;
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            m_Elapsed += Time.fixedDeltaTime;
            if (m_Elapsed >= m_Duration)
            {
                m_Elapsed -= m_Duration;
                CheckAssets();

            }
        }

        List<AssetItem> Snap()
        {
            Object[] alls = Resources.FindObjectsOfTypeAll<Object>();
            List<AssetItem> snapedAssets = new List<AssetItem>();
            for (int i = 0; i < alls.Length; ++i)
            {
                snapedAssets.Add(new AssetItem(
                    alls[i].name,
                    alls[i].GetType().ToString(),
                    alls[i].GetInstanceID(),
                    alls[i].GetHashCode()
                    ));
            }
            return snapedAssets;
        }

        List<AssetItem> GetDiffs(List<AssetItem> froms, List<AssetItem> tos)
        {
            List<AssetItem> diffs = new List<AssetItem>();

            Dictionary<int, AssetItem> toMap = new Dictionary<int, AssetItem>();
            for (int i = 0, l = tos.Count; i < l; ++i)
            {
                toMap[tos[i].hashCode] = tos[i];
            }

            for (int i = 0, l = froms.Count; i < l; ++i)
            {
                if (!toMap.ContainsKey(froms[i].hashCode))
                {
                    diffs.Add(froms[i]);
                }
            }
            return diffs;
        }

        public void CheckAssets()
        {
            if (m_SnapedAssets == null || m_SnapedAssets.Count == 0)
            {
                m_SnapedAssets = Snap();
            }
            else
            {
                List<AssetItem> currents = Snap();
                List<AssetItem> removeds = GetDiffs(m_SnapedAssets, currents);
                m_SnapedAssets = currents;

                //show removed items;
                if (removeds != null && removeds.Count > 0)
                {
                    for (int i = 0, l = removeds.Count; i < l; ++i)
                    {
                        Debug.LogFormat("remove asset name={0},type={1},id={2}", removeds[i].name, removeds[i].type, removeds[i].instanceId);
                    }
                }
            }
        }
    }
}
