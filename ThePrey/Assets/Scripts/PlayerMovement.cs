using System.Collections;
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


	void Awake() {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		rigidbody = GetComponent<Rigidbody> ();
	}

	void Update() {

		// Calculate movement:
		float inputX = Input.GetAxisRaw("Horizontal");
		float inputY = Input.GetAxisRaw("Vertical");
        float mouseX = Input.GetAxis("Mouse X");

        Vector3 moveDir = new Vector3(inputX, 0, inputY).normalized;
		Vector3 targetMoveAmount = moveDir * walkSpeed;
		moveAmount = Vector3.SmoothDamp(moveAmount,targetMoveAmount,ref smoothMoveVelocity,.15f);
		transform.Rotate(Vector3.up * mouseX * rotateSpeed);

	}

	void FixedUpdate() {
		// Apply movement to rigidbody
		Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
		rigidbody.MovePosition(rigidbody.position + localMove);
	}
}
