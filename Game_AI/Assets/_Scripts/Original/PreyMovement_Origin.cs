using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyMovement_Origin : Movement_Origin
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
        public float maxForce;

        public float feelerLength;
        public float searchRadius;
        public float fleeRadius;

    }

    [SerializeField]
    Value value;

    List<GameObject> predatorInAround;

    Coroutine searchNeighbor;
    Coroutine searchPredator;

    private void Awake()
    {
        value = JsonUtility.FromJson<Value>(Resources.Load<TextAsset>("Json/Prey").text);
    }

    new void Start()
    {
        base.Start();

        rigid = GetComponent<Rigidbody>();

        predatorInAround = new List<GameObject>();

        searchNeighbor = StartCoroutine(SearchNeighbor());
        searchPredator = StartCoroutine(SearchPredator());
    }

    void Update()
    {
        //wallAvoidVec = WallAvoid(value.feelerLength) * value.avoidanceSteeringForce;
        //cohesionVec = Cohesion() * value.cohesionSteeringForce;
        //alignmentVec = Alignment() * value.alignmentSteeringForce;
        //separationVec = Separation() * value.separationSteeringForce;
        //FleeVec = Flee(predatorInAround) * value.fleeSteeringForce;

        //dirToLook = wallAvoidVec + cohesionVec + alignmentVec + separationVec + fleeVec;
        //dirToLook = Calculator();
        dirToLook = Calculator() + Revolution(15);
        dirToLook = Vector3.Lerp(transform.forward, dirToLook, Time.deltaTime);
        transform.localRotation = Quaternion.LookRotation(dirToLook);
    }

    private void FixedUpdate()
    {
        Vector3 vec = transform.forward * value.speed;
        rigid.velocity = vec;
    }

    IEnumerator SearchNeighbor()
    {
        if (objInSight.Count > 0)
            objInSight.Clear();

        Collider[] preyHits = Physics.OverlapSphere(transform.localPosition, value.searchRadius, preyMask);

        for (int i = 0; i < Mathf.Clamp(preyHits.Length, 0, 20); i++)
        {
            if (Vector3.Dot(preyHits[i].transform.position, transform.position) > 0.96f && preyHits[i].gameObject != gameObject)
                objInSight.Add(preyHits[i].gameObject);
        }

        yield return new WaitForSeconds(Random.Range(0.4f, 0.8f));
        searchNeighbor = StartCoroutine(SearchNeighbor());
    }

    IEnumerator SearchPredator()
    {
        if (predatorInAround.Count > 0) predatorInAround.Clear();

        Collider[] predatorHits = Physics.OverlapSphere(transform.localPosition, value.fleeRadius, predatorMask);

        foreach (var hit in predatorHits)
        {
            predatorInAround.Add(hit.gameObject);
        }

        yield return new WaitForSeconds(0.1f);
        searchNeighbor = StartCoroutine(SearchPredator());
    }

    Vector3 Calculator()
    {
        Vector3 calculatedVec = Vector3.zero;

        // 10일 때 40 전후의 값을 가짐
        wallAvoidVec = WallAvoid(value.feelerLength) * value.avoidanceSteeringForce;
        if (!AccumulateForce(ref calculatedVec, wallAvoidVec))
        {
            return calculatedVec;
        }

        // 70일 때 80~110
        //fleeVec = Flee(predatorInAround) * value.fleeSteeringForce;
        //if (!AccumulateForce(ref calculatedVec, fleeVec))
        //{
        //    //Debug.Log("벽에서 멀어지기");
        //    return calculatedVec;
        //}

        fleeVec = Evade(predatorInAround)/* * value.fleeSteeringForce*/;
        if (!AccumulateForce(ref calculatedVec, fleeVec))
        {
            //Debug.Log("벽에서 멀어지기");
            return calculatedVec;
        }


        // 8일 때 90~140
        separationVec = Separation() * value.separationSteeringForce;
        if (!AccumulateForce(ref calculatedVec, separationVec))
        {
            //Debug.Log("도망치기");
            return calculatedVec;
        }


        //if (calculatedVec.magnitude >= 58)
        //    Debug.Log(calculatedVec.magnitude);

        alignmentVec = Alignment() * value.alignmentSteeringForce;
        if (!AccumulateForce(ref calculatedVec, alignmentVec))
        {
            //Debug.Log("멀어지기");
            return calculatedVec;
        }

        cohesionVec = Cohesion() * value.cohesionSteeringForce;
        if (!AccumulateForce(ref calculatedVec, cohesionVec))
        {
            //Debug.Log("평균방향");
            return calculatedVec;
        }


        //if (calculatedVec.magnitude >= value.maxForce)
        //Debug.Log(calculatedVec.magnitude);


        return calculatedVec;
    }

    bool AccumulateForce(ref Vector3 runningTot, Vector3 forceToAdd)
    {
        float remainingSize = value.maxForce - runningTot.magnitude;

        if (remainingSize <= 0)
        {
            return false;
        }

        if (forceToAdd.magnitude < remainingSize)
            runningTot += forceToAdd;
        else
            runningTot += forceToAdd.normalized * remainingSize;

        return true;
    }

    //public void OnDrawGizmos()
    //{
        //Gizmos.color = Color.green;
        //    Gizmos.DrawLine(transform.localPosition, transform.localPosition + transform.forward * value.feelerLength);
        //    Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward + transform.right).normalized * value.feelerLength * 0.5f);
        //    Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward - transform.right).normalized * value.feelerLength * 0.5f);
        //    Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward + transform.up).normalized * value.feelerLength * 0.5f);
        //    Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward - transform.up).normalized * value.feelerLength * 0.5f);
        //Gizmos.DrawLine(transform.localPosition, transform.localPosition + FleeVec);

    //    Gizmos.color = Color.white;
    //    Gizmos.DrawWireSphere(transform.position, value.searchDistance);
    //    Gizmos.DrawLine(transform.localPosition, transform.localPosition + Cohesion() * value.feelerLength);
    //}
}