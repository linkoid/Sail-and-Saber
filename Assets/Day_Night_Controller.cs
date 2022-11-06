using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Day_Night_Controller : MonoBehaviour
{

    [SerializeField] int Days =0;
    [SerializeField] int ticks =0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {


        ticks++;
            
        if(ticks % 21600 ==0){
            Days+=1;
            ticks = 0;
        }
        gameObject.transform.Rotate(.01f,0,0);
    }
}
