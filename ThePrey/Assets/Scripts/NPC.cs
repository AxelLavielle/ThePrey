using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour {

    struct behavior {
        public Vector3 movement;
        public float rotation;
        public int type;

        public behavior(Vector3 m, float r, int t) {
            movement = m;
            rotation = r;
            type = t;
        }
    }

    // public vars
    public float walkSpeed = 6;
	public float rotateSpeed = 3;
    public float fieldOfViewDegrees = 130;
    public float viewDistance = 15;

    // System vars
    Vector3 moveAmount;
	Vector3 smoothMoveVelocity;
	Rigidbody rigidbody;

	bool sneak;
	bool run;

	// Ressources
	public int life = 10;
	public float maxStamina = 2;
    private float stamina;

    private List<GameObject> visible = new List<GameObject>();

    void Awake() {
		rigidbody = GetComponent<Rigidbody> ();
		sneak = false;
		run = true;
        stamina = maxStamina;
	}

    behavior Behavior()
    {
        return (new behavior(new Vector3(0, 0, 0), 0, 0));
    }

	void Update() {

		// Calculate movement
        behavior behaviorRet = Behavior();
		Vector3 targetMoveAmount = behaviorRet.movement * walkSpeed;

        if (behaviorRet.type == 1) {
			targetMoveAmount /= 4;
			run = false;
			sneak = true;
			stamina += Time.deltaTime;
			if (stamina >= maxStamina)
				stamina = maxStamina;
		} else if (behaviorRet.type == 2 && (stamina == maxStamina || run)) {
			sneak = false;
			stamina -= Time.deltaTime;
			run = true;
			if (stamina == 0)
				run = false;
			targetMoveAmount *= 1.75f;
		} else { /*walk or idle*/
			run = false;
			sneak = false;
			stamina += Time.deltaTime;
			if (stamina >= maxStamina)
				stamina = maxStamina;
		}

        // Must be NPC movements
        moveAmount = Vector3.SmoothDamp(moveAmount,targetMoveAmount,ref smoothMoveVelocity,.15f);
		transform.Rotate(Vector3.up * behaviorRet.rotation * rotateSpeed);

	}

	void FixedUpdate() {
		// Apply movement to rigidbody
		Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
		rigidbody.MovePosition(rigidbody.position + localMove);

        GetVision();
    }

    void GetVision()
    {
        visible.Clear();
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Visible");

        Vector3 eyePos = transform.position;
        eyePos.y += 2;

        foreach(GameObject obj in temp)
        {
            RaycastHit hit;
            Vector3 rayDirection = obj.transform.position - eyePos;

            if ((Vector3.Angle(rayDirection, transform.forward)) <= fieldOfViewDegrees * 0.5f)
            {
                // Detect if object is within the field of view
                if (Physics.Raycast(eyePos, rayDirection, out hit, viewDistance))
                {
                    visible.Add(obj);
                }
            }
        }

        RaycastHit hit2;
        Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
        playerPos.y += 1.5f;
        Vector3 rayDirection2 = playerPos - eyePos;

        if ((Vector3.Angle(rayDirection2, transform.forward)) <= fieldOfViewDegrees * 0.5f)
        {
            // Detect if object is within the field of view
            if (Physics.Raycast(eyePos, rayDirection2, out hit2, viewDistance))
            {
                visible.Add(GameObject.FindGameObjectWithTag("Player"));
            }
        }
    }
}
