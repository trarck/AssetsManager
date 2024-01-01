using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManage
{
    public interface IReferenceManager
	{
		Dictionary<ulong, AssetReference> assets
		{
			get;set;
		}

		Dictionary<ulong, AssetBundleReference> assetBundles
		{
			get;set;
		}

		void Init();

		void Clean();

		bool TryGetAsset(ulong assetPathHash, out AssetReference ar);
		bool TryGetAsset(string assetPath, out AssetReference ar);
		void AddAssetReference(AssetReference ar);
		void RemoveAssetReference(AssetReference ar);

		bool TryGetAssetBundle(ulong assetBundlePath, out AssetBundleReference abr);
		void AddAssetBundleReference(AssetBundleReference abr, bool cache);
		void RemoveAssetBundleReference(AssetBundleReference abr);

		void UnloadUnuseds();

		void UnloadUnuseds(int tag, bool removeTag = true);

		void UnloadUnusedBundles();

		void UnloadUnusedBundles(int tag);

		void UnloadUnusedAssets();

		void UnloadUnusedAssets(int tag);

		void UncacheAll();

		void UncacheAssetBundle(ulong assetBundleName);

		void UncacheAssetBundle(AssetBundleReference abr);

		void UncacheAsset(ulong assetName);

		void UncacheAsset(AssetReference ar);

		void RemoveTags(int tag);

		void BreakAssetsBundleReferenceAll();

		/// <summary>
		/// remove asset's asset bundle reference
		/// when asset is loaded from asset bundle,normally asset bundle can be unload(false).
		/// this can be reduce memory used.
		/// 如果asset bunlde被多个asset引用，而同时加载这些asset的时候，不能一个一个删除，可以在加载完成后集中删除。
		/// </summary>
		/// <param name="tag"></param>
		void BreakAssetBundleReferenceByTag(int tag);

	}
}
