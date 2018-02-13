using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(0, Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f, 0);
		transform.Translate(0, 0, Input.GetAxis("Vertical") * Time.deltaTime * 3.0f);
	}
}
