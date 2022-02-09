using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
{
    public interface IInfoManager
    {
        bool loading { get; }
        string version { get; }
        int format { get; }
        event Action<bool> onLoadComplete;
        Dictionary<string, AssetBundleInfo> AssetBundleInfos { get; }
        Dictionary<string, AssetInfo> AssetInfos { get; }

        void Init();
		void Clean();

		void Load(string filePath);
        void LoadFromStream(Stream steam);
        void SaveBinary(string fileName);
        void SaveBinaryStream(Stream stream);

        AssetInfo FindAssetInfo(string key);
        AssetInfo FindAssetInfoWithAlias(string alias);
        AssetBundleInfo FindAssetBundleInfo(string key);

   
    }
}
