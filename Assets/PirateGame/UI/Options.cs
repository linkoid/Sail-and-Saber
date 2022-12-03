using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using PirateGame;

public class Options : MonoBehaviour
{
    public Slider VolumeSlider,SFX_Slider;
    //public AudioSource SPX => GameObject.Find("SoundEffect").GetComponent<AudioSource>();

    [SerializeField] bool isPaused =false;
    [SerializeField] GameObject Target;

    private void Awake()
    {
    }

    private void Start()
    {
		// Fixes volume defaulting to 0
		VolumeSlider.value = PlayerPrefs.GetFloat("Music_Volume", 0.4f);
		SFX_Slider.value   = PlayerPrefs.GetFloat("SFX_Volume"  , 0.8f);

		// Fixes settings not applying until volume sliders were moved
		MusicMaster.MusicController.GlobalVolume = VolumeSlider.value * 0.1f;
		SetAllSFX_Values();
	}

    private void Update()
    {
		PlayerPrefs.SetFloat("Music_Volume", VolumeSlider.value);
		MusicMaster.MusicController.GlobalVolume = VolumeSlider.value * 0.1f;
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

	public void ResetData()
	{
		PlayerPrefs.SetString("Fort1", "unCaptured");
		PlayerPrefs.SetString("Fort2", "unCaptured");
		PlayerPrefs.SetString("Fort3", "unCaptured");
	}
}
