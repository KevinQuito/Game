using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        //SceneManager.LoadScene("SampleScene");    // Another way to change the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Scene will change to the next one, depending on the order of the build scene
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
