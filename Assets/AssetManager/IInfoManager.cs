using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
{
    public interface IInfoManager
    {
        bool inited { get; }
        string version { get; }
        int format { get; }
        event Action<bool> onInitComplete;

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
