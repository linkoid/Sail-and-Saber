using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toggler : MonoBehaviour
{

    public void Toggle(GameObject TargetObject){
        TargetObject.SetActive(!TargetObject.activeSelf);
    }

}
