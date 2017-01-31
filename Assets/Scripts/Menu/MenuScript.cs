using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : CommunScript
{
    public Sprite muteSound, normalSound;
    public Button soundButton, rightButton, leftButton, buyButton, startButton;
    public Image playerShip;
    public Text scoreText, goldText;

    private int menuPos = 0;

    public void Awake()
    {
        Time.timeScale = 1;
        load();
        
        AudioListener.volume = playerPref.volume;
        if (playerPref.volume == 0)
        {
            soundButton.GetComponent<Image>().sprite = muteSound;
        }
        
        goldText.text = " " + playerPref.gold;

        menuPos = playerPref.currentShip;
        loadMenuPos();
    }


    public void right()
    {
        menuPos++;
        startButton.GetComponent<Animator>().enabled = false;
        loadMenuPos();
    }

    public void left()
    {
        menuPos--;
        startButton.GetComponent<Animator>().enabled = false;
        loadMenuPos();
    }

    private void loadMenuPos()
    {
        playerShip.sprite = Resources.Load<Sprite>(ships[menuPos].sprite);
        if (menuPos != 0 && !playerPref.ships.Contains(menuPos))
        {
            buyButton.gameObject.SetActive(true);
            buyButton.GetComponentInChildren<Text>().text = ships[menuPos].price.ToString();
            buyButton.interactable = playerPref.gold >= ships[menuPos].price;
        }
        else
        {
            buyButton.gameObject.SetActive(false);
        }
        startButton.gameObject.SetActive(!buyButton.gameObject.activeSelf);

        rightButton.gameObject.SetActive(menuPos < ships.Count - 1);
        leftButton.gameObject.SetActive(menuPos != 0);
    }

    public void buyShip()
    {
        if (playerPref.gold >= ships[menuPos].price)
        {
            playerPref.ships.Add(menuPos);
            playerPref.gold -= ships[menuPos].price;
            goldText.text = playerPref.gold.ToString();
            save();
            buyButton.gameObject.SetActive(false);
            startButton.gameObject.SetActive(true);
        }
    }

    public void StartGame()
    {
        playerPref.currentShip = menuPos;
        save();
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
        playerPref.volume = AudioListener.volume;
        save();
    }

    public void updateScore()
    {
        scoreText.text = playerPref.bestScore.ToString();
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
