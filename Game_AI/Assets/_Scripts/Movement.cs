using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [System.Serializable]
    class Value
    {
        public float speed;

        public float avoidanceSteeringForce;
        public float cohesionSteeringForce;
        public float alignmentSteeringForce;
        public float separationSteeringForce;
        public float fleeSteeringForce;

        public float feelerLength;
        public float searchRadius;
        public float fleeRadius;
    }

    [SerializeField]
    Value value;

    Rigidbody rigid;

    List<Collider> collidersInSight;
    List<Collider> predatorInAround;

    Coroutine search;

    Vector3 dirToLook;
    Vector3 avoidanceVec;
    Vector3 cohesionVec;
    Vector3 alignmentVec;
    Vector3 separationVec;
    Vector3 FleeVec;

    int wallMask;
    int preyMask;
    int predatorMask;

    private void Awake()
    {
        value = JsonUtility.FromJson<Value>(Resources.Load<TextAsset>("Json/Prey").text);
        rigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        collidersInSight = new List<Collider>();
        predatorInAround = new List<Collider>();

        wallMask        = 1 << LayerMask.NameToLayer("Wall");
        preyMask        = 1 << LayerMask.NameToLayer("Prey");
        predatorMask    = 1 << LayerMask.NameToLayer("Predator");

        search = StartCoroutine(Search());
    }

    void Update()
    {
        avoidanceVec    = Avoidance();
        cohesionVec     = Cohesion() * value.cohesionSteeringForce;
        alignmentVec    = Alignment() * value.alignmentSteeringForce;
        separationVec   = Separation() * value.separationSteeringForce;
        FleeVec         = Flee() * value.fleeSteeringForce;
    }

    private void LateUpdate()
    {
        dirToLook = avoidanceVec + cohesionVec + alignmentVec + separationVec + FleeVec;
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
        if (collidersInSight.Count > 0) collidersInSight.Clear();
        if (predatorInAround.Count > 0) predatorInAround.Clear();

        Collider[] preyHits = Physics.OverlapSphere(transform.localPosition, value.searchRadius, preyMask);
        Collider[] PredatorHits = Physics.OverlapSphere(transform.localPosition, value.fleeRadius, predatorMask);

        for (int i = 0; i < Mathf.Clamp(preyHits.Length, 0, 20); i++)
        {
            if (Vector3.Dot(preyHits[i].transform.position, transform.position) > 0.96f)
                collidersInSight.Add(preyHits[i]);
        }

        foreach (var hit in PredatorHits)
        {
            predatorInAround.Add(hit);
        }

        yield return new WaitForSeconds(Random.Range(0.5f, 1f));
        search = StartCoroutine(Search());
    }

    Vector3 Avoidance()
    {
        RaycastHit[] hit = new RaycastHit[4];
        RaycastHit closeToHit;
        float feelerLength = value.feelerLength;

        Physics.Raycast(transform.localPosition, transform.forward, out closeToHit, value.feelerLength, wallMask);
        Physics.Raycast(transform.localPosition, transform.forward + transform.right, out hit[0], value.feelerLength * 0.5f, wallMask);
        Physics.Raycast(transform.localPosition, transform.forward - transform.right, out hit[1], value.feelerLength * 0.5f, wallMask);
        Physics.Raycast(transform.localPosition, transform.forward + transform.up, out hit[2], value.feelerLength * 0.5f, wallMask);
        Physics.Raycast(transform.localPosition, transform.forward - transform.up, out hit[3], value.feelerLength * 0.5f, wallMask);

        // 가장 가까운 hit 찾기
        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].collider == null) continue;
            if (closeToHit.collider == null)
            {
                closeToHit = hit[i];
                continue;
            }

            if ((closeToHit.transform.position - transform.position).sqrMagnitude > (hit[i].collider.transform.position - transform.position).sqrMagnitude)
            {
                closeToHit = hit[i];
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

        if (closeToHit.collider == null)
        {
            return transform.forward;
        }

        return closeToHit.normal * (feelerLength / closeToHit.distance) * value.avoidanceSteeringForce;
    }

    // 주변 물고기의 중심 벡터로 이동
    Vector3 Cohesion()
    {
        Vector3 dirVec = Vector3.zero;

        if (collidersInSight.Count > 1)
        {
            foreach (var hit in collidersInSight)
            {
                dirVec += hit.transform.position;
            }
        }
        else
        {
            return dirVec;
        }

        dirVec /= collidersInSight.Count;
        dirVec -= transform.position;
        dirVec.Normalize();

        return dirVec;
    }

    // 주변 물고기의 Heading 벡터의 평균으로 회전
    Vector3 Alignment()
    {
        Vector3 dirVec = Vector3.zero;

        if (collidersInSight.Count > 1)
        {
            foreach (var hit in collidersInSight)
            {
                dirVec += hit.transform.forward;
            }
        }
        else
        {
            return dirVec;
        }

        dirVec /= collidersInSight.Count;
        dirVec -= transform.forward;
        dirVec.Normalize();

        return dirVec;
    }

    // 주변 물고기와 거리 벌리기 
    Vector3 Separation()
    {
        Vector3 dirVec = Vector3.zero;

        if (collidersInSight.Count > 1)
        {
            foreach (var hit in collidersInSight)
            {
                Vector3 toThisFish = transform.position - hit.transform.position;
                if (toThisFish.magnitude < 0.0001f) continue;

                dirVec += toThisFish.normalized / toThisFish.magnitude;
            }
        }
        else
        {
            return dirVec;
        }

        return dirVec;
    }

    Vector3 Flee()
    {
        Vector3 dirVec = Vector3.zero;

        if (predatorInAround == null) return dirVec;

        foreach (var hit in predatorInAround)
        {
            Vector3 toThisFish = transform.position - hit.transform.position;
            if (toThisFish.magnitude < 0.0001f) continue;

            dirVec += toThisFish.normalized / toThisFish.magnitude;
        }

        //dirVec /= predatorInAround.Count;
        //dirVec -= transform.position;
        //dirVec.Normalize();

        return dirVec;
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