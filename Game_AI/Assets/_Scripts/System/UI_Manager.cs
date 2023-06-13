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
        public string original;
        public string optimized;
        public string back;
    };

    [SerializeField]
    Comment commentString;

    GameObject canvas;
    GameObject menu;
    GameObject idle;
    GameObject spawnMenu;
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
        spawnMenu       = menu.transform.GetChild(2).gameObject;
        comment         = menu.transform.GetChild(3).gameObject;

        raycast = canvas.GetComponent<GraphicRaycaster>();
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        commentDict = new Dictionary<string, string>();
        CreateDictionary();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
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
                spawnMenu.SetActive(false);
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
                if (commentDict.ContainsKey(results[0].gameObject.name))
                {
                    commentDict.TryGetValue(results[0].gameObject.name, out str);
                }
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
    }

    public void ClickedSpawn()
    {
        idle.SetActive(false);
        spawnMenu.SetActive(true);
    }

    public void ClickedOriginal()
    {

    }

    public void ClickedOptimized()
    {

    }

    public void ClickedBack()
    {
        spawnMenu.SetActive(false);
        idle.SetActive(true);
    }

    public void ClickedClear()
    {

    }

    void CreateDictionary()
    {
        commentDict.Add("Value", commentString.value);
        commentDict.Add("Spawn", commentString.spawn);
        commentDict.Add("Clear", commentString.clear);
        commentDict.Add("Original", commentString.original);
        commentDict.Add("Optimized", commentString.optimized);
        commentDict.Add("Back", commentString.back);
    }
}
