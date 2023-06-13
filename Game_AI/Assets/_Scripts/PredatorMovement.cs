using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorMovement : Movement
{
    [System.Serializable]
    class Value
    {
        public float speed;

        public float avoidanceSteeringForce;
        public float cohesionSteeringForce;

        public float feelerLength;
        public float searchRadius;
    }

    [SerializeField]
    Value value;

    Coroutine search;

    private void Awake()
    {
        List<float> list = GameObject.Find("GameManager").GetComponent<GameManager>().GetPredatorValue();
        value.speed                     = list[0];
        value.avoidanceSteeringForce    = list[1];
        value.cohesionSteeringForce     = list[2];
        value.feelerLength              = list[3];
        value.searchRadius              = list[4];
    }

    new void Start()
    {
        base.Start();

        rigid = GetComponent<Rigidbody>();

        search = StartCoroutine(SearchPrey());
    }

    void Update()
    {
        wallAvoidVec = WallAvoid(value.feelerLength) * value.avoidanceSteeringForce;
        cohesionVec = Cohesion() * value.cohesionSteeringForce;

        dirToLook = wallAvoidVec + cohesionVec + Revolution(50);
        dirToLook = Vector3.Lerp(transform.forward, dirToLook, Time.deltaTime);
        transform.localRotation = Quaternion.LookRotation(dirToLook);
    }

    private void FixedUpdate()
    {
        Vector3 vec = transform.forward * value.speed;
        rigid.velocity = vec;
    }

    IEnumerator SearchPrey()
    {
        if (objInSight.Count > 0)
            objInSight.Clear();

        Collider[] preyHits = Physics.OverlapSphere(transform.localPosition, value.searchRadius, preyMask);

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
