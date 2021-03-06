﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
{
    public class AMSetting
    {
        //unity的webRequest.timeout默认值0，即没有timeout。
        //web请求超时时间.
        public static int WebRequestTimeout = 60;
        //下载请求超时时间,一般默认为0.下载AssetBundle时不应该有超时时间,和web请求不同。
        public static int DownloadTimeout = 0;
        //web请求重试次数
        public static int RequestRetryTimes = 3;
        //AssetBundle加载依赖错误时是否继续
        public static bool BreakOnBundleLoadDependencyError=true;
        //是否cache依赖的Bundle.
        //一般情况下建议开启。在一次加载完成后手动释放cache的bundle.
        public static bool CacheDependencyBundle = false;
    }
}
