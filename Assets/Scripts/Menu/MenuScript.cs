using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{

    public Sprite muteSound, normalSound;

    public Text scoreText;

    public void Awake()
    {
        Time.timeScale = 1;
    }

    public void StartGame()
    {
        // "Stage1" is the name of the first scene we created.
        SceneManager.LoadScene("Stage1", LoadSceneMode.Single);
    }

    public void ToggleAudio(Button button)
    {
        print(AudioListener.volume);
        if (AudioListener.volume == 0)
        {
            AudioListener.volume = 1;
            button.GetComponent<Image>().sprite = normalSound;
        }
        else
        {
            AudioListener.volume = 0;
            button.GetComponent<Image>().sprite = muteSound;
        }

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
