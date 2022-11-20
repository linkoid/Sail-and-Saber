using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class options : MonoBehaviour
{
    [SerializeField] bool isPaused =false;
    [SerializeField] GameObject Target;
    void OnPause(InputValue input){
        if(input.isPressed){
            isPaused = !isPaused;
            Time.timeScale = (isPaused) ? 0 : 1;
            Target.SetActive(isPaused);
        }
	
    }

}
