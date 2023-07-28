using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorMovement : Movement
{
    public float speed;

    public float avoidanceSteeringForce;
    public float cohesionSteeringForce;

    public float feelerLength;
    public float searchRadius;

    Coroutine search;

    private void Awake()
    {
        List<float> list = GameObject.Find("GameManager").GetComponent<GameManager>().GetPredatorValue();
        speed                     = list[0];
        avoidanceSteeringForce    = list[1];
        cohesionSteeringForce     = list[2];
        feelerLength              = list[3];
        searchRadius              = list[4];
    }

    new void Start()
    {
        base.Start();

        rigid = GetComponent<Rigidbody>();

        search = StartCoroutine(SearchPrey());
    }

    void Update()
    {
        wallAvoidVec = WallAvoid(feelerLength) * avoidanceSteeringForce;
        cohesionVec = Cohesion() * cohesionSteeringForce;

        dirToLook = wallAvoidVec + cohesionVec + Revolution(50);
        dirToLook = Vector3.Lerp(transform.forward, dirToLook, Time.deltaTime);
        transform.localRotation = Quaternion.LookRotation(dirToLook);
    }

    private void FixedUpdate()
    {
        Vector3 vec = transform.forward * speed;
        rigid.velocity = vec;
    }

    IEnumerator SearchPrey()
    {
        if (objInSight.Count > 0)
            objInSight.Clear();

        Collider[] preyHits = Physics.OverlapSphere(transform.localPosition, searchRadius, preyMask);

        for (int i = 0; i < Mathf.Clamp(preyHits.Length, 0, 20); i++)
        {
            if (Vector3.Dot(preyHits[i].transform.position, transform.position) > 0)
                objInSight.Add(preyHits[i].gameObject);
        }

        yield return new WaitForSeconds(Random.Range(0.8f, 1.6f));
        search = StartCoroutine(SearchPrey());
    }

    //public void OnDrawGizmos()
    //{
        //Gizmos.color = Color.green;
        //Gizmos.DrawLine(transform.localPosition, transform.localPosition + transform.forward * value.feelerLength);
        //Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward + transform.right).normalized * value.feelerLength * 0.5f);
        //Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward - transform.right).normalized * value.feelerLength * 0.5f);
        //Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward + transform.up).normalized * value.feelerLength * 0.5f);
        //Gizmos.DrawLine(transform.localPosition, transform.localPosition + (transform.forward - transform.up).normalized * value.feelerLength * 0.5f);

        //Gizmos.color = Color.white;
        //Gizmos.DrawWireSphere(transform.position, value.searchDistance);
        //Gizmos.DrawLine(transform.localPosition, transform.localPosition + Cohesion() * value.feelerLength);
    //}
}
