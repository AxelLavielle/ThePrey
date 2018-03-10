using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ending : MonoBehaviour {
    public Text endingMessage;


	// Use this for initialization
	void Start () {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (EndGame.victory)
            endingMessage.text = "VICTORY !";
        else
            endingMessage.text = "Defeat ...";
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
