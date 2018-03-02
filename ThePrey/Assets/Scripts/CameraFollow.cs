using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
	[SerializeField]
	GameObject target;
	[SerializeField]
	float distToPlayer;
	[SerializeField]
	float rotation;
	[SerializeField]
	float rotSpeed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		distToPlayer += Input.GetAxis ("Mouse ScrollWheel");
        target.transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * target.GetComponent<Player>().rotateSpeed);
        if (distToPlayer < 1.5f)
			distToPlayer = 1.5f;
		else if (distToPlayer > 10)
			distToPlayer = 10;
		rotation += Input.GetAxis ("Mouse Y") / rotSpeed;
		if (rotation <= 0)
			rotation = 0.001f;
		else if (rotation >= 1)
			rotation = 0.999f;
		Vector3 offset = target.transform.forward * -1 * rotation + target.transform.up * (1 - rotation);
		offset.Normalize ();
		transform.position = target.transform.position + offset * distToPlayer;
		transform.LookAt (target.transform);

    }
}
