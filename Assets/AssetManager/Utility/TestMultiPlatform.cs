using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.AssetManager.Utility
{
#if UNITY_ANDROID
    using TestTarget = TestAndroid;
#elif UNITY_IOS
    using TestTarget = TestIos;
#else
    using TestTarget = TestCommon;
#endif


    public class TestMultiPlatform
    {
        public static void DoA()
        {
            TestTarget.DoA();
        }

        public static void DoB()
        {
            TestTarget.DoB();
        }
    }

    public class TestAndroid
    {
        public static void DoA()
        {

        }
        public static void DoB()
        {

        }
    }

    public class TestIos
    {
        public static void DoA()
        {

        }
        public static void DoB()
        {

        }
    }
    public class TestCommon
    {
        public static void DoA()
        {

        }
        public static void DoB()
        {

        }
    }
}
