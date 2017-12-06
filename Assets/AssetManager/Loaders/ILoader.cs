using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YH.AssetManager
{
    public interface ILoader
    {
        bool isDone { get; set; }

        void Start();
    }
}
