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
        Version version { get; }
        byte format { get; }

        bool useBundleDependenciesAll { get; }

        event Action<bool> onLoadComplete;

        void Init();
		void Clean();

		void Load(string filePath);
        void LoadFromStream(Stream steam);
        void SaveBinary(string fileName);
        void SaveBinaryStream(Stream stream);

        AssetLoadInfo GetAssetInfo(string assetPath);
        AssetLoadInfo GetAssetInfo(string assetPath, ulong assetPathHash);
        AssetLoadInfo GetAssetInfoWithAlias(string alias);
        AssetBundleLoadInfo GetAssetBundleInfo(ulong bundleId);
    }
}
