using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> VC_Camera;
    GameObject currentCamera;
    GameObject target;

    UI_Manager ui_Manager;

    float freeLookXSpeed;
    float freeLookYSpeed;

    bool lockFlag;
    
    void Start()
    {
        ui_Manager  = GameObject.Find("UI_Manager").GetComponent<UI_Manager>();
        target      = GameObject.Find("Center");

        VC_Camera.Add(GameObject.Find("VirtureCamera").transform.GetChild(0).gameObject);
        VC_Camera.Add(GameObject.Find("VirtureCamera").transform.GetChild(1).gameObject);
        VC_Camera.Add(GameObject.Find("VirtureCamera").transform.GetChild(2).gameObject);

        currentCamera = VC_Camera[0];
        currentCamera.GetComponent<CinemachineVirtualCameraBase>().Priority++;
    }

    void Update()
    {
        if (!lockFlag)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().Priority--;
                currentCamera = VC_Camera[0];
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().Priority++;
                ui_Manager.ChangeCameraName("Camera 1");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().Priority--;
                currentCamera = VC_Camera[1];
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().Priority++;
                ui_Manager.ChangeCameraName("Camera 2");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().Priority--;
                currentCamera = VC_Camera[2];
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().Priority++;
                ui_Manager.ChangeCameraName("Free Look Camera");
            }
        }
    }

    public void FixedCamera(bool lockFlag)
    {
        this.lockFlag = lockFlag;

        if (lockFlag)
        {
            if (currentCamera == VC_Camera[2])
            {
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().LookAt = null;
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().Follow = null;
                freeLookXSpeed = currentCamera.GetComponent<CinemachineFreeLook>().m_XAxis.m_MaxSpeed;
                freeLookYSpeed = currentCamera.GetComponent<CinemachineFreeLook>().m_YAxis.m_MaxSpeed;
                currentCamera.GetComponent<CinemachineFreeLook>().m_XAxis.m_MaxSpeed = 0;
                currentCamera.GetComponent<CinemachineFreeLook>().m_YAxis.m_MaxSpeed = 0;
            }
        }
        else
        {
            if (currentCamera == VC_Camera[2])
            {
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().LookAt       = target.transform;
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().Follow       = target.transform;
                currentCamera.GetComponent<CinemachineFreeLook>().m_XAxis.m_MaxSpeed    = freeLookXSpeed;
                currentCamera.GetComponent<CinemachineFreeLook>().m_YAxis.m_MaxSpeed    = freeLookYSpeed;
            }
        }
    }
}
