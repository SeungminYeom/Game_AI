using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [System.Serializable]
    class Value
    {
        public float speed;
        public float steeringForce;

        public float feelerLength;
        public float searchDistance;
    }

    [SerializeField]
    Value value;

    Rigidbody rigid;

    Vector3 dirToLook;
    Vector3 wallAvoidenceVec;
    [SerializeField] Vector3 cohesionVec;

    int wallMask;
    int fishMask;

    private void Awake()
    {
        value = JsonUtility.FromJson<Value>(Resources.Load<TextAsset>("Json/Value").text);
        rigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        wallMask = 1 << LayerMask.NameToLayer("Wall");
        fishMask = 1 << LayerMask.NameToLayer("Fish");
    }

    void Update()
    {
        wallAvoidenceVec = WallAvoidence();
        cohesionVec = Cohesion();
        dirToLook = wallAvoidenceVec + cohesionVec * 2f;
        dirToLook = Vector3.Lerp(transform.forward, dirToLook, Time.deltaTime);
        //cohesionVec = Vector3.Lerp(transform.forward, cohesionVec, Time.deltaTime);
        //wallAvoidenceVec = Vector3.Lerp(transform.forward, wallAvoidenceVec, Time.deltaTime);
        transform.localRotation = Quaternion.LookRotation(dirToLook);
        //transform.localRotation = Quaternion.LookRotation(cohesionVec);
        //transform.localRotation = Quaternion.LookRotation(wallAvoidenceVec);
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
        float feelerLength = value.feelerLength;

        Physics.Raycast(transform.localPosition, transform.forward, out proximateHit, value.feelerLength, wallMask);
        Physics.Raycast(transform.localPosition, transform.forward + transform.right, out hit[0], value.feelerLength * 0.5f, wallMask);
        Physics.Raycast(transform.localPosition, transform.forward - transform.right, out hit[1], value.feelerLength * 0.5f, wallMask);
        Physics.Raycast(transform.localPosition, transform.forward + transform.up, out hit[2], value.feelerLength * 0.5f, wallMask);
        Physics.Raycast(transform.localPosition, transform.forward - transform.up, out hit[3], value.feelerLength * 0.5f, wallMask);

        // 가장 가까운 hit 찾기
        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].collider == null) continue;

            if (proximateHit.distance > hit[i].distance)
            {
                proximateHit = hit[i];
                feelerLength = value.feelerLength * 0.5f;
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
            //Vector3 steeringForce = proximateHit.normal * (feelerLength - proximateHit.distance) * value.steeringForce;
            Vector3 steeringForce = proximateHit.normal * (feelerLength / proximateHit.distance) * value.steeringForce;
            //steeringForce = Vector3.Lerp(transform.forward, steeringForce, Time.deltaTime);
            return steeringForce;
        }

        return transform.forward;
    }

    // 주변 물고기의 중심 벡터로 이동
    Vector3 Cohesion()
    {
        Collider[] hits = Physics.OverlapSphere(transform.localPosition, value.searchDistance, fishMask);
        Vector3 centerVec = Vector3.zero;

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                centerVec += hit.transform.position;
            }
        }
        else
        {
            return centerVec;
        }

        centerVec /= hits.Length;
        centerVec -= transform.position;
        centerVec.Normalize();

        return centerVec;
    }

    // 주변 물고기의 Heading 벡터의 평균으로 회전


    // 주변 물고기와 거리 벌리기

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.localPosition, transform.localPosition + transform.forward * value.feelerLength);
        //Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward + transform.right).normalized * value.feelerLength * 0.5f);
        //Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward - transform.right).normalized * value.feelerLength * 0.5f);
        //Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward + transform.up).normalized * value.feelerLength * 0.5f);
        //Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward - transform.up).normalized * value.feelerLength * 0.5f);

        Gizmos.color = Color.white;
        //Gizmos.DrawWireSphere(transform.position, value.searchDistance);
        //Gizmos.DrawLine(transform.localPosition, transform.localPosition + Cohesion() * value.feelerLength);
    }
}