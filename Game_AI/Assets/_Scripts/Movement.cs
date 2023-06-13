using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    protected List<Collider> objInSight;

    protected Rigidbody rigid;

    protected int wallMask;
    protected int preyMask;
    protected int predatorMask;


    protected Vector3 dirToLook;
    protected Vector3 wallAvoidVec;
    protected Vector3 cohesionVec;
    protected Vector3 alignmentVec;
    protected Vector3 separationVec;
    protected Vector3 FleeVec;

    protected void Start()
    {
        objInSight = new List<Collider>();

        wallMask        = 1 << LayerMask.NameToLayer("Wall");
        preyMask        = 1 << LayerMask.NameToLayer("Prey");
        predatorMask    = 1 << LayerMask.NameToLayer("Predator");
    }

    // 달아나기
    protected Vector3 Flee(List<Collider> colliders)
    {
        Vector3 dirVec = Vector3.zero;

        if (colliders == null) return dirVec;

        foreach (var hit in colliders)
        {
            Vector3 toThisVec = transform.position - hit.transform.position;
            if (toThisVec.magnitude < 0.0001f) continue;

            dirVec += toThisVec.normalized / toThisVec.magnitude;
        }

        return dirVec;
    }

    // 벽 피하기
    protected Vector3 WallAvoid(float feelerLength)
    {
        RaycastHit[] hit = new RaycastHit[4];
        RaycastHit closeToHit;

        Physics.Raycast(transform.localPosition, transform.forward, out closeToHit, feelerLength * 2, wallMask);
        Physics.Raycast(transform.localPosition, transform.forward + transform.right, out hit[0], feelerLength, wallMask);
        Physics.Raycast(transform.localPosition, transform.forward - transform.right, out hit[1], feelerLength, wallMask);
        Physics.Raycast(transform.localPosition, transform.forward + transform.up, out hit[2], feelerLength, wallMask);
        Physics.Raycast(transform.localPosition, transform.forward - transform.up, out hit[3], feelerLength, wallMask);

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
            }
        }

        if (closeToHit.collider == null)
        {
            return transform.forward;
        }

        return closeToHit.normal * (feelerLength / closeToHit.distance);
    }

    // 주변 물고기의 중심 벡터로 이동
    protected Vector3 Cohesion()
    {
        Vector3 dirVec = Vector3.zero;

        if (objInSight.Count > 1)
        {
            foreach (var hit in objInSight)
            {
                dirVec += hit.transform.position;
            }
        }
        else
        {
            return dirVec;
        }

        dirVec /= objInSight.Count;
        dirVec -= transform.position;
        dirVec.Normalize();

        return dirVec;
    }

    // 주변 물고기의 Heading 벡터의 평균으로 회전
    protected Vector3 Alignment()
    {
        Vector3 dirVec = Vector3.zero;

        if (objInSight.Count > 1)
        {
            foreach (var hit in objInSight)
            {
                dirVec += hit.transform.forward;
            }
        }
        else
        {
            return dirVec;
        }

        dirVec /= objInSight.Count;
        dirVec -= transform.forward;
        dirVec.Normalize();

        return dirVec;
    }

    // 주변 물고기와 거리 벌리기 
    protected Vector3 Separation()
    {
        Vector3 dirVec = Vector3.zero;

        if (objInSight.Count > 1)
        {
            foreach (var hit in objInSight)
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
}