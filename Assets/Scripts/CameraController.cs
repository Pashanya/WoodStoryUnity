using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{ 
    [SerializeField] private Transform target;

    [SerializeField] private float smoothTime = .15f;
    
    private Vector3 offset = new Vector3(0f, 0f, -10f);
    private Vector3 velocity = Vector3.zero;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        Vector3 targetPosition = target.position + offset;


        // Debug.Log(target.position.y - transform.position.y);
        
        // if (Mathf.Abs(target.position.y - transform.position.y) > 5) smoothTime = 0.01f;
        
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        
        // transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }
}
