using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour {

    public enum BehaviourType {
        Wander,
        Attack,
        Formation,
        Track,
        Bush,
        Flee
    };

    struct behaviour {
        public Vector3 target;
        public Quaternion rotation;
        public BehaviourType type;
        public bool done;

        public behaviour(Vector3 m, Quaternion r, BehaviourType t) {
            target = m;
            rotation = r;
            type = t;
            done = false;
        }
    }

    // public vars
    public float walkSpeed = 6;
	public float rotateSpeed = 3;
    public float fieldOfViewDegrees = 130;
    public float viewDistance = 10;
    public float shootTimer = 0f;

    private Queue<Vector3> bushes = new Queue<Vector3>();

    // System vars
    Vector3 moveAmount;
	Vector3 smoothMoveVelocity;
	Rigidbody rigidbody;
    Animator _animator;

    //Animator vars
    bool walk;
    bool run;
    bool shoot;
//    bool flee;
    bool attack;
    bool cover;

    Vector3 _target;
    Vector3 _rotation;
    Vector3 _vel;

    float t = 0.4f;
    float maxAcc = 30f;
    float maxVel = 5;
    float rotationSpeed = 35f;

    int wanderDir = 10;

    behaviour behaviourRet;
    Vector3 playerLastPos;

    MeshRenderer ground;

	// Ressources
	public int life = 10;
	public float maxStamina = 2;
    private float stamina;
    private float wanderTimer = 0f;

    private List<GameObject> visible = new List<GameObject>();

    void Awake() {
		rigidbody = GetComponent<Rigidbody> ();
        _animator = GetComponent<Animator>();
        behaviourRet = Behaviour();
        ground = GameObject.Find("Plane").GetComponent<MeshRenderer>();
        //sneak = false;
		run = false;
        attack = false;
        stamina = maxStamina;
	}

    behaviour Behaviour()
    {
        return (new behaviour(new Vector3(0, 0, 0), Quaternion.identity, 0));
    }


    void upStamina()
    {
        stamina += Time.deltaTime;
        if (stamina >= maxStamina)
            stamina = maxStamina;
    }

    void FixedUpdate() {
        // Calculate movement
        shootTimer -= Time.deltaTime;
        behaviourRet = ChooseBehaviour(behaviourRet);
        _target = behaviourRet.target;
        print(behaviourRet.type);

        if (behaviourRet.type != BehaviourType.Attack)
            steeringSeek();
        if (shootTimer > 0)
            shootTimer -= Time.deltaTime;
        if (behaviourRet.type == BehaviourType.Wander) {
            _vel /= 4;
            run = false;
            walk = true;
            upStamina();
        } else if ((behaviourRet.type == BehaviourType.Track || behaviourRet.type == BehaviourType.Bush) && (stamina == maxStamina || run)) {
            stamina -= Time.deltaTime;
            run = true;
            if (stamina == 0)
                run = false;
            _vel *= 1.75f;
        } else if (behaviourRet.type == BehaviourType.Attack) {
            run = false;
            shoot = true;
            print("attacking player");
            shootPlayer();
            upStamina();
        } else { /*walk or idle*/
            run = false;
            walk = true;
            upStamina();
        }

        _animator.SetBool("walk", walk);
        _animator.SetBool("run", run);
        _animator.SetBool("shoot", shoot);
        _animator.SetInteger("hp", life);
        steeringRotation();
        transform.eulerAngles += _rotation * Time.deltaTime;
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        if (behaviourRet.type != BehaviourType.Attack)
            transform.position = CheckBorder(transform.position + _vel * Time.deltaTime);
        if (behaviourRet.type == BehaviourType.Bush && gameObject.transform.position == behaviourRet.target)
            bushes.Enqueue(behaviourRet.target);

    }

    behaviour ChooseBehaviour(behaviour b)
    {
        GetVision();
        int priority = 0;
        foreach (GameObject obj in visible)
        {
            switch (obj.name)
            {
                case "Player":
                    print("SEEN PLAYER : life  = " + life);
                    if (life < 2 && !cover)
                        b.type = BehaviourType.Flee;
                    else
                        b.type = BehaviourType.Attack;
                    priority = 3;
                    b.target = obj.gameObject.transform.position;
                    playerLastPos = b.target;
                    return b;
                case "bush":
                    print("Checking nearby bush");
                    if (priority >= 2 || b.target == obj.transform.position || bushes.Contains(obj.gameObject.transform.position))
                        break;
                    b.type = BehaviourType.Bush;
                    b.target = obj.gameObject.transform.position;
                    priority = 1;
                    break; 
                case "footprint":
                    print("tracking footprints");
                    if (priority >= 3)
                        break;
                    b.type = BehaviourType.Track;
                    if (priority == 2 && 
                        Vector3.Distance(b.target, gameObject.transform.position) > Vector3.Distance(b.target, obj.gameObject.transform.position))
                            break;
                    priority = 2;
                    b.target = obj.gameObject.transform.position;
                    b.rotation = obj.gameObject.transform.localRotation;
                    break;
                default:
                    break;
            }
        }
        if (priority == 0 && wanderTimer <= 0)
        {
            wanderTimer = 1;
            print("Looking for signs");
            b.type = BehaviourType.Wander;
            b.target = gameObject.transform.position;
            b.target.z += wanderDir;
            b.target.x += Random.Range(-15, 15);
            print(gameObject.transform.position);
            print(b.target);
        }
        else if (priority == 0)
            wanderTimer -= Time.deltaTime;
        shoot = false;
        walk = false;
        return b;
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

    Vector3 CheckBorder(Vector3 pos)
    {
        if (pos.x < ground.bounds.min.x)
        {
            pos.x = ground.bounds.min.x;
            wanderDir *= -1;
        }
        else if (pos.x > ground.bounds.max.x)
        {
            pos.x = ground.bounds.max.x;
            wanderDir *= -1;
        }
        if (pos.z > ground.bounds.max.z)
            pos.z = ground.bounds.max.z;
        else if (pos.z < ground.bounds.min.z)
            pos.z = ground.bounds.min.z;
        return pos;
    }

	public void takeDamage()
	{
		life -= 1;
        _animator.SetTrigger("hit");
	}

	void shootPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (shootTimer > 0)
            return;
        shootTimer = 1;
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
