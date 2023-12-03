using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class GameObjectUtils
{
    public static void SafeDestroy(Func<GameObject> getGameObject)
    {
        try
        {
            if (getGameObject())
                UnityEngine.Object.Destroy(getGameObject());
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameObjectSafe] SafeDestroy :{e}");
        }
    }
}

public class GameData
{
    [Serializable]
    public class SystemValue
    {
        public float width;
        public float depth;
        public float height;

        public float spawnPosRange;
        public int spawnRotRange;
        public int preySpawnAmount;
        public int predatorSpawnAmount;
    }

    [Serializable]
    public class PredatorValue
    {
        public float speed;

        public float avoidanceSteeringForce;
        public float cohesionSteeringForce;

        public float feelerLength;
        public float searchRadius;
    }

    [Serializable]
    public class PreyValue
    {
        public float speed;

        public float avoidanceSteeringForce;
        public float cohesionSteeringForce;
        public float alignmentSteeringForce;
        public float separationSteeringForce;
        public float fleeSteeringForce;
        public float maxForce;

        public float feelerLength;
        public float searchRadius;
        public float fleeRadius;
    }
}

public class GameManager : MonoBehaviour
{
    [SerializeField] GameData.SystemValue   systemValue;
    [SerializeField] GameData.PredatorValue predatorValue;
    [SerializeField] GameData.PreyValue     preyValue;

    GameObject walls;

    [SerializeField] List<GameObject> preys;
    [SerializeField] List<GameObject> predators;

    Coroutine calculatorAvgFps;

    float avgFps = 0;

    void Awake()
    {
        Screen.SetResolution(1920, 1080, true);
    }

    void Start()
    {
        systemValue = JsonUtility.FromJson<GameData.SystemValue>(Resources.Load<TextAsset>("Json/System").text);
        predatorValue = JsonUtility.FromJson<GameData.PredatorValue>(Resources.Load<TextAsset>("Json/Predator").text);
        preyValue = JsonUtility.FromJson<GameData.PreyValue>(Resources.Load<TextAsset>("Json/prey").text);


        Cursor.lockState = CursorLockMode.Locked;

        preys = new List<GameObject>();
        predators = new List<GameObject>();

        walls = GameObject.Find("Walls");

        //CreateBox();
        SpawnPrey();
        SpawnPredator();
        calculatorAvgFps = StartCoroutine(CalculatorAvgFps());
    }

    void CreateBox()
    {
        walls.transform.Find("Top").localPosition       = new Vector3(0, systemValue.height * 0.5f, 0);
        walls.transform.Find("Bottom").localPosition    = new Vector3(0, -systemValue.height * 0.5f, 0);
        walls.transform.Find("North").localPosition     = new Vector3(0, 0, systemValue.depth * 0.5f);
        walls.transform.Find("South").localPosition     = new Vector3(0, 0, -systemValue.depth * 0.5f);
        walls.transform.Find("East").localPosition      = new Vector3(systemValue.width * 0.5f, 0, 0);
        walls.transform.Find("West").localPosition      = new Vector3(-systemValue.width * 0.5f, 0, 0);

        walls.transform.Find("Top").localScale          = new Vector3(systemValue.width * 0.1f, 1, systemValue.depth * 0.1f);
        walls.transform.Find("Bottom").localScale       = new Vector3(systemValue.width * 0.1f, 1, systemValue.depth * 0.1f);
        walls.transform.Find("North").localScale        = new Vector3(systemValue.width * 0.1f, 1, systemValue.height * 0.1f);
        walls.transform.Find("South").localScale        = new Vector3(systemValue.width * 0.1f, 1, systemValue.height * 0.1f);
        walls.transform.Find("East").localScale         = new Vector3(systemValue.height * 0.1f, 1, systemValue.depth * 0.1f);
        walls.transform.Find("West").localScale         = new Vector3(systemValue.height * 0.1f, 1, systemValue.depth * 0.1f);
    }

    public List<float> GetValue(string name)
    {
        List<float> returnList = new List<float>();

        if (name == "Prey")
        {
            returnList.Add(preyValue.speed);
            returnList.Add(preyValue.avoidanceSteeringForce);
            returnList.Add(preyValue.cohesionSteeringForce);
            returnList.Add(preyValue.alignmentSteeringForce);
            returnList.Add(preyValue.separationSteeringForce);
            returnList.Add(preyValue.fleeSteeringForce);
            returnList.Add(preyValue.searchRadius);
            returnList.Add(preyValue.fleeRadius);
            returnList.Add(systemValue.preySpawnAmount);
        }
        else if (name == "Predator")
        {
            returnList.Add(predatorValue.speed);
            returnList.Add(predatorValue.avoidanceSteeringForce);
            returnList.Add(predatorValue.cohesionSteeringForce);
            returnList.Add(predatorValue.searchRadius);
            returnList.Add(systemValue.predatorSpawnAmount);
        }

        return returnList;
    } 

    public void SetValue(List<float> list, string name)
    {
        if (name == "Prey")
        {
            preyValue.speed                     = list[0];
            preyValue.avoidanceSteeringForce    = list[1];
            preyValue.cohesionSteeringForce     = list[2];
            preyValue.alignmentSteeringForce    = list[3];
            preyValue.separationSteeringForce   = list[4];
            preyValue.fleeSteeringForce         = list[5];
            preyValue.searchRadius              = list[6];
            preyValue.fleeRadius                = list[7];
            systemValue.preySpawnAmount         = (int)list[8];

            Debug.Log("저장 완료");
            Debug.Log(preyValue.speed);
        }
        else if (name == "Predator")
        {
            predatorValue.speed                     = list[0];
            predatorValue.avoidanceSteeringForce    = list[1];
            predatorValue.cohesionSteeringForce     = list[2];
            predatorValue.searchRadius              = list[3];
            systemValue.predatorSpawnAmount         = (int)list[4];
        }
    }

    public void SpawnObjects()
    {
        if (preys.Count == 0 && predators.Count == 0)
        {
            SpawnPrey();
            SpawnPredator();
        }
    }

    public void ClearAllObjects()
    {
        if (preys.Count > 0 || predators.Count > 0)
        {
            for (int i = preys.Count - 1; i >= 0; i--)
            {
                GameObjectUtils.SafeDestroy(() => preys[i]);
            }
            preys.Clear();
            for (int i = predators.Count - 1; i >= 0; i--)
            {
                GameObjectUtils.SafeDestroy(() => predators[i]);
            }
            predators.Clear();
        }
    }

    void SpawnPrey()
    {
        GameObject prey;
        GameObject preyList = GameObject.Find("PreyList");

        for (int i = 0; i < systemValue.preySpawnAmount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-systemValue.width * systemValue.spawnPosRange, systemValue.width * systemValue.spawnPosRange),
                                      Random.Range(-systemValue.height * systemValue.spawnPosRange, systemValue.height * systemValue.spawnPosRange),
                                      Random.Range(-systemValue.depth * systemValue.spawnPosRange, systemValue.depth * systemValue.spawnPosRange));
            Vector3 dir = new Vector3(Random.Range(-systemValue.spawnRotRange, systemValue.spawnRotRange),
                                      Random.Range(-systemValue.spawnRotRange, systemValue.spawnRotRange),
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

        for (int i = 0; i < systemValue.predatorSpawnAmount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-systemValue.width * systemValue.spawnPosRange, systemValue.width * systemValue.spawnPosRange),
                                      Random.Range(-systemValue.height * systemValue.spawnPosRange, systemValue.height * systemValue.spawnPosRange),
                                      Random.Range(-systemValue.depth * systemValue.spawnPosRange, systemValue.depth * systemValue.spawnPosRange));

            predator = Instantiate(Resources.Load("Prefabs/Penguin"), pos, Quaternion.identity) as GameObject;
            predator.transform.LookAt(Vector3.zero);
            predator.transform.SetParent(predatorList.transform);
            predators.Add(predator);
        }
    }

    public List<float> GetPredatorValue()
    {
        List<float> list = new List<float>
        {
            predatorValue.speed,
            predatorValue.avoidanceSteeringForce,
            predatorValue.cohesionSteeringForce,
            predatorValue.feelerLength,
            predatorValue.searchRadius
        };

        return list;
    }

    public List<float> GetPreyValue()
    {
        List<float> list = new List<float>
        {
            preyValue.speed,
            preyValue.avoidanceSteeringForce,
            preyValue.cohesionSteeringForce,
            preyValue.alignmentSteeringForce,
            preyValue.separationSteeringForce,
            preyValue.fleeSteeringForce,
            preyValue.maxForce,
            preyValue.feelerLength,
            preyValue.searchRadius,
            preyValue.fleeRadius
        };

        return list;
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

    //void OnGUI()
    //{
    //    Rect position = new Rect(30, 20, Screen.width, Screen.height);

    //    GUIStyle style = new GUIStyle();
    //    style.fontSize = 50;
    //    style.normal.textColor = Color.black;

    //    GUI.Label(position, string.Format("{0:N1} FPS", avgFps), style);
    //}

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireCube(Vector3.zero, new Vector3(50, 50, 50));
    //}
}
