using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    class Value
    {
        public int cubeSize;
        public float spawnPosition;
        public int spawnRotation;
        public int spawnNumRange;
    }

    [SerializeField]
    Value value;

    void Awake()
    {
        value = JsonUtility.FromJson<Value>(Resources.Load<TextAsset>("Json/Value").text);
    }

    void Start()
    {
        StartCoroutine(CreateObj());
    }

    void Update()
    {
        
    }

    IEnumerator CreateObj()
    {
        GameObject fish;
        GameObject fishList = GameObject.Find("FishList");

        float range = value.cubeSize * value.spawnPosition;
        for (int i = 0; i < value.spawnNumRange; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-range, range),
                                      Random.Range(-range, range),
                                      Random.Range(-range, range));
            Vector3 dir = new Vector3(Random.Range(-value.spawnRotation, value.spawnRotation),
                                      Random.Range(-value.spawnRotation, value.spawnRotation),
                                      0);

            fish = Instantiate(Resources.Load("Prefabs/Fish"), pos, Quaternion.Euler(dir)) as GameObject;
            fish.transform.SetParent(fishList.transform);
        }
        yield return null;
    }
}
