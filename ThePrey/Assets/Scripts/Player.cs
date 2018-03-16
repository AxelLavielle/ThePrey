using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	// public vars
	public float walkSpeed = 6;
	public float rotateSpeed = 3;

	// System vars
	Vector3 moveAmount;
	Vector3 smoothMoveVelocity;
	Rigidbody rigidbody;

    // Footprints vars
    GameObject footprintCopy;
    bool nextIsLeft = true;
    float distanceSinceLastStep = 0;

    bool sneak;
	bool run;

	// Ressources
	public int life = 10;
	public float maxStamina = 2;
    private float stamina;

    void Awake() {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		rigidbody = GetComponent<Rigidbody> ();
        sneak = false;
		run = true;
        stamina = maxStamina;
        footprintCopy = GameObject.FindGameObjectWithTag("Asset");
	}

	void Update() {

        // Calculate movement:
        float inputX = Input.GetAxisRaw("Horizontal");
		float inputY = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = new Vector3(inputX, 0, inputY).normalized;
		Vector3 targetMoveAmount = moveDir * walkSpeed;
		if (Input.GetButton ("Shift")) {
			targetMoveAmount /= 4;
			run = false;
			sneak = true;
			stamina += Time.deltaTime;
			if (stamina >= maxStamina)
				stamina = maxStamina;
		} else if (Input.GetButton ("Space") && (stamina == maxStamina || run)) {
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
			if (stamina >= maxStamina)
				stamina = maxStamina;
		}
		moveAmount = Vector3.SmoothDamp(moveAmount,targetMoveAmount,ref smoothMoveVelocity,.15f);

	}

	void FixedUpdate() {
		// Apply movement to rigidbody
		Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
		rigidbody.MovePosition(rigidbody.position + localMove);
        distanceSinceLastStep += Mathf.Abs(localMove.x) + Mathf.Abs(localMove.y) + Mathf.Abs(localMove.z);

        if(distanceSinceLastStep > 1)
        {
            distanceSinceLastStep = 0;

            GameObject ft = Instantiate(footprintCopy, transform.parent);
            ft.transform.SetPositionAndRotation(transform.localPosition, transform.localRotation);

            Quaternion rot = transform.localRotation;
            rot *= Quaternion.Euler(90, 0, 0);

            ft.transform.localRotation = rot;

            if (nextIsLeft)
                ft.transform.position -= ft.transform.right / 3;
            else
                ft.transform.position += ft.transform.right / 3;
            nextIsLeft = !nextIsLeft;

            Vector3 pos = ft.transform.localPosition;
            pos.y = 0.0001f;

            ft.transform.localPosition = pos;

            Destroy(ft, 10);
        }
    }
}
