using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof (CharacterController))]
	[RequireComponent(typeof (AudioSource))]
	[RequireComponent(typeof(Animator))]
	public class FirstPersonController : MonoBehaviour
	{
		[SerializeField] private bool m_IsWalking;
		[SerializeField] private float m_WalkSpeed;
		[SerializeField] private float m_RunSpeed;
		[SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
		[SerializeField] private float m_JumpSpeed;
		[SerializeField] private float m_StickToGroundForce;
		[SerializeField] private float m_GravityMultiplier;
		[SerializeField] private MouseLook m_MouseLook;
		[SerializeField] private bool m_UseFovKick;
		[SerializeField] private FOVKick m_FovKick = new FOVKick();
		[SerializeField] private bool m_UseHeadBob;
		[SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
		[SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
		[SerializeField] private float m_StepInterval;
		[SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
		[SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
		[SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.
		[SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
		[SerializeField] float m_AnimSpeedMultiplier = 1f;
		[SerializeField] float m_MovingTurnSpeed = 360;
		[SerializeField] float m_StationaryTurnSpeed = 180;

		private float staminaReduction = 20;
		private PlayerHealth playerHealth;
		const float k_Half = 0.5f;
		private float m_TurnAmount;
		private Rigidbody m_Rigidbody;
		private Animator m_Animator;
		private Camera m_Camera;
		private bool m_Jump;
		private float m_YRotation;
		private Vector2 m_Input;
		private Vector3 m_MoveDir = Vector3.zero;
		private CharacterController m_CharacterController;
		private CollisionFlags m_CollisionFlags;
		private bool m_PreviouslyGrounded;
		private Vector3 m_OriginalCameraPosition;
		private float m_StepCycle;
		private float m_NextStep;
		private bool m_Jumping;
		private AudioSource m_AudioSource;
		private ChatScript chatscript;

        public PhotonView name;

		// Use this for initialization
		private void Start()
		{
			chatscript = GameObject.FindGameObjectWithTag ("ChatBox").GetComponent<ChatScript> ();
			m_Animator = transform.GetComponentInChildren<Animator> ();
			m_Rigidbody = transform.GetComponent<Rigidbody> ();
			m_CharacterController = GetComponent<CharacterController>();
			m_Camera = GetComponentInChildren<Camera> ();
			m_OriginalCameraPosition = m_Camera.transform.localPosition;
			m_FovKick.Setup(m_Camera);
			m_HeadBob.Setup(m_Camera, m_StepInterval);
			m_StepCycle = 0f;
			m_NextStep = m_StepCycle/2f;
			m_Jumping = false;
			m_AudioSource = GetComponent<AudioSource>();
			m_MouseLook.Init(transform , m_Camera.transform);
			playerHealth = GetComponent<PlayerHealth>();
            name.RPC("updateName", PhotonTargets.AllBuffered, PhotonNetwork.playerName);
		}
		
		
		// Update is called once per frame
		private void Update()
		{
			RotateView();

			if (!chatscript.isActive) {
			// the jump state needs to read here to make sure it is not missed
				if (!m_Jump)
				{
					m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
				}
			}
			
			if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
			{
				StartCoroutine(m_JumpBob.DoBobCycle());
				PlayLandingSound();
				m_MoveDir.y = 0f;
				m_Jumping = false;
			}
			if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
			{
				m_MoveDir.y = 0f;
			}



			m_PreviouslyGrounded = m_CharacterController.isGrounded;
		}
		
		
		private void PlayLandingSound()
		{
			m_AudioSource.clip = m_LandSound;
			m_AudioSource.Play();
			m_NextStep = m_StepCycle + .5f;
		}
		
		//updates the parameters for the animation
		private void AnimationUpdate()
		{
			// m_input has been updated by GetInput to the current input state.
			// update the animator parameters

			m_Animator.SetFloat("Forward", m_Input.y, 0.1f, Time.deltaTime);
			m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
			
			
			m_Animator.SetBool ("OnGround", !m_Jumping);
			
			
			if (m_Jumping) {
                m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
			}
			// calculate which leg is behind, so as to leave that leg trailing in the jump animation
			// (This code is reliant on the specific run cycle offset in our animations,
			// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
			float runCycle =
				Mathf.Repeat(
					m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
			float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_Input.y;
			if (!m_Jumping)
			{
				m_Animator.SetFloat("JumpLeg", jumpLeg);
			}
		}
		/*
		void UpdateAnimator(Vector3 move)
		{
			// update the animator parameters
			m_Animator.SetFloat("Forward", m_Input.y, 0.1f, Time.deltaTime);
			m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
			
			
			m_Animator.SetBool("OnGround", !m_Jumping);
			if (!m_CharacterController.isGrounded)
			{
				m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
			}
			
			// calculate which leg is behind, so as to leave that leg trailing in the jump animation
			// (This code is reliant on the specific run cycle offset in our animations,
			// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
			float runCycle =
				Mathf.Repeat(
					m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
			float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_Input.y;
			if (m_CharacterController.isGrounded)
			{
				m_Animator.SetFloat("JumpLeg", jumpLeg);
			}
			
			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
			// which affects the movement speed because of the root motion.
			if (m_CharacterController.isGrounded && move.magnitude > 0)
			{
				m_Animator.speed = m_AnimSpeedMultiplier;
			}
			else
			{
				// don't use that while airborne
				m_Animator.speed = 1;
			}
		}
		*/
		
		
		private void FixedUpdate()
		{
			float speed;
			GetInput(out speed);
			
			// update animation
			AnimationUpdate ();
			
			// always move along the camera forward as it is the direction that it being aimed at
			Vector3 desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;
			m_TurnAmount = Mathf.Atan2(m_Input.x, m_Input.y);
			
			//UpdateAnimator (desiredMove);
			
			// get a normal for the surface that is being touched to move along it
			RaycastHit hitInfo;
			Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
			                   m_CharacterController.height/2f);
			desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
			
			m_MoveDir.x = desiredMove.x*speed;
			m_MoveDir.z = desiredMove.z*speed;
			
			
			if (m_CharacterController.isGrounded)
			{
				m_MoveDir.y = -m_StickToGroundForce;
				
				if (m_Jump)
				{
					m_MoveDir.y = m_JumpSpeed;
					PlayJumpSound();
					m_Jump = false;
					m_Jumping = true;
				}
			}
			else
			{
				m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
			}
			m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);
			
			ProgressStepCycle(speed);
			UpdateCameraPosition(speed);
		}
		
		
		private void PlayJumpSound()
		{
			m_AudioSource.clip = m_JumpSound;
			m_AudioSource.Play();
		}
		
		
		private void ProgressStepCycle(float speed)
		{
			if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
			{
				m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 3.5f : m_RunstepLenghten * 3.0f)))*
					Time.fixedDeltaTime;
			}
			
			if (!(m_StepCycle > m_NextStep))
			{
				return;
			}
			
			m_NextStep = m_StepCycle + m_StepInterval;
			
			PlayFootStepAudio();
		}
		
		
		private void PlayFootStepAudio()
		{
			if (!m_CharacterController.isGrounded)
			{
				return;
			}
			// pick & play a random footstep sound from the array,
			// excluding sound at index 0
			int n = Random.Range(1, m_FootstepSounds.Length);
			m_AudioSource.clip = m_FootstepSounds[n];
			m_AudioSource.PlayOneShot(m_AudioSource.clip);
			// move picked sound to index 0 so it's not picked next time
			m_FootstepSounds[n] = m_FootstepSounds[0];
			m_FootstepSounds[0] = m_AudioSource.clip;
		}
		
		
		private void UpdateCameraPosition(float speed)
		{
			Vector3 newCameraPosition;
			if (!m_UseHeadBob)
			{
				return;
			}
			if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
			{
				m_Camera.transform.localPosition =
					m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
					                    (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
				newCameraPosition = m_Camera.transform.localPosition;
				newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
			}
			else
			{
				newCameraPosition = m_Camera.transform.localPosition;
				newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
			}
			m_Camera.transform.localPosition = newCameraPosition;
		}
		
		
		private void GetInput(out float speed)
		{

			// Read input
			if (!chatscript.isActive) {
				float horizontal = CrossPlatformInputManager.GetAxis ("Horizontal");
				float vertical = CrossPlatformInputManager.GetAxis ("Vertical");
			
				bool waswalking = m_IsWalking;

				// walking or running
				if (Input.GetKey (KeyCode.LeftShift)) {
					Debug.Log ("Left");
					// have stamina
					// can run
					playerHealth.ReduceStamina (staminaReduction * Time.deltaTime);
					m_IsWalking = !playerHealth.HasStamina ();
				
				} else {
					m_IsWalking = true;
				}

				#if !MOBILE_INPUT
				// On standalone builds, walk/run speed is modified by a key press.
				// keep track of whether or not the character is walking or running
				//m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
				#endif
				// set the desired speed to be walking or running
				speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
				m_Input = new Vector2 (horizontal, vertical);
			
				// normalize input if it exceeds 1 in combined length:
				if (m_Input.sqrMagnitude > 1) {
					m_Input.Normalize ();
				}
			
				// handle speed change to give an fov kick
				// only if the player is going to a run, is running and the fovkick is to be used
				if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0) {
					StopAllCoroutines ();
					StartCoroutine (!m_IsWalking ? m_FovKick.FOVKickUp () : m_FovKick.FOVKickDown ());
				}
			} else {
				speed = 0;
			}
		}
		
		
		private void RotateView()
		{
			m_MouseLook.LookRotation (transform, m_Camera.transform);
		}
		
		
		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			Rigidbody body = hit.collider.attachedRigidbody;
			//dont move the rigidbody if the character is on top of it
			if (m_CollisionFlags == CollisionFlags.Below)
			{
				return;
			}
			
			if (body == null || body.isKinematic)
			{
				return;
			}
			body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
		}
	}
}
