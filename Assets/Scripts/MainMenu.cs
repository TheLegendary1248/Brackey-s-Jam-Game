using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public static MainMenu singleton;
    public GameObject canvas;
    public GameObject loadingScreen;
    public GameObject startScreen;
    public GameObject deathScreen;
    public Text[] timeScores;
    public Text depth;
    public Text timeAliveText;

    void Start()
    {
        singleton = this;
    }
    public void LoadGame()
    {
        Level.NextLevel();
        startScreen.SetActive(false);
    }
    public void ShowLoadingScreen(bool f)
    {
        loadingScreen.SetActive(f);
        canvas.SetActive(f);
    }
    public void ShowDeath(float[] times, float timeAlive)
    {
        for (int i = 0; i < 5; i++)
        {
            if (i >= times.Length - 1) timeScores[i].text = "NaN";
            else timeScores[i].text = $"{(int)times[i] / 60}m {string.Format("{0:F3}", times[i] % 60f)}s";
        }
        depth.text = $"Reach Depth {Level.depth}";
        timeAliveText.text = $"Time Alive {(int)timeAlive / 60}m {string.Format("{0:F3}", timeAlive % 60f)}s";
        deathScreen.SetActive(true);
    }
    public void AcceptDeath()
    {
        Level.depth = 0;
        deathScreen.SetActive(false);
        canvas.SetActive(true);
        startScreen.SetActive(true);
    }
    public void Exit() => Application.Quit();
}
