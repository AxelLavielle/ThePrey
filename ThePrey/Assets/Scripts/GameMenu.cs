using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenu : MonoBehaviour {
    public GameObject menuPause;
    public GameObject game;
    public GameObject mainCam;

	// Use this for initialization
	void Start () {
        print(GameMode.mode);
        mainCam = GameObject.Find("Main Camera");
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("escape"))
        {
            if (menuPause.activeSelf)
            {
                resumeGame();
                return;
            }
            menuPause.SetActive(true);
            mainCam.GetComponent<CameraFollow>().enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            game.SetActive(false);
        }
    }

    public void resumeGame()
    {
        menuPause.SetActive(false);
        mainCam.GetComponent<CameraFollow>().enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        game.SetActive(true);
    }
}
