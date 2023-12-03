using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderShark : MonoBehaviour
{
    public int rotateSpeed;

    private void Update()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rot.y += Time.deltaTime * rotateSpeed;
        transform.localRotation = Quaternion.Euler(0, rot.y, 0);
    }
}
