using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameMode
{
    public static int mode = 0;
}

public class Menu : MonoBehaviour {
    string[] modes = new string[3];
    string[] descriptions = new string[3];
    public Text mode;
    public Text description;


    private void Start()
    {
        modes[0] = "Killing spree";
        modes[1] = "Survival";
        modes[2] = "Grand theft baby";

        descriptions[0] = "Kill all enemies before the timer goes off";
        descriptions[1] = "Survive the hunt for as long as you can";
        descriptions[2] = "Get your progeny out of the hunter's hand and flee with it";

        if (SceneManager.GetActiveScene().name == "ModeSelection")
        {
            mode.text = modes[0];
            description.text = descriptions[0];
        }
    }


    public void ModeSelection()
    {
        SceneManager.LoadScene(1);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Leave()
    {
        Application.Quit();
    }

    public void LeftSelection()
    {
        GameMode.mode--;
        if (GameMode.mode < 0)
            GameMode.mode = 2;
        mode.text = modes[GameMode.mode];
        description.text = descriptions[GameMode.mode];
    }

    public void RightSelection()
    {
        GameMode.mode++;
        GameMode.mode %= 3;
        mode.text = modes[GameMode.mode];
        description.text = descriptions[GameMode.mode];
    }

    public void StartMode()
    {
        SceneManager.LoadScene(2);
    }
}
