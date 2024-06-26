﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManage
{
    public class ReferenceManager:IReferenceManager
	{
		//all loaded  asset bundles
		Dictionary<ulong, AssetBundleReference> m_AssetBundles = null;

		//all loaded  assets
		Dictionary<ulong, AssetReference> m_Assets = null;

		public Dictionary<ulong, AssetBundleReference> assetBundles
		{
			get
			{
				return m_AssetBundles;
			}
			set
			{
				m_AssetBundles = value;
			}
		}

		public Dictionary<ulong, AssetReference> assets
		{
			get
			{
				return m_Assets;
			}
			set
			{
				m_Assets = value;
			}
		}

		public void Init()
		{
			m_AssetBundles = new Dictionary<ulong, AssetBundleReference>();
			m_Assets = new Dictionary<ulong, AssetReference>();
		}

		public void Clean()
		{
			m_Assets.Clear();
			m_AssetBundles.Clear();
		}

		#region Asset
		public bool TryGetAsset(ulong assetPathHash, out AssetReference ar)
		{
			return m_Assets.TryGetValue(assetPathHash, out ar);
		}

		public bool TryGetAsset(string assetPath, out AssetReference ar)
		{
            if (string.IsNullOrEmpty(assetPath))
            {
				ar = null;
				return false;
            }
			return TryGetAsset(xxHash.xxHash64.ComputeHash(assetPath), out ar);
		}

		public void AddAssetReference(AssetReference ar)
		{
			if (ar != null)
			{
				m_Assets[ar.id] = ar;
				//asset loader always standalone
				ar.Cache();
				ar.onDispose += RemoveAssetReference;
			}
		}

		public void RemoveAssetReference(AssetReference ar)
		{
			m_Assets.Remove(ar.id);
		}
		#endregion

		#region AssetBundle
		public bool TryGetAssetBundle(ulong assetBundlePath, out AssetBundleReference abr)
		{
			return m_AssetBundles.TryGetValue(assetBundlePath, out abr);
		}

		public void AddAssetBundleReference(AssetBundleReference abr,bool cache)
		{
			if (abr != null)
			{
				m_AssetBundles[abr.id] = abr;

				if (cache)
				{
					abr.Cache();
				}
				else
				{
					abr.isCache = false;
				}
				abr.onDispose += RemoveAssetBundleReference;
			}
		}

		public void RemoveAssetBundleReference(AssetBundleReference abr)
		{
			m_AssetBundles.Remove(abr.id);
		}

		#endregion

		#region Unload unused
		public void UnloadUnuseds()
		{
			UnloadUnusedAssets();
			UnloadUnusedBundles();
			Resources.UnloadUnusedAssets();
		}

		public void UnloadUnuseds(int tag, bool removeTag = true)
		{
			UnloadUnusedAssets(tag);
			UnloadUnusedBundles(tag);
			if (removeTag)
			{
				RemoveTags(tag);
			}
			Resources.UnloadUnusedAssets();
		}

		public void UnloadUnusedBundles()
		{
			if (m_AssetBundles.Count == 0)
			{
				return;
			}

			AssetBundleReference abr = null;

			Stack<ulong> checkQueue = StackPool<ulong>.Get();
			HashSet<ulong> checkings = HashSetPool<ulong>.Get();

			foreach (ulong key in m_AssetBundles.Keys)
			{
				abr = m_AssetBundles[key];
				if (abr.isCache)
				{
					checkQueue.Push(key);
					checkings.Add(key);
				}
			}

			Action<ulong> checkFun = (key) =>
			{
				abr = m_AssetBundles[key];
				checkings.Remove(key);

				if (abr.isUnused())
				{
					//check dependencies
					if (abr.dependencies != null && abr.dependencies.Count > 0)
					{
						foreach (AssetBundleReference sub in abr.dependencies)
						{
							if (sub.isCache && !checkings.Contains(sub.id))
							{
								checkQueue.Push(sub.id);
							}
						}
					}

					abr.Dispose();
					m_AssetBundles.Remove(key);
				}
			};

			//recheck unused asset bundle
			while (checkQueue.Count > 0)
			{
				checkFun(checkQueue.Pop());
			}

			StackPool<ulong>.Release(checkQueue);
			HashSetPool<ulong>.Release(checkings);
		}

		public void UnloadUnusedBundles(int tag)
		{
			if (m_AssetBundles.Count == 0)
			{
				return;
			}

			AssetBundleReference abr = null;

			Stack<ulong> checkQueue = StackPool<ulong>.Get();
			HashSet<ulong> checkings = HashSetPool<ulong>.Get();


			Action<ulong> checkFun = (key) =>
			{
				abr = m_AssetBundles[key];
				checkings.Remove(key);

				if (abr.isUnused())
				{
					//check dependencies
					if (abr.dependencies != null && abr.dependencies.Count > 0)
					{
						foreach (AssetBundleReference sub in abr.dependencies)
						{
							//只有同样tag和空tag的ref才需要重新检查。
							if (sub.isCache && (sub.tagCount == 0 || sub.HaveTag(tag)) && !checkings.Contains(sub.id))
							{
								checkQueue.Push(sub.id);
							}
						}
					}

					abr.Dispose();
					m_AssetBundles.Remove(key);
				}
			};

			foreach (ulong key in m_AssetBundles.Keys)
			{
				abr = m_AssetBundles[key];
				if (abr.HaveTag(tag) && abr.isCache)
				{
					checkQueue.Push(key);
					checkings.Add(key);
				}
			}

			//recheck unused asset bundle
			while (checkQueue.Count > 0)
			{
				checkFun(checkQueue.Pop());
			}

			StackPool<ulong>.Release(checkQueue);
			HashSetPool<ulong>.Release(checkings);
		}

		public void UnloadUnusedAssets()
		{
			if (m_Assets.Count == 0)
			{
				return;
			}
			AssetReference ar = null;
			List<ulong> keys = ListPool<ulong>.Get();
			keys.AddRange(m_Assets.Keys);

			for (int i = 0, l = keys.Count; i < l; ++i)
			{
				ar = m_Assets[keys[i]];
				if (ar.isUnused())
				{
					ar.Dispose();
					m_Assets.Remove(keys[i]);
				}
			}
			ListPool<ulong>.Release(keys);
		}

		public void UnloadUnusedAssets(int tag)
		{
			if (m_Assets.Count == 0)
			{
				return;
			}

			AssetReference ar = null;
			List<ulong> keys = ListPool<ulong>.Get();
			keys.AddRange(m_Assets.Keys);

			for (int i = 0, l = keys.Count; i < l; ++i)
			{
				ar = m_Assets[keys[i]];
				if (ar.HaveTag(tag))
				{
					if (ar.isUnused())
					{
						ar.Dispose();
						m_Assets.Remove(keys[i]);
					}
				}
			}
			ListPool<ulong>.Release(keys);
		}
	  
		#endregion

		#region uncache
		void UncacheAllAssetBundles()
		{
			foreach (var iter in m_AssetBundles)
			{
				AssetBundleReference abr = iter.Value;
				abr.UnCache();
			}
		}

		void UncacheAllAssets()
		{
			foreach (var iter in m_Assets)
			{
				AssetReference ar = iter.Value;
				ar.UnCache();
			}
		}

		public void UncacheAll()
		{
			UncacheAllAssets();
			UncacheAllAssetBundles();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetBundleName"></param>
		public void UncacheAssetBundle(ulong assetBundleName)
		{
			if (m_AssetBundles.ContainsKey(assetBundleName))
			{
				AssetBundleReference abr = m_AssetBundles[assetBundleName];
				abr.UnCache();
			}
		}

		public void UncacheAssetBundle(AssetBundleReference abr)
		{
			if (abr != null)
			{
				abr.UnCache();
			}
		}

		public void UncacheAsset(ulong assetName)
		{
			if (m_Assets.ContainsKey(assetName))
			{
				AssetReference ar = m_Assets[assetName];
				ar.UnCache();
			}
		}

		public void UncacheAsset(AssetReference ar)
		{
			if (ar != null)
			{
				ar.UnCache();
			}
		}

		#endregion

		#region Remove tags
		public void RemoveTags(int tag)
		{
			RemoveAssetsTag(tag);
			RemoveAssetBundlesTag(tag);
		}

		protected void RemoveAssetBundlesTag(int tag)
		{
			if (m_AssetBundles.Count == 0)
			{
				return;
			}

			AssetBundleReference abr = null;
			var iter = m_AssetBundles.GetEnumerator();
			while (iter.MoveNext())
			{
				abr = iter.Current.Value;
				if (abr.isCache && abr.HaveTag(tag))
				{
					abr.RemoveTag(tag);
				}
			}
		}

		protected void RemoveAssetsTag(int tag)
		{
			if (m_Assets.Count == 0)
			{
				return;
			}

			AssetReference ar = null;
			var iter = m_Assets.GetEnumerator();
			while (iter.MoveNext())
			{
				ar = iter.Current.Value;
				if (ar.isCache && ar.HaveTag(tag))
				{
					ar.RemoveTag(tag);
				}
			}
		}
		#endregion

		/// <summary>
		/// remove asset's asset bundle reference
		/// </summary>
		public void BreakAssetsBundleReferenceAll()
		{
			if (m_Assets.Count == 0)
			{
				return;
			}

			AssetReference ar = null;
			var iter = m_Assets.GetEnumerator();
			while (iter.MoveNext())
			{
				ar = iter.Current.Value;
				if (ar.isCache)
				{
					ar.ReleaseBundleReference();
				}
			}
		}

		/// <summary>
		/// remove asset's asset bundle reference
		/// when asset is loaded from asset bundle,normally asset bundle can be unload(false).
		/// this can be reduce memory used.
		/// 如果asset bunlde被多个asset引用，而同时加载这些asset的时候，不能一个一个删除，可以在加载完成后集中删除。
		/// </summary>
		/// <param name="tag"></param>
		public void BreakAssetBundleReferenceByTag(int tag)
		{
			if (m_Assets.Count == 0)
			{
				return;
			}

			AssetReference ar = null;
			var iter = m_Assets.GetEnumerator();
			while (iter.MoveNext())
			{
				ar = iter.Current.Value;
				if (ar.isCache && ar.HaveTag(tag))
				{
					ar.ReleaseBundleReference();
				}
			}
		}

	}
}
