﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YH.AssetManager;

public class Battle : MonoBehaviour {

	// Use this for initialization
	void Start () {
        AssetManager.Instance.UnloadUnuseds("Main");
        StartCoroutine(Test());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator Test()
    {
        yield return new WaitForSeconds(1);
        AssetManager.Instance.UnloadUnuseds();
    }
}