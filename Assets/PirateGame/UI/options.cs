using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using PirateGame;

public class options : MonoBehaviour
{
    public Slider VolumeSlider,SFX_Slider;
    public AudioSource musicSource;
    //public AudioSource SPX => GameObject.Find("SoundEffect").GetComponent<AudioSource>();

    [SerializeField] bool isPaused =false;
    [SerializeField] GameObject Target;

    private void Awake()
    {
        try {
            musicSource = GameObject.Find("Music_Player").GetComponent<AudioSource>();
        } catch(UnityException e) {
            Debug.Log(e.StackTrace);
        }
    }

    private void Start()
    {
        PlayerPrefs.SetFloat("SFX_Volume",SFX_Slider.value);
    }

    private void Update()
    {
        if(musicSource != null){
            musicSource.volume = VolumeSlider.value;
        }
	}
	
	// Invoked when the value of the slider changes.
	public void SetAllSFX_Values()
	{
        
        PlayerPrefs.SetFloat("SFX_Volume",SFX_Slider.value);
        var allSFX_Objects = FindObjectsOfType<SoundEffect>();
        foreach(SoundEffect se in allSFX_Objects){
            se.AudioSource.volume = SFX_Slider.value;
        }
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
