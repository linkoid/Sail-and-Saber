using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Scene_Transfer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Q()
    {
        Application.Quit();
    }
    
    public void TargetScene(string SceneToTransfer){
        if(SceneInBuild(SceneToTransfer)){
            
            Debug.Log(SceneToTransfer);
            SceneManager.LoadScene(SceneToTransfer);
        }else{
            
            Debug.Log(SceneToTransfer + " Does not exist in build index");
        }
        return; 
    }

    public void NextScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    //TODO binary search
    public bool SceneInBuild(string  name){
        Debug.Log(SceneManager.GetSceneByBuildIndex(0).name + " "  + SceneManager.sceneCountInBuildSettings);
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++){
                if( SceneManager.GetSceneByBuildIndex(i).name == name){
                    return true;
                }else {
                    Debug.Log( SceneManager.GetSceneByBuildIndex(i).name);
                }
        }
        return false;
    }
}
