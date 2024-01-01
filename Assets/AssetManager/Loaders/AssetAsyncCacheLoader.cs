namespace YH.AssetManage
{
    public class AssetAsyncCacheLoader : AssetAsyncLoader
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
            Complete();
        }

    }
}
