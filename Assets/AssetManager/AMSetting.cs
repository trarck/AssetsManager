using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
{
    public class AMSetting
    {
        //web请求超时时间
        public static int WebRequestTimeout = 12;
        //web请求重试次数
        public static int RequestRetryTimes = 3;
        //AssetBundle加载依赖错误时是否继续
        public static bool BreakOnBundleLoadDependencyError=true;
        //是否cache依赖的Bundle.
        //一般情况下建议开启。在一次加载完成后手动释放cache的bundle.
        public static bool CacheDependencyBundle = false;
    }
}
