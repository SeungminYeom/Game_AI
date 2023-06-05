using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [System.Serializable]
    class Value
    {
        public float speed;
        public float steering;
    }

    [SerializeField]
    Value value;

    Rigidbody rigidbody;

    private void Awake()
    {
        value       = JsonUtility.FromJson<Value>(Resources.Load<TextAsset>("Json/Value").text);
        rigidbody   = GetComponent<Rigidbody>();
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        rigidbody.velocity = transform.TransformDirection(Vector3.left) * value.speed;
    }
}
