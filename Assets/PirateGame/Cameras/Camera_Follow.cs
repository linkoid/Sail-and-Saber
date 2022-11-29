using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Follow : MonoBehaviour
{
    public Transform ToFollow;

    
    public Vector3 Offset;

    // Start is called before the first frame update
    void Start()
    {
        ToFollow = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = ToFollow.position + Offset;
        
        //ToFollow.rotation += Offset.rotation;
    }
}
