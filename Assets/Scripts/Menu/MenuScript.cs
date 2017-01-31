using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    private const string VOLUME_KEY = "VOLUME";
    private const string GOLD_KEY = "GOLD";

    public Sprite muteSound, normalSound;
    public Button soundButton;
    public Text scoreText, goldText;

    public void Awake()
    {
        Time.timeScale = 1;
        float volume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        AudioListener.volume = volume;
        if (volume == 0)
        {
            soundButton.GetComponent<Image>().sprite = muteSound;
        }
        int gold = PlayerPrefs.GetInt(GOLD_KEY, 0);
        goldText.text = " " + gold;
    }

    public void StartGame()
    {
        // "Stage1" is the name of the first scene we created.
        SceneManager.LoadScene("Stage1", LoadSceneMode.Single);
    }

    public void ToggleAudio()
    {
        if (AudioListener.volume == 0)
        {
            AudioListener.volume = 1;
            soundButton.GetComponent<Image>().sprite = normalSound;
        }
        else
        {
            AudioListener.volume = 0;
            soundButton.GetComponent<Image>().sprite = muteSound;
        }
        PlayerPrefs.SetFloat(VOLUME_KEY, AudioListener.volume);
    }

    public void updateScore()
    {
        scoreText.text = GameHelper.Instance.LoadBestScore().ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

}
