using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using PirateGame;

public class Options : MonoBehaviour
{
    public Slider VolumeSlider,SFX_Slider;
    public AudioSource musicSource;
    //public AudioSource SPX => GameObject.Find("SoundEffect").GetComponent<AudioSource>();

    [SerializeField] bool isPaused =false;
    [SerializeField] GameObject Target;

    private void Awake()
    {
        // Fixes NullReferenceException s
        musicSource = GameObject.Find("Music_Player")?.GetComponent<AudioSource>();
    }

    private void Start()
    {
		// Fixes volume defaulting to 0
		VolumeSlider.value = PlayerPrefs.GetFloat("Music_Volume", 0.8f);
		SFX_Slider.value   = PlayerPrefs.GetFloat("SFX_Volume"  , 0.8f);

        // Fixes settings not applying until volume sliders were moved
        SetAllSFX_Values();
	}

    private void Update()
    {
        if(musicSource != null){
            musicSource.volume = VolumeSlider.value;
			// Fixes music volume not being saved
			PlayerPrefs.SetFloat("Music_Volume", VolumeSlider.value);
		}
	}
	
	// Invoked when the value of the slider changes.
	public void SetAllSFX_Values()
	{
        PlayerPrefs.SetFloat("SFX_Volume",SFX_Slider.value);
        // Fixes settings not applying to newly generated SoundEffects
        SoundEffect.GlobalVolume = SFX_Slider.value;
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
