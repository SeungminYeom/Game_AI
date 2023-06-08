using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvaderMovement : MonoBehaviour
{
    [System.Serializable]
    class Value
    {
        public float speed;

        public float avoidanceSteeringForce;
        public float cohesionSteeringForce;
        public float alignmentSteeringForce;
        public float separationSteeringForce;

        public float feelerLength;
        public float searchDistance;
    }

    [SerializeField]
    Value value;

    Rigidbody rigid;

    List<Collider> collidersInSight;

    Coroutine search;

    Vector3 dirToLook;
    Vector3 avoidenceVec;
    Vector3 cohesionVec;
    Vector3 alignmentVec;
    Vector3 separationVec;

    int wallMask;
    int preyMask;
    int PredatorMask;

    private void Awake()
    {
        value = JsonUtility.FromJson<Value>(Resources.Load<TextAsset>("Json/Value").text);
        rigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        collidersInSight = new List<Collider>();

        wallMask        = 1 << LayerMask.NameToLayer("Wall");
        preyMask        = 1 << LayerMask.NameToLayer("Prey");
        PredatorMask    = 1 << LayerMask.NameToLayer("Predator");

        //search = StartCoroutine(Search());
    }

    void Update()
    {
        avoidenceVec = Avoidence();
        //cohesionVec = Cohesion() * value.cohesionSteeringForce;
        //alignmentVec = Alignment() * value.alignmentSteeringForce;
        //separationVec = Separation() * value.separationSteeringForce;

        dirToLook = avoidenceVec + cohesionVec + alignmentVec + separationVec;
        dirToLook = Vector3.Lerp(transform.forward, dirToLook, Time.deltaTime);
        transform.localRotation = Quaternion.LookRotation(dirToLook);
    }

    private void FixedUpdate()
    {
        Vector3 vec = transform.forward * value.speed;
        rigid.velocity = vec;
    }

    IEnumerator Search()
    {
        if (collidersInSight.Count > 0)
            collidersInSight.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.localPosition, value.searchDistance, preyMask);

        int hitsMax = Mathf.Clamp(hits.Length, 0, 30);

        //foreach (var hit in hits)
        for (int i = 0; i < hitsMax; i++)
        {
            if (Vector3.Dot(hits[i].transform.position, transform.position) > 0.96f)
                collidersInSight.Add(hits[i]);

            //if (Vector3.Dot(hit.transform.position, transform.position) > 0.96f)
            //    collidersInSight.Add(hit);
        }

        yield return new WaitForSeconds(Random.Range(0.3f, 0.7f));
        search = StartCoroutine(Search());
    }

    Vector3 Avoidence()
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
            //Vector3 steeringForce = proximateHit.normal * (feelerLength / proximateHit.distance) * value.avoidanceSteeringForce;
            Vector3 steeringForce = proximateHit.normal * Mathf.Pow((feelerLength / proximateHit.distance), 2);
            return steeringForce;
        }

        return transform.forward;
    }

    // 주변 물고기의 중심 벡터로 이동
    Vector3 Cohesion()
    {
        Vector3 centerVec = Vector3.zero;

        if (collidersInSight.Count > 1)
        {
            foreach (var hit in collidersInSight)
            {
                centerVec += hit.transform.position;
            }
        }
        else
        {
            return centerVec;
        }

        centerVec /= collidersInSight.Count;
        centerVec -= transform.position;
        centerVec.Normalize();

        return centerVec;
    }

    // 주변 물고기의 Heading 벡터의 평균으로 회전
    Vector3 Alignment()
    {
        Vector3 centerVec = Vector3.zero;

        if (collidersInSight.Count > 1)
        {
            foreach (var hit in collidersInSight)
            {
                centerVec += hit.transform.forward;
            }
        }
        else
        {
            return centerVec;
        }

        centerVec /= collidersInSight.Count;
        centerVec -= transform.forward;
        centerVec.Normalize();

        return centerVec;
    }

    // 주변 물고기와 거리 벌리기 
    Vector3 Separation()
    {
        Vector3 centerVec = Vector3.zero;

        if (collidersInSight.Count > 1)
        {
            foreach (var hit in collidersInSight)
            {
                Vector3 toThisFish = transform.position - hit.transform.position;
                if (toThisFish.magnitude < 0.0001f) continue;

                centerVec += toThisFish.normalized / toThisFish.magnitude;
            }
        }
        else
        {
            return centerVec;
        }

        return centerVec;
    }

    //public void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawLine(transform.localPosition, transform.localPosition + transform.forward * value.feelerLength);
    //    Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward + transform.right).normalized * value.feelerLength * 0.5f);
    //    Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward - transform.right).normalized * value.feelerLength * 0.5f);
    //    Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward + transform.up).normalized * value.feelerLength * 0.5f);
    //    Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward - transform.up).normalized * value.feelerLength * 0.5f);

    //    Gizmos.color = Color.white;
    //    Gizmos.DrawWireSphere(transform.position, value.searchDistance);
    //    Gizmos.DrawLine(transform.localPosition, transform.localPosition + Cohesion() * value.feelerLength);
    //}
}
