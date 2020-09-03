using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YH.AssetManage
{
	public interface ILoaderManager
	{
		IRequestManager requestManager{get;set;}
		IInfoManager infoManager{get;set;}

		void Init();

		void Init(IInfoManager infoManager, IRequestManager requestManager);

		void Clean();

		AssetBundleAsyncLoader CreateAssetBundleAsyncLoader(string path);
		AssetBundleAsyncLoader CreateAssetBundleExistLoader(string path);
		AssetBundleSyncLoader CreateAssetBundleSyncLoader(string path);

		AssetAsyncLoader CreateAssetAsyncLoader(string path);
		AssetAsyncLoader CreateAssetExistLoader(string path);
		AssetSyncLoader CreateAssetSyncLoader(string path);

		void ActiveLoader(Loader loader);
		void ReleaseLoader(Loader loader);

	}
}
