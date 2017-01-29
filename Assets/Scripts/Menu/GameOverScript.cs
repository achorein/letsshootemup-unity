using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour {

    private Button[] buttons;
    private Text[] texts;
    private Image[] images;

    public GameObject gamePanel;
    
    public Text winText;
    public Text loseText;

    public Text scoreText;
    public Text trophyText;

    void Awake()
    {
        // Get the buttons
        buttons = GetComponentsInChildren<Button>();
        texts = GetComponentsInChildren<Text>();
        images = GetComponentsInChildren<Image>(false);

        // Disable them
        HideButtons();
    }

    public void HideButtons()
    {
        Time.timeScale = 1;
        foreach (var b in buttons)
        {
            b.gameObject.SetActive(false);
        }
        foreach (var t in texts)
        {
            t.gameObject.SetActive(false);
        }
        foreach (var i in images)
        {
            if (i.gameObject.GetInstanceID() != GetInstanceID())
            {
                //i.gameObject.SetActive(false);
                i.enabled = false;
            }
        }
        gamePanel.SetActive(true);
        GetComponent<Image>().enabled = false;
    }

    public void ShowButtons(bool win)
    {
        Time.timeScale = 0;

        gamePanel.SetActive(false);
        GetComponent<Image>().enabled = true;
        scoreText.text = GameHelper.Instance.getScore().ToString();
        if (winText == null)
        {
            return;
        }

        GameHelper.Instance.SaveScore();
        trophyText.text = GameHelper.Instance.LoadBestScore().ToString();
        foreach (var b in buttons)
        {
            b.gameObject.SetActive(true);
        }
        foreach (var t in texts)
        {
            t.gameObject.SetActive(true);
        }
        foreach (var i in images)
        {
            i.enabled = true;
        }
        if (win) {
            loseText.gameObject.SetActive(false);
        } else {
            winText.gameObject.SetActive(false);
        }
    }

    public void ExitToMenu()
    {
        // Reload the level
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        // Reload the level
        SceneManager.LoadScene("Stage1", LoadSceneMode.Single);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ExitToMenu();
        }
    }

}
