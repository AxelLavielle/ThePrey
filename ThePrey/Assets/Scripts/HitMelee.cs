using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitMelee : MonoBehaviour {

	float attackTimer;

	// Use this for initialization
	void Start () {
		attackTimer = 0.85f;
	}
	
	// Update is called once per frame
	void Update () {
		attackTimer -= Time.deltaTime;
	}


	public void reset()
	{
		attackTimer = 0.85f;
	}

	void OnTriggerStay(Collider collider) {
		if (attackTimer < 0 && collider.tag == "NPC") {
			collider.GetComponent<NPC> ().takeDamage ();
			attackTimer = 1.9f;
		}
	}
}