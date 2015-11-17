﻿using UnityEngine;
using System.Collections;

public class ZombieRandomMovement : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (PhotonNetwork.isMasterClient) {
			transform.Translate (Vector3.forward * (Time.deltaTime * 0.3f));
			transform.Rotate (0, 2 * Time.deltaTime, 0);
		}
	}
}
