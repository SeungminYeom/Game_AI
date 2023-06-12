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

    bool lockFlag;
    
    void Start()
    {
        target = GameObject.Find("Center");

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
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().Priority--;
                currentCamera = VC_Camera[1];
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().Priority++;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().Priority--;
                currentCamera = VC_Camera[2];
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().Priority++;
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
            }
        }
        else
        {
            if (currentCamera == VC_Camera[2])
            {
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().LookAt = target.transform;
                currentCamera.GetComponent<CinemachineVirtualCameraBase>().Follow = target.transform;
            }
        }
    }
}
