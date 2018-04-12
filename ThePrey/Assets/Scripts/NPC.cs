using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour {

    public enum BehaviourType
    {
        Formation,
        Wander,
        Bush,
        Track,
        Attack,
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
    public float shootDistance = 10;
    public float shootTimer = 0f;
    public GameObject pathfinder;
    private PathFinding _pathfinder;
    private Queue<Vector3> bushes = new Queue<Vector3>();
    private float bushTimer = 5;

    // System vars
    Vector3 moveAmount;
	Vector3 smoothMoveVelocity;
	Rigidbody rgbd;
    Animator _animator;

    //Animator vars
    bool walk;
    bool run;
    bool shoot;
    bool attack;
    bool cover;

    float avoidTargetRadius = 10;
    Vector3 _avoidTarget;
    Vector3 _target;
    Vector3 _rotation;
    Vector3 _vel;

    public GameObject leader;
    public Vector3 offset;

    float t = 0.4f;
    float maxAcc = 35f;
    float maxVel = 5;
    float rotationSpeed = 35f;

    int wanderDir = 10;

    behaviour behaviourRet;
    Vector3 playerLastPos;
    bool playerSeen;
    float playerSeenTimer = 0;

    MeshRenderer ground;

	// Ressources
	public int life = 10;
	public float maxStamina = 2;
    private float stamina;
    private float wanderTimer = 0f;

    NPCHandler handler;

    private List<GameObject> visible = new List<GameObject>();

    void Awake() {
		rgbd = GetComponent<Rigidbody> ();
        _animator = GetComponent<Animator>();
        behaviourRet = Behaviour();
        playerSeen = false;
        ground = GameObject.Find("Plane").GetComponent<MeshRenderer>();
		run = false;
        attack = false;
        stamina = maxStamina;
        handler = GameObject.FindGameObjectWithTag("GameController").GetComponent<NPCHandler>();
        _pathfinder = pathfinder.GetComponent<PathFinding>();
        _target = gameObject.transform.position;
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

    void updateBehaviour()
    {
        if (behaviourRet.type == BehaviourType.Formation && handler.GetGeneralBehaviour() != BehaviourType.Wander)
            _target = leader.transform.TransformPoint(offset);
        if (behaviourRet.type != BehaviourType.Attack)
        {
            steeringSeek();
            steeringRotation();
        }
        if (shootTimer > 0)
            shootTimer -= Time.deltaTime;
        switch (behaviourRet.type)
        {
            case BehaviourType.Formation:
                if (handler.GetGeneralBehaviour() != BehaviourType.Wander)
                {
                    run = true;
                    walk = false;
                    //shoot = false;
                    break;
                }
                if (gameObject != leader &&
                    (gameObject.transform.position.z > leader.transform.position.z && wanderDir > 0
                    || gameObject.transform.position.z < leader.transform.position.z && wanderDir < 0))
                    _vel /= 4;
                else if (gameObject != leader &&
                    (gameObject.transform.position.z < leader.transform.position.z && wanderDir > 0
                    || gameObject.transform.position.z > leader.transform.position.z && wanderDir < 0))
                    _vel /= 2;
                else
                    _vel /= 3;
                run = false;
                walk = true;
                upStamina();
                break;
            case BehaviourType.Wander:
                _vel /= 4;
                run = false;
                walk = true;
                upStamina();
                break;
            case BehaviourType.Track: case BehaviourType.Bush:
                if (stamina == maxStamina || run)
                {
                    stamina -= Time.deltaTime;
                    run = true;
                    if (stamina == 0)
                        run = false;
                    _vel *= 1.75f;
                }
                break;
            case BehaviourType.Attack:
                run = false;
                shoot = true;
                shootPlayer();
                upStamina();
                break;
            case BehaviourType.Flee:
                _vel *= -1;
                walk = true;
                shoot  =false;
                upStamina();
                break;
            default:
                run = false;
                walk = true;
                upStamina();
                break;
        }
    }

    void FixedUpdate() {
        // Calculate movement
        shootTimer -= Time.deltaTime;
        behaviourRet = ChooseBehaviour(behaviourRet);
        handler.SetNPCBehavior(gameObject, behaviourRet.type);
        if (Vector3.Distance(gameObject.transform.position, _target) < avoidTargetRadius)
            _target = _pathfinder.getPosition(gameObject.transform.position, behaviourRet.target);
        updateBehaviour();
        if (bushes.Count > 0)
        {
            if (bushTimer <= 0)
            {
                bushes.Dequeue();
                bushTimer = 5;
            }
            else
                bushTimer -= Time.deltaTime;
        }
        if (playerSeen)
        {
            playerSeenTimer += Time.deltaTime;
            if (playerSeenTimer >= 10)
            {
                playerSeenTimer = 0;
                playerSeen = false;
            }
        }
        _animator.SetBool("walk", walk);
        _animator.SetBool("run", run);
        _animator.SetBool("shoot", shoot);
        _animator.SetInteger("hp", life);
        if (!float.IsNaN(_rotation.y) && !float.IsInfinity(_rotation.y))
        {
            transform.eulerAngles += _rotation * Time.deltaTime;
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
        if (behaviourRet.type != BehaviourType.Attack)
            transform.position = CheckBorder(transform.position + _vel * Time.deltaTime);
        if (behaviourRet.type == BehaviourType.Bush && gameObject.transform.position == behaviourRet.target)
            bushes.Enqueue(behaviourRet.target);
    }

    behaviour ChooseBehaviour(behaviour b)
    {
        GetVision();
        int priority = 0;
        int formationPrio = (int)handler.GetGeneralBehaviour();
        foreach (GameObject obj in visible)
        {
            switch (obj.name)
            {
                case "Player":
                    if (Vector3.Distance(obj.gameObject.transform.position, gameObject.transform.position) > shootDistance)
                        b.type = BehaviourType.Track;
                    else if (obj.GetComponent<Player>().life > 5 && Vector3.Distance(obj.gameObject.transform.position, gameObject.transform.position) < 5)
                        b.type = BehaviourType.Flee;
                    else
                        b.type = BehaviourType.Attack;
                    priority = 3;
                    b.target = obj.gameObject.transform.position;
                    playerLastPos = b.target;
                    playerSeen = true;
                    return b;
                case "bush":
                    if (formationPrio >= 2 || priority >= 2 || b.target == obj.transform.position || bushes.Contains(obj.gameObject.transform.position))
                        break;
                    b.type = BehaviourType.Bush;
                    b.target = obj.gameObject.transform.position;
                    priority = 1;
                    break; 
                case "footprint":
                    if (priority >= 3 || formationPrio >= 3)
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
        if (priority < 3 && playerSeen)
        {
            b.target = playerLastPos;
            b.type = BehaviourType.Track;
        }
        else if ((b.type != BehaviourType.Formation || handler.GetGeneralBehaviour() == BehaviourType.Wander) && priority == 0 && wanderTimer <= 0)
        {
            wanderTimer = 1;
            b.type = BehaviourType.Wander;
            b.target = gameObject.transform.position;
            b.target.z += wanderDir;
            b.target.x += Random.Range(-15, 15);
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
            float dist = Mathf.Sqrt(Mathf.Pow(transform.position.x - obj.transform.position.x, 2) + Mathf.Pow(transform.position.z - obj.transform.position.z, 2));
            Vector3 rayDirection = obj.transform.position - eyePos;

            if ((Vector3.Angle(rayDirection, transform.forward)) <= fieldOfViewDegrees * 0.5f || dist < 3)
            {
                // Detect if object is within the field of view
                if (Physics.Raycast(eyePos, rayDirection, out hit, viewDistance) || dist < 3)
                {
                    if(hit.collider.gameObject == obj || dist < 3)
                        visible.Add(obj);
                }
            }
        }
        RaycastHit hit2;
        Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
        playerPos.y += 1.5f;
        float dist2 = Mathf.Sqrt(Mathf.Pow(transform.position.x - playerPos.x, 2) + Mathf.Pow(transform.position.z - playerPos.z, 2));
        Vector3 rayDirection2 = playerPos - eyePos;

        if ((Vector3.Angle(rayDirection2, transform.forward)) <= fieldOfViewDegrees * 0.5f || dist2 < 3)
        {
            // Detect if object is within the field of view
            if (Physics.Raycast(eyePos, rayDirection2, out hit2, viewDistance) || dist2 < 3)
            {
                if(hit2.collider.gameObject == GameObject.FindGameObjectWithTag("Player") || dist2 < 3)
                    visible.Add(GameObject.FindGameObjectWithTag("Player"));
            }
        }
	}

    float steeringRotation()
    {
        Vector3 dir = _target - transform.position;
        Vector3 fwd = transform.forward;
        float angle = Mathf.Atan2(fwd.z * dir.x - dir.z * fwd.x, fwd.x * dir.x + fwd.z * dir.z) * Mathf.Rad2Deg;
        if (float.IsNaN(angle) || float.IsInfinity(angle))
        {
            _rotation = new Vector3(0, 0, 0);
            return 0;
        }
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
        Vector3 steer_vel = rgbd.velocity + acc * t;
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
        if (pos.z < ground.bounds.min.z + 0.5)
        {
            pos.z = ground.bounds.min.z + 0.7f;
            wanderDir = 10;
        }
        else if (pos.z > ground.bounds.max.z - 0.5)
        {
            pos.z = ground.bounds.max.z - 0.7f;
            wanderDir = -10;
        }
        if (pos.x > ground.bounds.max.x)
            pos.x = ground.bounds.max.x;
        else if (pos.x < ground.bounds.min.x)
            pos.x = ground.bounds.min.x;
        return pos;
    }

	public void takeDamage()
	{
		life -= 1;
        _animator.SetTrigger("hit");
	}

    public void setFormation(GameObject newLeader, Vector3 newOffset)
    {
        //Debug.Log("setFormation new Leader: " + newLeader);
        leader = newLeader;
        if (leader != gameObject)
        {
            behaviourRet.type = BehaviourType.Formation;
            //print(name + " is in formation behaviour");
        }
        else
            behaviourRet.type = BehaviourType.Wander;
        offset = newOffset;
    }

    void shootPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (shootTimer > 0)
            return;
        shootTimer = 2;
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
