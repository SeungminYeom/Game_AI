using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.EventSystems;
using System.ComponentModel.Design;

public class UI_Manager : MonoBehaviour
{
    [System.Serializable]
    class Comment
    {
        public string value;
        public string spawn;
        public string clear;
        public string prey;
        public string predator;
        public string save;
        public string back;
    };

    [SerializeField]
    Comment commentString;

    GameObject canvas;
    GameObject menu;
    GameObject idle;
    GameObject valueMenu;
    GameObject preyMenu;
    GameObject predatorMenu;
    GameObject cameraName;
    GameObject comment;

    GraphicRaycaster    raycast;
    EventSystem         eventSystem;
    PointerEventData    pointerEventData;

    CameraManager cameraManager;

    Dictionary<string, string> commentDict;

    float nameChangeStartTime;

    private void Awake()
    {
        commentString = JsonUtility.FromJson<Comment>(Resources.Load<TextAsset>("Json/Comment").text);
    }

    void Start()
    {
        cameraManager   = GameObject.Find("CameraManager").GetComponent<CameraManager>();
        canvas          = GameObject.Find("Canvas");
        menu            = canvas.transform.GetChild(0).gameObject;
        cameraName      = canvas.transform.GetChild(1).gameObject;
        idle            = menu.transform.GetChild(1).gameObject;
        valueMenu       = menu.transform.GetChild(2).gameObject;
        preyMenu        = menu.transform.GetChild(3).gameObject;
        predatorMenu    = menu.transform.GetChild(4).gameObject;
        comment         = menu.transform.GetChild(5).gameObject;

        raycast = canvas.GetComponent<GraphicRaycaster>();
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        commentDict = new Dictionary<string, string>();
        CreateDictionary();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!menu.activeSelf)
            {
                menu.SetActive(true);
                cameraManager.FixedCamera(true);

                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                idle.SetActive(true);
                valueMenu.SetActive(false);
                preyMenu.SetActive(false);
                predatorMenu.SetActive(false);
                menu.SetActive(false);
                cameraManager.FixedCamera(false);

                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if (cameraName.activeSelf && Time.time - nameChangeStartTime > 2f)
        {
            cameraName.SetActive(false);
        }

        if (menu.activeSelf)
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            raycast.Raycast(pointerEventData, results);

            string str = "";
            if (results.Count > 0)
            {
                commentDict.TryGetValue(results[0].gameObject.name, out str);
            }

            if (str != "")
            {
                comment.SetActive(true);
                comment.GetComponent<TextMeshProUGUI>().text = str;
            }
            else
            {
                comment.SetActive(false);
            }
        }
    }

    public void ChangeCameraName(string name)
    {
        cameraName.SetActive(true);
        cameraName.GetComponent<TextMeshProUGUI>().text = name;

        nameChangeStartTime = Time.time;
    }

    public void ExitProgram()
    {
        UnityEngine.Application.Quit();
    }

    public void ClickedValue()
    {
        idle.SetActive(false);
        valueMenu.SetActive(true);
    }

    public void ClickedPrey()
    {
        valueMenu.SetActive(false);
        preyMenu.SetActive(true);

        List<float> values = GameObject.Find("GameManager").GetComponent<GameManager>().GetValue("Prey");

        //Speed
        preyMenu.transform.GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = values[0].ToString();
        //Avoidance
        preyMenu.transform.GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text = values[1].ToString();
        //Cohesion
        preyMenu.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text = values[2].ToString();
        //Separation
        preyMenu.transform.GetChild(3).GetChild(0).GetComponent<TMP_InputField>().text = values[3].ToString();
        //Alignment
        preyMenu.transform.GetChild(4).GetChild(0).GetComponent<TMP_InputField>().text = values[4].ToString();
        //Flee
        preyMenu.transform.GetChild(5).GetChild(0).GetComponent<TMP_InputField>().text = values[5].ToString();
        //SearchRadius
        preyMenu.transform.GetChild(6).GetChild(0).GetComponent<TMP_InputField>().text = values[6].ToString();
        //FleeRadius
        preyMenu.transform.GetChild(7).GetChild(0).GetComponent<TMP_InputField>().text = values[7].ToString();
        //SpawnAmount
        preyMenu.transform.GetChild(8).GetChild(0).GetComponent<TMP_InputField>().text = values[8].ToString();
    }

    public void ClickedPredator()
    {
        valueMenu.SetActive(false);
        predatorMenu.SetActive(true);

        List<float> values = GameObject.Find("GameManager").GetComponent<GameManager>().GetValue("Predator");

        //Speed
        predatorMenu.transform.GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = values[0].ToString();
        //Avoidance
        predatorMenu.transform.GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text = values[1].ToString();
        //Cohesion
        predatorMenu.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text = values[2].ToString();
        //SearchRadius
        predatorMenu.transform.GetChild(3).GetChild(0).GetComponent<TMP_InputField>().text = values[3].ToString();
        //SpawnAmount
        predatorMenu.transform.GetChild(4).GetChild(0).GetComponent<TMP_InputField>().text = values[4].ToString();
    }

    public void ClickedBack(GameObject currentUI)
    {
        if (currentUI == preyMenu)
        {
            preyMenu.SetActive(false);
            valueMenu.SetActive(true);
        }
        else if (currentUI == predatorMenu)
        {
            predatorMenu.SetActive(false);
            valueMenu.SetActive(true);
        }
        else if (currentUI == valueMenu)
        {
            valueMenu.SetActive(false);
            idle.SetActive(true);
        }
    }

    public void ClickedSave(GameObject currentUI)
    {
        List<float> list = new List<float>();

        if (currentUI == preyMenu)
        {
            //Speed
            list.Add(float.Parse(preyMenu.transform.GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text));
            //Avoidance
            list.Add(float.Parse(preyMenu.transform.GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text));
            //Cohesion
            list.Add(float.Parse(preyMenu.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text));
            //Separation
            list.Add(float.Parse(preyMenu.transform.GetChild(3).GetChild(0).GetComponent<TMP_InputField>().text));
            //Alignment
            list.Add(float.Parse(preyMenu.transform.GetChild(4).GetChild(0).GetComponent<TMP_InputField>().text));
            //Flee
            list.Add(float.Parse(preyMenu.transform.GetChild(5).GetChild(0).GetComponent<TMP_InputField>().text));
            //SearchRadius
            list.Add(float.Parse(preyMenu.transform.GetChild(6).GetChild(0).GetComponent<TMP_InputField>().text));
            //FleeRadius
            list.Add(float.Parse(preyMenu.transform.GetChild(7).GetChild(0).GetComponent<TMP_InputField>().text));
            //SpawnAmount
            list.Add(float.Parse(preyMenu.transform.GetChild(8).GetChild(0).GetComponent<TMP_InputField>().text));

            GameObject.Find("GameManager").GetComponent<GameManager>().SetValue(list, "Prey");
            preyMenu.SetActive(false);
        }
        else if (currentUI == predatorMenu)
        {
            //Speed
            list.Add(float.Parse(predatorMenu.transform.GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text));
            //Avoidance
            list.Add(float.Parse(predatorMenu.transform.GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text));
            //Cohesion
            list.Add(float.Parse(predatorMenu.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text));
            //SearchRadius
            list.Add(float.Parse(predatorMenu.transform.GetChild(3).GetChild(0).GetComponent<TMP_InputField>().text));
            //SpawnAmount
            list.Add(float.Parse(predatorMenu.transform.GetChild(4).GetChild(0).GetComponent<TMP_InputField>().text));

            GameObject.Find("GameManager").GetComponent<GameManager>().SetValue(list, "Predator");
            predatorMenu.SetActive(false);
        }


        valueMenu.SetActive(true);
    }

    void CreateDictionary()
    {
        commentDict.Add("Value", commentString.value);
        commentDict.Add("Spawn", commentString.spawn);
        commentDict.Add("Clear", commentString.clear);
        commentDict.Add("Prey", commentString.prey);
        commentDict.Add("Predator", commentString.predator);
        commentDict.Add("Save", commentString.save);
        commentDict.Add("Back", commentString.back);
    }
}
