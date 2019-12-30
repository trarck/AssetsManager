using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
{
    public class SyncLoaderRequest : Request
    {
        public override bool isDone
        {
            get
            {
                return true;
            }
        }

        public override float progress
        {
            get
            {
                return 1;
            }
        }

        public override bool haveError
        {
            get
            {
                return false;
            }
        }
    }

    public class EmptyLoaderRequest : Request
    {
        public override bool isDone
        {
            get
            {
                return true;
            }
        }

        public override float progress
        {
            get
            {
                return 1;
            }
        }

        public override bool haveError
        {
            get
            {
                return false;
            }
        }
    }
}
