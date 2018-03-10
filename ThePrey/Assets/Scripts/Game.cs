using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour {

    private enum GameMode
    {
        E_SURVIVE,
        E_REVENGE,
        E_CTF
    };

    private float timer;
    public Text timerLabel;

	// Use this for initialization
	void Start ()
    {
        timer = 60f;
	}
	
	// Update is called once per frame
	void Update ()
    {
        timer -= Time.deltaTime;
        float minutes = timer / 60;
        float seconds = timer % 60;
        float fraction = (timer * 100) % 100;

        timerLabel.text = string.Format("{0:00} : {1:00} : {2:000}", minutes, seconds, fraction);

        if (timer <= 0)
        {
            timer = 60;
            //Win or loose
        }
	}
}
