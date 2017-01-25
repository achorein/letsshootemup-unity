using UnityEngine;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour {

    private Button[] buttons;
    private Text[] texts;

    public Text winText;
    public Text loseText;

    void Awake()
    {
        // Get the buttons
        buttons = GetComponentsInChildren<Button>();
        texts = GetComponentsInChildren<Text>();

        // Disable them
        HideButtons();
    }

    public void HideButtons()
    {
        foreach (var b in buttons)
        {
            b.gameObject.SetActive(false);
        }
        foreach (var t in texts)
        {
            t.gameObject.SetActive(false);
        }
    }

    public void ShowButtons(bool win)
    {
        if (winText == null)
        {
            return;
        }
        foreach (var b in buttons)
        {
            b.gameObject.SetActive(true);
        }
        foreach (var t in texts)
        {
            t.gameObject.SetActive(true);
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
        Application.LoadLevel("Menu");
    }

    public void RestartGame()
    {
        // Reload the level
        Application.LoadLevel("Stage1");
    }

}
