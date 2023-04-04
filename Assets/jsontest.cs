using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class jsontest : MonoBehaviour
{
    [Serializable]
    public struct LevelsSchema
    {
        public LevelsPrev[] root;
    }

    [Serializable]
    public struct LevelsPrev
    {
        public int id;
    }
    [SerializeField] public LevelsSchema curLevels;
    void Start()
    {
        var levelUrl = "http://127.0.0.1:3001/levels";
        StartCoroutine(Get(levelUrl));

    }

    [Serializable]
    public class test1
    {
        public int id;
        public string info;
    }

    [SerializeField] public test1 test;
    private IEnumerator Get(string url)
    {
        using (var webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log("Error:" + webRequest.error);
            }
            else
            {
                var text = webRequest.downloadHandler.text;
                Debug.Log("received" + text);
                // text = text.Substring(1, text.Length-2);
                // Debug.Log("After trim" + webRequest.downloadHandler.text);
                curLevels= JsonUtility.FromJson<LevelsSchema>(text);
            }
        };
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
