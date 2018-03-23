using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapturePoint : MonoBehaviour {

    private int originalTime = 3;
    public float timer;
    public bool isCapturing = false;

    Game game;

    private void Start()
    {
        GameObject temp = GameObject.FindGameObjectWithTag("GameController");
        game = temp.GetComponent<Game>();
        timer = originalTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
            isCapturing = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            timer = originalTime;
            isCapturing = false;
        }
    }

    private void Update()
    {
        if (isCapturing)
            timer -= Time.deltaTime;
        if (timer <= 0)
            game.PointCaptured();
    }
}
