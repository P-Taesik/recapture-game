using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    void Start(){
    
    }
    public void SceneChange(){
        Debug.Log("스테이지 1이동");
        SceneManager.LoadScene("Stage 1");
    }
}
