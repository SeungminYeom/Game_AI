using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    GameObject canvas;
    GameObject menu;

    CameraManager cameraManager;

    void Start()
    {
        cameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();

        canvas = GameObject.Find("Canvas");
        menu = canvas.transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!menu.activeSelf)
            {
                //tap.SetActive(false);
                menu.SetActive(true);

                Cursor.lockState = CursorLockMode.None;

                //Time.timeScale = 0.0f;
                //Time.fixedDeltaTime = Time.timeScale * 0.02f;

                cameraManager.FixedCamera(true);
            }
            else
            {
                //tap.SetActive(true);
                menu.SetActive(false);

                Cursor.lockState = CursorLockMode.Locked;

                //Time.timeScale = 1.0f;
                //  Time.fixedDeltaTime = Time.timeScale * 0.02f;

                cameraManager.FixedCamera(false);
            }
        }
    }

    public void ExitProgram()
    {
        Application.Quit();
    }
}
