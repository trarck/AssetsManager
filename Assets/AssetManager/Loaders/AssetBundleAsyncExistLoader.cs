namespace YH.AssetManage
{
    public class AssetBundleAsyncExistLoader : AssetBundleAsyncLoader
	{
        public override bool isDone
        {
            get
            {
                return true;
            }
        }

        public override void Start()
        {
            if (info != null)
            {
                AMDebug.LogErrorFormat("[AssetManage]Load AssetBundle mode not open {0}", info.fullName);
            }
            Complete();
        }
    }
}
