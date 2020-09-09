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

		AssetBundleLoader LoadAssetBundleAsync(string path, int tag, bool cache,
			Action<AssetBundleReference> completeHandle = null,
			Action<AssetBundleLoader> beforLoadComplete = null,
			Action<AssetBundleLoader> afterLoadComplete = null);
		AssetBundleReference LoadAssetBundleSync(string path, int tag, bool cache = true);

		AssetBundleAsyncLoader CreateAssetBundleAsyncLoader(string path);
		AssetBundleAsyncLoader CreateAssetBundleExistLoader(string path);
		AssetBundleSyncLoader CreateAssetBundleSyncLoader(string path);


		AssetLoader LoadAssetAsync(string path, int tag, Type type, bool autoReleaseBundle,
			Action<AssetReference> completeHandle = null,
			Action<AssetLoader> beforLoadComplete = null,
			Action<AssetLoader> afterLoadComplete = null);

		AssetReference LoadAssetSync(string path, int tag, Type type);

		AssetAsyncLoader CreateAssetAsyncLoader(string path);
		AssetAsyncLoader CreateAssetExistLoader(string path);
		AssetSyncLoader CreateAssetSyncLoader(string path);

		void ActiveLoader(Loader loader);
		void ReleaseLoader(Loader loader);

	}
}
