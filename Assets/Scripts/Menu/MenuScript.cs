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

    public Transform hfPrefab, hfContentView;

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

    public void StartGame(GameObject panel)
    {
        playerPref.currentShip = menuPos;
        save();
        print("playerPref.currentMaxLevel: " + playerPref.currentMaxLevel);
        if (playerPref.currentMaxLevel > 1)
        {
            panel.SetActive(true);
            Button[] levelButtons = panel.GetComponentsInChildren<Button>();
            for (int i = 0; i < levelButtons.Length - 1; i++) {
                levelButtons[i].interactable = (i < playerPref.currentMaxLevel);
                //var lockImage = levelButtons[i].GetComponentInChildren<Image>();
                //lockImage.gameObject.SetActive(playerPref.currentMaxLevel >= i + 1);
            }
        }
        else
        {
            StartLevel(1);
        }
    }

    public void StartLevel(int level)
    {
        SceneManager.LoadScene("Stage" + level, LoadSceneMode.Single);
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

    public void loadLeaderBoardPanel()
    {
        scoreText.text = playerPref.bestScore.ToString();
    }

    public void loadHFPanel()
    {
        foreach (Transform child in hfContentView.transform)
        {
            Destroy(child.gameObject);
        }
        hfContentView.DetachChildren();

        foreach (HF hf in hfs[HF.TYPE_HF.Kill]) {
            var newHf = Instantiate(hfPrefab) as Transform;
            var hfTexts = newHf.GetComponentsInChildren<Text>();
            hfTexts[0].text = hf.description;
            hfTexts[1].text = " " + ((playerPref.kills > hf.nb)? hf.nb : playerPref.kills) + "/" + hf.nb;
            hfTexts[2].text = "+" + hf.gold;
            if (playerPref.kills < hf.nb)
                newHf.GetComponent<Image>().color = new Color32(100, 100, 100, 255);
            newHf.SetParent(hfContentView.transform, false);
        }
        foreach (HF hf in hfs[HF.TYPE_HF.Bonus])
        {
            var newHf = Instantiate(hfPrefab) as Transform;
            var hfTexts = newHf.GetComponentsInChildren<Text>();
            hfTexts[0].text = hf.description;
            hfTexts[1].text = " " + ((playerPref.bonus > hf.nb) ? hf.nb : playerPref.bonus) + "/" + hf.nb;
            hfTexts[2].text = "+" + hf.gold;
            if (playerPref.bonus < hf.nb)
                newHf.GetComponent<Image>().color = new Color32(100, 100, 100, 255);
            newHf.SetParent(hfContentView.transform, false);
        }
    }
    
    public void ResetGame()
    {
        playerPref = new PlayerPref();
        save();
        Awake();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
