using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using YH.AssetManage;

namespace Tests
{
    public class AssetManagerTest
	{
        AssetManager m_AssetManager;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("OneTimeSetUp");
			m_AssetManager = AssetManager.Instance;
            m_AssetManager.Init();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Debug.Log("OneTimeTearDown");
            m_AssetManager.Clean();
        }

        [SetUp]
        public void SetUp()
        {
            Debug.Log("Setup");
        }

        [TearDown]
        public void TearDown()
        {
            Debug.Log("TearDown");

            m_AssetManager.UnloadUnuseds();
        }

        [Test]
        public void TestInfoLoad()
        {

        }
    }
}
