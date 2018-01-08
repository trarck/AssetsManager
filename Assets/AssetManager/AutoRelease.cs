using UnityEngine;

namespace YH.AssetManager
{
    public class AutoRelease
    {
        public int releaseFrame;
        public string key;

        public void ReleaseNextFrame()
        {
            releaseFrame = Time.frameCount + 1;
        }

        public void ReleaseNextFrame(int frame)
        {
            releaseFrame = frame + 1;
        }

        public bool IsReleasFrame(int frame)
        {
            return releaseFrame==frame;
        }
    }
}
