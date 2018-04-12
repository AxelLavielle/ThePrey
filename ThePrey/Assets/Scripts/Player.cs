using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	// public vars
	public float walkSpeed = 6;
	public float rotateSpeed = 3;
    public bool isInBush = false;

	// System vars
	Vector3 moveAmount;
	Vector3 smoothMoveVelocity;
	Rigidbody rigidbody;
    Animator _animator;
	GameObject staminabar;

	// Footprints vars
    GameObject footprintCopy;
    bool nextIsLeft = true;
    float distanceSinceLastStep = 0;

    //Animator vars
    bool sneak;
	bool run;
    bool walk;
    public int life = 10;

    // Ressources
    public float maxStamina = 2;
    private float stamina;

	void Awake() {
        _animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		rigidbody = GetComponent<Rigidbody> ();
        walk = false;
        sneak = false;
		run = false;
        stamina = maxStamina;
        footprintCopy = GameObject.FindGameObjectWithTag("Asset");
		staminabar = GameObject.FindGameObjectWithTag ("staminabar");
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
            walk = false;
            stamina += Time.deltaTime;
			if (stamina >= maxStamina)
				stamina = maxStamina;
		} else if (Input.GetButton ("Space") && (stamina == maxStamina || run)) {
            walk = false;
			sneak = false;
			stamina -= Time.deltaTime;
			run = true;
			if (stamina <= 0)
				run = false;
			targetMoveAmount *= 1.75f;
		} else {
            walk = true;
			run = false;
			sneak = false;
			stamina += Time.deltaTime;
			if (stamina >= maxStamina)
				stamina = maxStamina;
		}
		moveAmount = Vector3.SmoothDamp(moveAmount,targetMoveAmount,ref smoothMoveVelocity,.15f);
		if (Mathf.Abs(moveAmount.x) + Mathf.Abs(moveAmount.y) + Mathf.Abs(moveAmount.z) < 1 && moveAmount.x + moveAmount.y + moveAmount.z > -1) {
			walk = false;
			run = false;
		}

        _animator.SetBool("walk", walk);
        _animator.SetBool("run", run);
        _animator.SetInteger("hp", life);
		staminabar.GetComponent<RectTransform>().sizeDelta = new Vector2(500 * stamina, 50);
    }

	void OnMouseDown()
	{
		_animator.SetBool ("shoot", true);
		transform.GetChild (0).gameObject.SetActive(true);
        transform.GetChild(0).gameObject.GetComponent<HitMelee>().reset();

    }

	void OnMouseUp()
	{
		_animator.SetBool("shoot", false);
		transform.GetChild (0).gameObject.SetActive(false);
	}

	void FixedUpdate() {
		// Apply movement to rigidbody
		Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
		rigidbody.MovePosition(rigidbody.position + localMove);
        distanceSinceLastStep += Mathf.Abs(localMove.x) + Mathf.Abs(localMove.y) + Mathf.Abs(localMove.z);

        if (distanceSinceLastStep > 2 && !sneak)
        {
            distanceSinceLastStep = 0;

            GameObject ft = Instantiate(footprintCopy, transform.parent);
            ft.name = "footprint";
            ft.transform.SetPositionAndRotation(transform.position, transform.rotation);

            Quaternion rot = transform.rotation;
            rot *= Quaternion.Euler(90, 0, 0);

            ft.transform.rotation = rot;

            if (nextIsLeft)
                ft.transform.position -= ft.transform.right / 3;
            else
                ft.transform.position += ft.transform.right / 3;
            nextIsLeft = !nextIsLeft;

            Vector3 pos = ft.transform.position;
            pos.y = transform.position.y + 0.1f;

            ft.transform.position = pos;
            ft.tag = "Visible";
            Destroy(ft, 10);
        }
    }

    public void takeDamage()
    {
        life -= 1;
        GameObject[] list = GameObject.FindGameObjectsWithTag("Heart");
		GameObject min;
		min = list [0];
    	foreach (GameObject g in list)
			if (g.name [7] > min.name [7])
				min = g;
        Destroy(min);
        _animator.SetTrigger("hit");
    }
}
