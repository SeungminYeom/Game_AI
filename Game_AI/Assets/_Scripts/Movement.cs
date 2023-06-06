using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [System.Serializable]
    class Value
    {
        public float speed;
        public float steeringForce;

        public float feelerLength;
    }

    [SerializeField]
    Value value;

    Rigidbody rigid;
    Vector3 wallAvoidenceVec;

    float deceleration = 1;

    private void Awake()
    {
        value       = JsonUtility.FromJson<Value>(Resources.Load<TextAsset>("Json/Value").text);
        rigid       = GetComponent<Rigidbody>();
    }

    void Start()
    {
    }

    void Update()
    {
        wallAvoidenceVec = WallAvoidence();
        transform.localRotation = Quaternion.LookRotation(wallAvoidenceVec);
    }

    private void FixedUpdate()
    {

        Vector3 vec = transform.forward * value.speed;
        rigid.velocity = vec;
    }

    Vector3 WallAvoidence()
    {
        RaycastHit[] hit = new RaycastHit[4];
        RaycastHit proximateHit;

        //6번 레이어의 정보, LayerMask는 bit flag임.
        int mask = (1 << 6);
        Physics.Raycast(transform.localPosition, transform.forward, out proximateHit, value.feelerLength, mask);
        Physics.Raycast(transform.localPosition, transform.forward + transform.right, out hit[0], value.feelerLength * 0.5f, mask);
        Physics.Raycast(transform.localPosition, transform.forward - transform.right, out hit[1], value.feelerLength * 0.5f, mask);
        Physics.Raycast(transform.localPosition, transform.forward + transform.up, out hit[2], value.feelerLength * 0.5f, mask);
        Physics.Raycast(transform.localPosition, transform.forward - transform.up, out hit[3], value.feelerLength * 0.5f, mask);

        // 가장 가까운 hit 찾기
        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].collider == null) continue;

            if (proximateHit.distance > hit[i].distance)
            {
                proximateHit = hit[i];
            }
        }

        /* 감속 코드
        if (hit.collider == null)
        {
            return 1.0f;
        }

        if (hit.distance / value.feelerLength < 0.3f)
        {
            return 0.0f;
        }

        return hit.distance / value.feelerLength;
        */

        if (proximateHit.collider != null)
        {
            Vector3 steeringForce = proximateHit.normal * (value.feelerLength - proximateHit.distance) * value.steeringForce;
            steeringForce = Vector3.Lerp(transform.forward, steeringForce, Time.deltaTime);
            return steeringForce;
            //transform.rotation = Quaternion.LookRotation(steeringForce);
        }

        return transform.forward;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.localPosition, transform.localPosition + transform.forward * value.feelerLength);
        Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward + transform.right).normalized * value.feelerLength * 0.5f);
        Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward - transform.right).normalized * value.feelerLength * 0.5f);
        Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward + transform.up).normalized * value.feelerLength * 0.5f);
        Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward - transform.up).normalized * value.feelerLength * 0.5f);
    }
}
