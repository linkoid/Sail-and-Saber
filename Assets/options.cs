using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class options : MonoBehaviour
{
    public Slider VolumeSlider;
    public AudioSource musicSource => GameObject.Find("Music_Player").GetComponent<AudioSource>();

    [SerializeField] bool isPaused =false;
    [SerializeField] GameObject Target;

    private void Start()
    {
    }

    private void Update()
    {
        musicSource.volume = VolumeSlider.value;
    }
    public void Resume(){
            isPaused = !isPaused;
            Time.timeScale = (isPaused) ? 0 : 1;
            Target.SetActive(isPaused);
    }

    void OnPause(InputValue input){
        if(!Target){
            return;
        }
        if(input.isPressed){
            Resume();
        }
	
    }

}
