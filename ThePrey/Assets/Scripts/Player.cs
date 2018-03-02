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

    GameObject footprintCopy;
    List<GameObject> footprints = new List<GameObject>();
    bool nextIsLeft = true;

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

        footprintCopy = GameObject.FindGameObjectWithTag("Asset");
        for (int i = 0; i < 10; ++i)
        {
            footprints.Add(Instantiate(footprintCopy));

            footprints[i].transform.SetParent(transform.parent);
            Vector3 pos = transform.localPosition;

            pos.y = 0.0001f;
            if (nextIsLeft)
                pos.x -= 0.3f;
            else
                pos.x += 0.3f;
            nextIsLeft = !nextIsLeft;
            footprints[i].transform.localPosition = pos;
        }

        sneak = false;
		run = true;
        stamina = maxStamina;
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
	}
}
