using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using YH.AssetManager;

public class Main : MonoBehaviour {
    

	// Use this for initialization
	void Start () {
        StartCoroutine(DoSomething());
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    IEnumerator DoSomething()
    {
        yield return new WaitForSeconds(3);
        Debug.Log("Goto Battle");

        AssetManager.Instance.LoadScene("Scenes/Battle.unity", AssetTag.Battle, (abr) =>
        {
            Debug.Log("LoadCompete");
            SceneManager.LoadScene("Scenes/Battle");
        });
    }
}
