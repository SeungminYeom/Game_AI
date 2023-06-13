using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    class Value
    {
        public float    width;
        public float    depth;
        public float    height;

        public float    spawnPosRange;
        public int      spawnRotRange;
        public int      preySpawnAmount;
        public int      predatorSpawnAmount;
    }

    [SerializeField]
    Value value;

    GameObject walls;

    [SerializeField]
    List<GameObject> preys;
    [SerializeField]
    List<GameObject> predators;

    Coroutine calculatorAvgFps;

    float avgFps = 0;

    void Awake()
    {
        Screen.SetResolution(1920, 1080, true);

        value = JsonUtility.FromJson<Value>(Resources.Load<TextAsset>("Json/System").text);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        preys = new List<GameObject>();
        predators = new List<GameObject>();

        walls = GameObject.Find("Walls");

        CreateAquarium();
        SpawnPrey();
        SpawnPredator();
        calculatorAvgFps = StartCoroutine(CalculatorAvgFps());
    }

    void Update()
    {
    }

    void CreateAquarium()
    {
        walls.transform.Find("Top").localPosition       = new Vector3(0, value.height * 0.5f, 0);
        walls.transform.Find("Bottom").localPosition    = new Vector3(0, -value.height * 0.5f, 0);
        walls.transform.Find("North").localPosition     = new Vector3(0, 0, value.depth * 0.5f);
        walls.transform.Find("South").localPosition     = new Vector3(0, 0, -value.depth * 0.5f);
        walls.transform.Find("East").localPosition      = new Vector3(value.width * 0.5f, 0, 0);
        walls.transform.Find("West").localPosition      = new Vector3(-value.width * 0.5f, 0, 0);

        walls.transform.Find("Top").localScale          = new Vector3(value.width * 0.1f, 1, value.depth * 0.1f);
        walls.transform.Find("Bottom").localScale       = new Vector3(value.width * 0.1f, 1, value.depth * 0.1f);
        walls.transform.Find("North").localScale        = new Vector3(value.width * 0.1f, 1, value.height * 0.1f);
        walls.transform.Find("South").localScale        = new Vector3(value.width * 0.1f, 1, value.height * 0.1f);
        walls.transform.Find("East").localScale         = new Vector3(value.height * 0.1f, 1, value.depth * 0.1f);
        walls.transform.Find("West").localScale         = new Vector3(value.height * 0.1f, 1, value.depth * 0.1f);
    }

    void SpawnPrey()
    {
        GameObject prey;
        GameObject preyList = GameObject.Find("PreyList");

        for (int i = 0; i < value.preySpawnAmount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-value.width * value.spawnPosRange, value.width * value.spawnPosRange),
                                      Random.Range(-value.height * value.spawnPosRange, value.height * value.spawnPosRange),
                                      Random.Range(-value.depth * value.spawnPosRange, value.depth * value.spawnPosRange));
            Vector3 dir = new Vector3(Random.Range(-value.spawnRotRange, value.spawnRotRange),
                                      Random.Range(-value.spawnRotRange, value.spawnRotRange),
                                      0);

            prey = Instantiate(Resources.Load("Prefabs/Fish"), pos, Quaternion.Euler(dir)) as GameObject;
            prey.transform.SetParent(preyList.transform);
            preys.Add(prey);
        }
    }

    void SpawnPredator()
    {
        GameObject predator;
        GameObject predatorList = GameObject.Find("PredatorList");

        for (int i = 0; i < value.predatorSpawnAmount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-value.width * value.spawnPosRange, value.width * value.spawnPosRange),
                                      Random.Range(-value.height * value.spawnPosRange, value.height * value.spawnPosRange),
                                      Random.Range(-value.depth * value.spawnPosRange, value.depth * value.spawnPosRange));

            predator = Instantiate(Resources.Load("Prefabs/Penguin"), pos, Quaternion.identity) as GameObject;
            predator.transform.LookAt(Vector3.zero);
            predator.transform.SetParent(predatorList.transform);
            predators.Add(predator);
        }
    }

    void OnGUI()
    {
        Rect position = new Rect(30, 20, Screen.width, Screen.height);

        GUIStyle style = new GUIStyle();
        style.fontSize = 50;
        style.normal.textColor = Color.black;

        GUI.Label(position, string.Format("{0:N1} FPS", avgFps), style);
    }

    IEnumerator CalculatorAvgFps()
    {
        float beginTime = Time.time;
        float fpsSum = 0f;
        float length = 0;

        while (Time.time - beginTime < 0.5f)
        {
            fpsSum += 1.0f / Time.deltaTime;
            length++;

            yield return null;
        }

        avgFps = fpsSum / length;

        calculatorAvgFps = StartCoroutine(CalculatorAvgFps());
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(50, 50, 50));
    }
}
