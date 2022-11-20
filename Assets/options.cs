using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class options : MonoBehaviour
{
    public Slider VolumeSlider;
    public Music musicPlayer;

    [SerializeField] bool isPaused =false;
    [SerializeField] GameObject Target;
    private void Update()
    {
        musicPlayer.musicSource.volume = VolumeSlider.value;
    }
    void OnPause(InputValue input){
        if(!Target){
            return;
        }
        if(input.isPressed){
            isPaused = !isPaused;
            Time.timeScale = (isPaused) ? 0 : 1;
            Target.SetActive(isPaused);
        }
	
    }

}
