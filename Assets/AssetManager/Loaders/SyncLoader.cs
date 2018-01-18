using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class SyncLoader : Loader
    {
        public override bool isDone
        {
            get
            {
                return true;
            }
        }
    }
}