﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
	// public vars
	public float walkSpeed = 6;
	public float rotateSpeed = 3;

	// System vars
	Vector3 moveAmount;
	Vector3 smoothMoveVelocity;
	Rigidbody rigidbody;

	bool sneak;
	bool run;

	// Ressources
	public int life = 10;
	public float stamina = 2;

	void Awake() {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		rigidbody = GetComponent<Rigidbody> ();
		sneak = false;
		run = true;
	}

	void Update() {

		// Calculate movement:
		float inputX = Input.GetAxisRaw("Horizontal");
		float inputY = Input.GetAxisRaw("Vertical");
        float mouseX = Input.GetAxis("Mouse X");

        Vector3 moveDir = new Vector3(inputX, 0, inputY).normalized;
		Vector3 targetMoveAmount = moveDir * walkSpeed;
		if (Input.GetButton ("Shift")) {
			targetMoveAmount /= 4;
			run = false;
			sneak = true;
			stamina += Time.deltaTime;
			if (stamina >= 2)
				stamina = 2;
		} else if (Input.GetButton ("Space") && (stamina == 2 || run)) {
			sneak = false;
			stamina -= Time.deltaTime;
			run = true;
			if (stamina == 0)
				run = false;
			targetMoveAmount *= 1.75f;
		} else {
			run = false;
			sneak = false;
			stamina += Time.deltaTime;
			if (stamina >= 2)
				stamina = 2;
		}
		moveAmount = Vector3.SmoothDamp(moveAmount,targetMoveAmount,ref smoothMoveVelocity,.15f);
		transform.Rotate(Vector3.up * mouseX * rotateSpeed);

	}

	void FixedUpdate() {
		// Apply movement to rigidbody
		Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
		rigidbody.MovePosition(rigidbody.position + localMove);
	}
}
