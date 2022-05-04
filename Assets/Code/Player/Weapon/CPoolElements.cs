using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPoolElements : MonoBehaviour
{
    public static CPoolElements SharedInstance;
    public int amountPool;
    List<GameObject> decalPool = new List<GameObject>();
    public GameObject decalPrefab;

    private void Awake()
    {
        SharedInstance = this;
    }

    private void Start()
    {
        for(int i = 0; i < amountPool; i++)
        {
            GameObject obj = (GameObject)Instantiate(decalPrefab);
            obj.SetActive(false);
            decalPool.Add(obj);
            obj.transform.parent = this.transform;
        }
    }

    public GameObject GetDecal()
    {
        for(int i = 0; i < decalPool.Count; i++)
        {
            if (!decalPool[i].activeInHierarchy)
                return decalPool[i];
        }
        return null;
    }
}
