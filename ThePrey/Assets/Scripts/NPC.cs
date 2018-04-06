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
    public float viewDistance = 10;
    public float shootTimer = 1f;

    // System vars
    Vector3 moveAmount;
	Vector3 smoothMoveVelocity;
	Rigidbody rigidbody;
    Animator _animator;

    //Animator vars
    bool walk;
    bool run;
    bool shoot;

    Vector3 _target;
    Vector3 _rotation;
    Vector3 _vel;

    float t = 0.4f;
    float maxAcc = 0.1f;
    float maxVel = 5;
    float rotationSpeed = 35f;

	// Ressources
	public int life = 10;
	public float maxStamina = 2;
    private float stamina;

    private List<GameObject> visible = new List<GameObject>();

    void Awake() {
		rigidbody = GetComponent<Rigidbody> ();
        _animator = GetComponent<Animator>();
        walk = false;
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

        shootTimer -= Time.deltaTime;

        if (behaviorRet.type == 1) {
			targetMoveAmount /= 4;
			run = false;
			walk = true;
			stamina += Time.deltaTime;
			if (stamina >= maxStamina)
				stamina = maxStamina;
		} else if (behaviorRet.type == 2 && (stamina == maxStamina || run)) {
			walk = false;
			stamina -= Time.deltaTime;
			run = true;
			if (stamina == 0)
				run = false;
			targetMoveAmount *= 1.75f;
		} else { /*walk or idle*/
			run = false;
			walk = false;
			stamina += Time.deltaTime;
			if (stamina >= maxStamina)
				stamina = maxStamina;
		}

        _animator.SetBool("walk", walk);
        _animator.SetBool("run", run);
        _animator.SetBool("shoot", shoot);
        _animator.SetInteger("hp", life);

        // Must be NPC movements
        moveAmount = Vector3.SmoothDamp(moveAmount,targetMoveAmount,ref smoothMoveVelocity,.15f);
		transform.Rotate(Vector3.up * behaviorRet.rotation * rotateSpeed);

	}

	void FixedUpdate() {
		// Apply movement to rigidbody
		Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
		rigidbody.MovePosition(rigidbody.position + localMove);
        GetVision();
        foreach(GameObject obj in visible)
        {
            if (obj == GameObject.FindGameObjectWithTag("Player") && shootTimer <= 0)
            {
                shoot = true;
                run = false;
                walk = false;
                shootPlayer();
                shootTimer = 1f;
                return;
            }
        }
        shoot = false;
        run = true;
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
                    if(hit.collider.gameObject == obj)
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
                if(hit2.collider.gameObject == GameObject.FindGameObjectWithTag("Player"))
                    visible.Add(GameObject.FindGameObjectWithTag("Player"));
            }
        }
	}

    float steeringRotation()
    {
        Vector3 dir = _target - transform.position;
        Vector3 fwd = transform.forward;
        float angle = Mathf.Atan2(fwd.z * dir.x - dir.z * fwd.x, fwd.x * dir.x + fwd.z * dir.z) * Mathf.Rad2Deg;
        if (float.IsNaN(angle))
            angle = 0f;
        Vector3 desired = new Vector3(0, 5 * Mathf.Min(Mathf.Abs(angle), rotationSpeed) * angle / Mathf.Abs(angle), 0);
        Vector3 steering = desired - _rotation;
        _rotation += steering;
        return (angle);
    }

    void steeringForward()
    {
        Vector3 dir = _target - transform.position;
        dir = Vector3.Normalize(dir);
        Vector3 acc = maxAcc * dir;
        Vector3 steer_vel = rigidbody.velocity + acc * t;
        float speed = Mathf.Sqrt(steer_vel.x * steer_vel.x + steer_vel.z * steer_vel.z);
        if (speed > maxVel)
            steer_vel = Vector3.Normalize(steer_vel) * maxVel;
        _vel = steer_vel;
    }

    void steeringSeek()
    {
        Vector3 dir = _target - transform.position;
        dir = Vector3.Normalize(dir);
        Vector3 acc = maxAcc * dir;
        Vector3 steer_vel = rigidbody.velocity + acc * t;
        float speed = Mathf.Sqrt(steer_vel.x * steer_vel.x + steer_vel.z * steer_vel.z);
        if (speed > maxVel)
            steer_vel = Vector3.Normalize(steer_vel) * maxVel;
        _vel = steer_vel;
    }

    public void setTarget(Vector3 target)
    {
        _target = target;
    }

	public void takeDamage()
	{
		life -= 1;
        _animator.SetTrigger("hit");
	}

	void shootPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if(player != null)
        {
            RaycastHit hit;

            Vector3 eyePos = transform.position;
            eyePos.y += 1.5f;

            Vector3 playerPos = player.transform.position;
            playerPos.y += 1.5f;

            Vector3 rayDirection2 = playerPos - eyePos;

            if ((Vector3.Angle(rayDirection2, transform.forward)) <= fieldOfViewDegrees * 0.5f)
            {
                // Detect if object is within the field of view
                if (Physics.Raycast(eyePos, rayDirection2, out hit, viewDistance))
                {
                    if (hit.collider.gameObject == player)
                    {
                        player.GetComponent<Player>().takeDamage();
                        GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioSource>().clip, 1.0f);
                    }
                }
            }
        }
    }
}
