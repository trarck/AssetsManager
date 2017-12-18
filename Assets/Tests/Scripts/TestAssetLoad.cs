using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YH.AssetManager;
public class TestAssetLoad : MonoBehaviour {
    [SerializeField]
    AssetManager m_AssetManager;

	// Use this for initialization
	void Start () {
        m_AssetManager.Init();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
