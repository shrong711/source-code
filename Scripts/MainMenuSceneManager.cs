using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSceneManager : MonoBehaviour
{
    public GameObject LoginPanel;

    public void LoadLoginPanel()
    {
        LoginPanel.SetActive(true);
    }

    public void LoadGame()
    {
        if (ApplicationManager.instance != null)
        {
            ApplicationManager.instance.LoadGame();
        }
    }

    public void QuitGame()
    {
        if (ApplicationManager.instance)
        {
            ApplicationManager.instance.QuitGame();
        }
    }
	
}
