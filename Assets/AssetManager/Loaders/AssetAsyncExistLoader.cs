namespace YH.AssetManage
{
    public class AssetAsyncExistLoader : AssetAsyncLoader
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
