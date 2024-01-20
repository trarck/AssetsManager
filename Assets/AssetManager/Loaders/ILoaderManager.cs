using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YH.AssetManage
{
	public interface ILoaderManager
	{
		IRequestManager requestManager
		{
			get; set;
		}
		IInfoManager infoManager
		{
			get; set;
		}
		IReferenceManager referenceManager
		{
			get; set;
		}

		void Init();

		void Init(IInfoManager infoManager, IRequestManager requestManager, IReferenceManager referenceManager);

		void Clean();

		AssetBundleLoader LoadAssetBundleAsync(ulong bundleId, int tag, bool cache,
			Action<AssetBundleReference> completeHandle = null,
			Action<AssetBundleLoader> beforLoadComplete = null,
			Action<AssetBundleLoader> afterLoadComplete = null);
		AssetBundleReference LoadAssetBundleSync(ulong bundleId, int tag, bool cache = true);

		AssetBundleAsyncLoader CreateAssetBundleAsyncLoader(ulong bundleId, int tag, bool cache);
		AssetBundleAsyncLoader CreateNewAssetBundleAsyncLoader(ulong bundleId);
		AssetBundleAsyncLoader CreateAssetBundleCacheLoader(ulong bundleId);
		AssetBundleLoader CreateAssetBundleSyncLoader(ulong bundleId);


		AssetLoaderOperation LoadAssetAsync(string path, int tag, Type type, bool autoReleaseBundle,
			Action<AssetReference> completeHandle = null,
			Action<AssetLoader> beforLoadComplete = null,
			Action<AssetLoader> afterLoadComplete = null);

		AssetReference LoadAssetSync(string path, int tag, Type type);
		AssetLoader CreateAssetAsyncLoader(string path, ulong pathHash = 0);
		AssetLoader CreateAssetCacheLoader(string path);
		AssetLoader CreateAssetSyncLoader(string path, ulong pathHash = 0);

		void ActiveLoader(Loader loader);
		void ReleaseLoader(Loader loader);
	}
}
