using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour {

    public enum GameModeEnum
    {
        E_REVENGE,
        E_SURVIVAL,
        E_CPF
    };

    private float timer;
    public Text timerLabel;

    public GameModeEnum mode;

    private GameObject player;
    private List<GameObject> NPCs = new List<GameObject>();

	// Use this for initialization
	void Start ()
    {
        timer = 60f;
        mode = (GameModeEnum)GameMode.mode;
        player = GameObject.FindGameObjectWithTag("Player");
        GameObject[] list = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject obj in list)
            NPCs.Add(obj);
	}
	
	// Update is called once per frame
	void Update ()
    {
        timer -= Time.deltaTime;
        float minutes = Mathf.Floor(timer / 60f);
        float seconds = Mathf.Floor(timer % 60f);
        float fraction = Mathf.Floor((timer * 100f) % 100f);

        timerLabel.text = string.Format("{0:00} : {1:00} : {2:000}", minutes, seconds, fraction);
        CheckNPC();
        CheckDefWin();
    }

    void CheckNPC()
    {
        for(int i = NPCs.Count - 1; i >= 0; --i)
        {
            if (NPCs[i].GetComponent<NPC>().life <= 0)
                NPCs.RemoveAt(i);
        }
    }

    void CheckDefWin()
    {

        // Check timer end
        if (timer <= 0)
        {
            if (mode == GameModeEnum.E_SURVIVAL)
                EndGame.victory = true;
            else
                EndGame.victory = false;
            SceneManager.LoadScene("Ending");
        }

        // Check player death
        if (player.GetComponent<Player>().life <= 0)
        {
            EndGame.victory = false;
            SceneManager.LoadScene("Ending");
        }

        // Check if no more NPCs
        if(NPCs.Count == 0 && mode != GameModeEnum.E_CPF)
        {
            EndGame.victory = true;
            SceneManager.LoadScene("Ending");
        }
    }

    public void PointCaptured()
    {
        if (mode == GameModeEnum.E_CPF)
        {
            EndGame.victory = true;
            SceneManager.LoadScene("Ending");
        }
    }
}
