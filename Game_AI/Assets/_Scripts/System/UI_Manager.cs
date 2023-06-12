using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    GameObject canvas;
    GameObject menu;

    GameObject idle;
    GameObject spawnMenu;
    //GameObject value;
    //GameObject spawn;
    //GameObject clear;
    //GameObject original;
    //GameObject optimized;
    //GameObject back;

    CameraManager cameraManager;

    void Start()
    {
        cameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();

        canvas      = GameObject.Find("Canvas");
        menu        = canvas.transform.GetChild(0).gameObject;
        idle        = menu.transform.GetChild(1).gameObject;
        spawnMenu   = menu.transform.GetChild(2).gameObject;
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
                idle.SetActive(true);
                spawnMenu.SetActive(false);
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
}
