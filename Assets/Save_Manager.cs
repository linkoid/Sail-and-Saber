using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Save_Manager : MonoBehaviour
{
    // Start is called before the first frame update
    public static Save_Manager instance = null;  

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        if (instance == null)
            //if not, set it to this.
            instance = this;
        //If instance already exists:
        else if (instance != this)
            Destroy(gameObject);

    }

    public void ResetData(){
        PlayerPrefs.SetString("Fort1","unCaptured");
        PlayerPrefs.SetString("Fort2","unCaptured");
        PlayerPrefs.SetString("Fort3","unCaptured");
        PlayerPrefs.SetInt("PlayerHealth",100);

    }
}
