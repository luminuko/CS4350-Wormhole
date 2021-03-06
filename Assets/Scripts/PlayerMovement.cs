﻿using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float moveSpeed = 6f;
	public float rotateSpeed = 2f;
	
	Vector3 movement;
	// Animator anim;
	Rigidbody playerRigidbody;
	int floorMask;
	float camRayLength = 100f;
	
	void Awake()
	{
		floorMask = LayerMask.GetMask ("Floor");
		// anim = GetComponent<Animator> ();
		playerRigidbody = GetComponent<Rigidbody> ();
	}
	
	void FixedUpdate()
	{
		float h = Input.GetAxisRaw ("Horizontal");
		float v = Input.GetAxisRaw ("Vertical");
		Move (h, v);
		Turning ();
		// Animating (h, v);
	}
	
	void Move(float h, float v)
	{
		movement.Set (h, 0f, v);
		movement = movement.normalized * moveSpeed * Time.deltaTime;
		playerRigidbody.MovePosition (transform.position + movement);
	}

	public void useGravity()
	{
		playerRigidbody.useGravity = true;
	}
	

	
	void Turning()
	{
		if (Input.GetKey (KeyCode.Q)) {
			Quaternion deltaRotation = Quaternion.AngleAxis(-rotateSpeed, Vector3.up);
			playerRigidbody.MoveRotation (deltaRotation * transform.rotation);
		}
		if (Input.GetKey (KeyCode.E)) {
			Quaternion deltaRotation = Quaternion.AngleAxis(rotateSpeed, Vector3.up);
			playerRigidbody.MoveRotation (deltaRotation * transform.rotation);
		}

	}

	/*
	void Animating(float h, float v)
	{
		bool walking = h != 0f || v != 0f;
		anim.SetBool ("IsWalking", walking);
		
	}
	*/
	
}
