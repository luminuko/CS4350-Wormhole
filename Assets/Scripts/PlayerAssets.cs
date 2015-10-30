﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerAssets : MonoBehaviour {

	public int startingCash = 0;
	public int currentCash;
	public int numOfTorchesLeft = 10;
    public int numOfPotions;
	public Text cashAmountDisplay;
	public GameObject torchInstance;
	public GameObject teleportInstance;
	public GameObject teleportA;
	CustomTeleporter teleportAScript;
	CustomTeleporter teleportBaseScript;
	public AudioClip cashClip;
	public AudioClip errorClip;
	AudioSource playerAudio;
	public GameObject BaseTeleport;

    public Text feedback;
    GameObject player;
    PlayerHealth playerHealth;

	// Use this for initialization
	void Awake () {
		currentCash = startingCash;
        numOfPotions = 0;
		playerAudio = GetComponent <AudioSource> ();
        GameObject feedbackObject = GameObject.FindGameObjectWithTag("Feedback");
        feedback = feedbackObject.GetComponent<Text>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<PlayerHealth>();

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.F)) {
			tryToPlaceTorch();
		}
		if (Input.GetKeyDown(KeyCode.E)){
			placeTeleport();
		}
        if (Input.GetKeyDown(KeyCode.R))
        {
            UsePotion(numOfPotions);
        }
	}

	public void AddCash (int amount)
	{
		
		currentCash += amount;

		cashAmountDisplay.text = currentCash.ToString ();
		playerAudio.clip = cashClip;
		playerAudio.Play ();
		
		
	}

	public void tryToPlaceTorch(){
		Vector3 torchPosition = transform.position + transform.forward * 2.0f;
		if (numOfTorchesLeft > 0 && GetClosestTorchDistance(torchPosition) > 3.0f) {
			Instantiate(torchInstance, torchPosition, Quaternion.identity);
			numOfTorchesLeft--;
		}
		else {
			// NEED TO SAVE THE CLIP THAT WAS IN playerAudio before that anot? :(
			//AudioClip temp = playerAudio.clip;
			playerAudio.clip = errorClip;
			playerAudio.Play();
		}
	}

	public void placeTeleport(){
		Vector3 teleportPosition = transform.position + transform.forward * 2.0f;
		
		if (teleportA != null) {
			Destroy(teleportA);
		}
		teleportA = (GameObject)Instantiate (teleportInstance, teleportPosition, Quaternion.identity);
		teleportAScript = teleportA.GetComponentInChildren<CustomTeleporter>();
		teleportBaseScript = BaseTeleport.GetComponentInChildren<CustomTeleporter>();
		teleportAScript.destinationPad.SetValue (teleportBaseScript.gameObject.transform, 0);
		teleportAScript.teleportPadOn = true;
		teleportBaseScript.destinationPad.SetValue (teleportAScript.gameObject.transform, 0);
		teleportBaseScript.teleportPadOn = true;
		
		
	}
	private float GetClosestTorchDistance(Vector3 torchPos)
	{
		GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("Torch");
		GameObject closestObject = null;
		float closestDistance = float.MaxValue;
		foreach (GameObject obj in objectsWithTag)
		{
			if(!closestObject)
			{
				closestObject = obj;
				closestDistance = Vector3.Distance(torchPos, obj.transform.position);
			}
			//compares distances
			if(Vector3.Distance(torchPos, obj.transform.position) <= Vector3.Distance(torchPos, closestObject.transform.position))
			{
				closestObject = obj;
				closestDistance = Vector3.Distance(torchPos, obj.transform.position);
			}
		}
		return closestDistance;
	}

    public void UsePotion(int noOfPotion) {
        if (noOfPotion > 0) {
            player.GetComponent<PlayerHealth>().currentHealth += 20;
            feedback.color = new Color(1, 1, 1, 2);
            feedback.text = "You have healed " + "20" + " health.";
            noOfPotion -= 1;
        }
    }


		/*else{
			Destroy (teleportA);
			teleportA = (GameObject)Instantiate (teleportInstance, teleportPosition, Quaternion.identity);
			teleportAScript = teleportA.GetComponentInChildren<CustomTeleporter>();
			teleportBScript = teleportB.GetComponentInChildren<CustomTeleporter>();
			teleportAScript.destinationPad.SetValue(teleportBScript.gameObject.transform, 0); 
			teleportAScript.teleportPadOn = true;
			teleportBScript.destinationPad.SetValue(teleportAScript.gameObject.transform, 0); 
			teleportBScript.teleportPadOn = true;
		} */

	


}
