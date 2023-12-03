using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PreyMovement : Movement
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

    List<GameObject> predatorInAround;

    Coroutine searchNeighbor;
    Coroutine searchPredator;

    private void Awake()
    {
        List<float> list = GameObject.Find("GameManager").GetComponent<GameManager>().GetPreyValue();
        speed                     = list[0];
        avoidanceSteeringForce    = list[1];
        cohesionSteeringForce     = list[2];
        alignmentSteeringForce    = list[3];
        separationSteeringForce   = list[4];
        fleeSteeringForce         = list[5];
        maxForce                  = list[6];
        feelerLength              = list[7];
        searchRadius              = list[8];
        fleeRadius                = list[9];
    }

    new void Start()
    {
        base.Start();

        rigid = GetComponent<Rigidbody>();
        rigid.useGravity = false;

        predatorInAround = new List<GameObject>();

        searchNeighbor = StartCoroutine(SearchNeighbor());
        searchPredator = StartCoroutine(SearchPredator());
    }

    void Update()
    {
        if (isOutOfWater) return;

        wallAvoidVec = WallAvoid(feelerLength) * avoidanceSteeringForce;
        cohesionVec = Cohesion() * cohesionSteeringForce;
        alignmentVec = Alignment() * alignmentSteeringForce;
        separationVec = Separation() * separationSteeringForce;
        fleeVec = Flee(predatorInAround) * fleeSteeringForce;

        dirToLook = wallAvoidVec + cohesionVec + alignmentVec + separationVec + fleeVec + Revolution(15);
        dirToLook = Vector3.Lerp(transform.forward, dirToLook, Time.deltaTime);
        transform.localRotation = Quaternion.LookRotation(dirToLook);
    }

    private void FixedUpdate()
    {
        if (isOutOfWater) return;

        Vector3 vec = transform.forward * speed;
        rigid.velocity = vec;
    }

    IEnumerator SearchNeighbor()
    {
        if (objInSight.Count > 0)
            objInSight.Clear();

        Collider[] preyHits = Physics.OverlapSphere(transform.localPosition, searchRadius, preyMask);

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

        Collider[] predatorHits = Physics.OverlapSphere(transform.localPosition, fleeRadius, predatorMask);

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

        wallAvoidVec = WallAvoid(feelerLength) * avoidanceSteeringForce;
        if (!AccumulateForce(ref calculatedVec, wallAvoidVec))
            return calculatedVec;

        fleeVec = Flee(predatorInAround) * fleeSteeringForce;
        if (!AccumulateForce(ref calculatedVec, fleeVec))
            return calculatedVec;

        separationVec = Separation() * separationSteeringForce;
        if (!AccumulateForce(ref calculatedVec, separationVec))
            return calculatedVec;

        alignmentVec = Alignment() * alignmentSteeringForce;
        if (!AccumulateForce(ref calculatedVec, alignmentVec))
            return calculatedVec;

        cohesionVec = Cohesion() * cohesionSteeringForce;
        if (!AccumulateForce(ref calculatedVec, cohesionVec))
            return calculatedVec;

        return calculatedVec;
    }

    bool AccumulateForce(ref Vector3 runningTot, Vector3 forceToAdd)
    {
        float remainingSize = maxForce - runningTot.magnitude;

        if (remainingSize <= 0)
            return false;

        if (forceToAdd.magnitude < remainingSize)
            runningTot += forceToAdd;
        else
            runningTot += forceToAdd.normalized * remainingSize;

        return true;
    }

    /*
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.localPosition, transform.localPosition + transform.forward * value.feelerLength);
            Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward + transform.right).normalized * value.feelerLength * 0.5f);
            Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward - transform.right).normalized * value.feelerLength * 0.5f);
            Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward + transform.up).normalized * value.feelerLength * 0.5f);
            Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward - transform.up).normalized * value.feelerLength * 0.5f);
        Gizmos.DrawLine(transform.localPosition, transform.localPosition + FleeVec);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, value.searchDistance);
        Gizmos.DrawLine(transform.localPosition, transform.localPosition + Cohesion() * value.feelerLength);
    }
    */
}