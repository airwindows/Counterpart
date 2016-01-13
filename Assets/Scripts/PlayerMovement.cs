using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// This handles movement input- the directional keys, jumping,
/// running, and mouse-look.
/// </summary>
[RequireComponent(typeof(BackgroundSound))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
	public GameObject ourlevel;
	public Camera mainCamera;
	public Camera wireframeCamera;
	public Camera skyboxCamera;
	public Light headlight;
	public TextMesh fpsMesh;
	public TextMesh botsMesh;
	public Renderer sightA;
	public Renderer sightB;
	public ParticleSystem particlesystem;
	public AnimationCurve slopeCurveModifier = new AnimationCurve (new Keyframe (-90.0f, 1.0f), new Keyframe (0.0f, 1.0f), new Keyframe (90.0f, 0.0f));
	public int yourMatch;
	public Color32[] yourBrain;
	public float activityRange = 10f;
	private int brainPointer;
	private float altitude = 1f;
	private Rigidbody rigidBody;
	private SphereCollider sphereCollider;
	private AudioSource audiosource;
	public float yourMatchDistance = 9999;
	public bool yourMatchOccluded = true;
	private int pingTimer = 2;
	private BackgroundSound backgroundSound;
	public AudioClip botBeep;
	public float baseFOV = 68f;
	public float mouseSensitivity = 100f;
	public float mouseDrag = 0f;
	public float baseJump = 2.5f;
	public float initialTurn = 0f;
	public float initialUpDown = 0f;
	public float maximumBank = 1f;
	private Vector3 desiredMove = Vector3.zero;
	private bool releaseJump = true;
	private float moveLimit = 0f;
	private float deltaTime = 0f;
	private float xRot = 0f;
	private float yRot = 0f;
	private float yCorrect = 0f;
	private float zRot = 0f;
	public float clampRotateAngle = Mathf.PI * 2f;
	private float lookAngleUpDown = Mathf.PI * 0.2f;
	public Vector3 desiredAimOffsetPosition;
	public float fps = 60f;
	private float prevFps = 60f;
	public int botNumber = 500;
	private int prevBotNumber = 500;
	public int totalBotNumber = 500;
	public int chooseGuardian = 0;
	private int blurHack;
	private Quaternion blurHackQuaternion;
	private GameObject allbots;
	private RaycastHit hit;
	private float nearGround = 1f;
	private float chaseTilt = 0f;
	private float speedSmoothing = 0f;
	private float cameraZoom = 0f;
	public float timeBetweenGuardians = 1f;
	public float probableGuilt = 0f;
	public float creepToRange = 1800f;
	public float creepRotAngle = 1f;


	void Awake ()
	{
		backgroundSound = GetComponent<BackgroundSound> ();
		rigidBody = GetComponent<Rigidbody> ();
		sphereCollider = GetComponent<SphereCollider> ();
		audiosource = GetComponent<AudioSource> ();
		allbots = GameObject.FindGameObjectWithTag ("AllBots").gameObject;
		baseJump = 2.5f;
		blurHack = 0;
	}

	void Update ()
	{
		//Each frame we run Update, regardless of what game control/physics is doing. This is the fundamental 'tick' of the game but it's wildly time-variant: it goes as fast as possible.

		if (QualitySettings.maximumLODLevel == 0) {
			float tempMouse = Input.GetAxis ("MouseX") / mouseSensitivity;
			mouseDrag = Mathf.Abs (tempMouse);
			initialTurn = Mathf.Lerp(initialTurn, initialTurn - tempMouse, Mathf.Abs(Input.GetAxis ("MouseX")*0.1f));
			tempMouse = Input.GetAxis ("MouseY") / mouseSensitivity;
			if (Mathf.Abs (tempMouse) > mouseDrag) mouseDrag = Mathf.Abs (tempMouse);
			initialUpDown = Mathf.Lerp(initialUpDown, initialUpDown + tempMouse, Mathf.Abs(Input.GetAxis ("MouseY")*0.1f));
			tempMouse = -tempMouse;
			if (initialTurn < 0f)
				initialTurn += clampRotateAngle;
			if (initialTurn > clampRotateAngle)
				initialTurn -= clampRotateAngle;
			initialUpDown = Mathf.Clamp (initialUpDown, -1.5f, 1.5f);
			//mouse is instantaneous so it can be in Update.
		}

		if (SystemInfo.supportsRenderTextures) {
			blurHack += 1;
			if (blurHack > 1) blurHack = 0;
			blurHackQuaternion = wireframeCamera.transform.localRotation;
			if (blurHack == 0) blurHackQuaternion.y += 0.0002f;
			if (blurHack == 1) blurHackQuaternion.y -= 0.0002f;
		//	if (blurHack == 2) blurHackQuaternion.x -= 0.00015f;
		//	if (blurHack == 3) blurHackQuaternion.z -= 0.00015f;
			//can be a four position jitter, but we have these big horizontal lines so we want only side-to-side jitter.
			wireframeCamera.transform.localRotation = blurHackQuaternion;
		}

		yRot = Mathf.Sin (initialUpDown);
		yCorrect = Mathf.Cos (initialUpDown);
		xRot = Mathf.Cos (initialTurn) * yCorrect;
		zRot = Mathf.Sin (initialTurn) * yCorrect;
		//there's our angle math

		desiredAimOffsetPosition = transform.localPosition;
		desiredAimOffsetPosition += new Vector3 (xRot, yRot, zRot);
		mainCamera.transform.LookAt (desiredAimOffsetPosition);
		//We simply offset a point from where we are, using simple orbital math, and look at it
		//The positioning is simple and predictable, and LookAt is great at translating that into quaternions.
		if (QualitySettings.maximumLODLevel == 0)
			mainCamera.transform.rotation = mainCamera.transform.rotation * Quaternion.Euler (0, 0, chaseTilt*(nearGround));
		else
			mainCamera.transform.rotation = mainCamera.transform.rotation * Quaternion.Euler (0, 0, chaseTilt*(nearGround));
		//we apply tilt and camera shake: raw Mouse Y is decent camera shake and chaseTilt
		//QualitySettings.maximumLODlevel is being used to pass controller choice as they can't both run at once
		//I don't need Unity's LOD as mine is simpler and works without it and does lots more

		if ((Input.GetButton("Jump") || Input.GetButton("KeyboardJump")) && releaseJump) {
			if (Physics.Raycast (transform.position, Vector3.down, out hit)){
					rigidBody.AddForce (Vector3.up * baseJump / Mathf.Pow(hit.distance, 3), ForceMode.Impulse);
				releaseJump = false;
				//if you jump you can climb steeper walls, but not vertical ones
				//we can trigger the jump at any time in Update, but only once for each FixedUpdate
				//then we gotta wait for FixedUpdate to space it out again. This will work for any twitch control
				//also, we don't have to care which input system is driving it with this
			}
		}

	}
			
	void FixedUpdate ()
	{
		//FixedUpdate is run as many times as needed, before an Update step: or, it's skipped if framerate is super high.
		//For this reason, if framerate is known to be always higher than 50fps, stuff can be put here to help the engine run faster
		//but if framerate's running low, we are not actually getting a spaced out distribution of frames, only a staggering of them
		//to allow physics to run correctly.
		Vector2 input = Vector2.zero;
		if (QualitySettings.maximumLODLevel == 0) input = new Vector2 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"));
		//keyboard input
		if (QualitySettings.maximumLODLevel == 1) {
			float tempJoystick = Input.GetAxis ("JoystickLookLeftRight") / (Mathf.Abs (Input.GetAxis ("JoystickLookLeftRight")) + 8f);
			initialTurn -= tempJoystick;
			mouseDrag = Mathf.Abs (tempJoystick);
			//this is just equals, if we only have the joystick. We started it with mouse
			tempJoystick = Input.GetAxis ("JoystickLookUpDown") / (Mathf.Abs (Input.GetAxis ("JoystickLookUpDown")) + 8f);
			initialUpDown += tempJoystick;
			tempJoystick = -tempJoystick;
			if (Mathf.Abs (tempJoystick) > mouseDrag)
				mouseDrag = Mathf.Abs (tempJoystick);
			mouseDrag /= 4f;
			//compensating for differences in input systems: see the tilt code for another case of this change
			if (initialTurn < -clampRotateAngle)
				initialTurn += clampRotateAngle;
			if (initialTurn > clampRotateAngle)
				initialTurn -= clampRotateAngle;
			lookAngleUpDown = Mathf.Lerp (lookAngleUpDown, Mathf.Abs (Input.GetAxis ("JoystickLookUpDown")) + 0.5f, Mathf.Abs (Input.GetAxis ("JoystickLookUpDown") * 0.1f) + 0.2f);
			//this is clamping the max look angle so if you're physically pushing the stick it lets you look farther. release the stick and it goes partway back again
			initialUpDown = Mathf.Clamp (initialUpDown, -lookAngleUpDown, lookAngleUpDown);
			input = new Vector2 (Mathf.Pow (Input.GetAxis ("JoystickMoveForwardBack"), 5f), Mathf.Pow (Input.GetAxis ("JoystickStrafeLeftRight"), 5f));
		} //joystick is a lot more complicated, but necessary to make controller responsive

		Vector3 groundContactNormal;
		Vector3 rawMove = mainCamera.transform.forward * input.y + mainCamera.transform.right * input.x;
		float adjacentSolid = 99999;
		float downSolid = 99999;

		releaseJump = true;
		//it's FixedUpdate, so release the jump in Update again so it can be retriggered.

		particlesystem.transform.localPosition = Vector3.forward * 0.5f;
		if (Input.GetButton ("Talk") || Input.GetButton ("KeyboardTalk") || Input.GetButton ("MouseTalk")) {
			if (!particlesystem.isPlaying) particlesystem.Play ();
			particlesystem.emissionRate = 30.0f;
		} else particlesystem.emissionRate = 0.0f;
		//this too can be fired by either system with no problem

		if (Physics.SphereCast (transform.position, sphereCollider.radius, Vector3.down, out hit)) {
			groundContactNormal = hit.normal;
		} else {
			groundContactNormal = Vector3.up;
		}
		desiredMove = Vector3.ProjectOnPlane (rawMove, groundContactNormal).normalized;

		pingTimer += 1;
		if (pingTimer > yourMatchDistance) pingTimer = 0;

		if ((audiosource.clip != botBeep) && audiosource.isPlaying) {
			//we are playing the smashing sounds of the giant guardians
		} else {
			audiosource.pitch = 3f / ((pingTimer + 64f) / 65f);
			if (pingTimer == 0) {
				if (ourlevel.GetComponent<SetUpBots> ().gameEnded == false) {
					if (audiosource.clip != botBeep)
						audiosource.clip = botBeep;
					if (yourMatchOccluded) {
						audiosource.volume = 0.2f;
						audiosource.reverbZoneMix = 2f;
					} else {
						audiosource.volume = 0.2f;
						//we will keep it the same so it sounds the same: increasing volume
						//caused it to sound different inc. in the reverb
						float verbZone = 2f - (70f / yourMatchDistance);
						if (verbZone < 0f)
							verbZone = 0f;
						audiosource.reverbZoneMix = verbZone;
					}
					audiosource.Play ();
				}
			}
			//this is our geiger counter for our bot
		}


		StartCoroutine ("SlowUpdates");

		if (Physics.Raycast (transform.position, Vector3.down, out hit)) {
			altitude = hit.distance;
			if (adjacentSolid > altitude)
				adjacentSolid = altitude;
			if (downSolid > altitude)
				downSolid = altitude;
			//down's special, use it to test for climbing walls with jumps
		} else {
			if (Physics.Raycast (transform.position, Vector3.up, out hit)) {
				//we are doing a ricochet as best we can
				transform.position = hit.point + Vector3.up;
				rigidBody.velocity = Vector3.Reflect(rigidBody.velocity, hit.normal);
				altitude = 1;
			}
		}

		if (Physics.Raycast (transform.position, Vector3.left, out hit)) {
			if (adjacentSolid > hit.distance)
				adjacentSolid = hit.distance;
		}
		if (Physics.Raycast (transform.position, Vector3.right, out hit)) {
			if (adjacentSolid > hit.distance)
				adjacentSolid = hit.distance;
		}
		if (Physics.Raycast (transform.position, Vector3.forward, out hit)) {
			if (adjacentSolid > hit.distance)
				adjacentSolid = hit.distance;
		}
		if (Physics.Raycast (transform.position, Vector3.back, out hit)) {
			if (adjacentSolid > hit.distance)
				adjacentSolid = hit.distance;
		}
		//this gives us a quick nearest-surface check for all directions but up
		
		if (adjacentSolid < 1) {
			float bumpUp = transform.position.y + ((1 - adjacentSolid) / 32);
			transform.position = new Vector3 (transform.position.x, bumpUp, transform.position.z);
			//this keeps us off the ground
			adjacentSolid = 1;
		}
		//thus we can only maneuver if we are near a surface
		
		adjacentSolid *= adjacentSolid;
		adjacentSolid *= adjacentSolid;
		
		if (desiredMove.y > 0) {
			float angle = Vector3.Angle (groundContactNormal, Vector3.up);
			moveLimit = slopeCurveModifier.Evaluate (angle);
			//apply a slope based limiting factor
			
			desiredMove *= moveLimit;
			desiredMove.y *= moveLimit;
			//we can't climb but we can go down all we want
			//try to restrict vertical movement more than lateral movement
		}

		float momentum = Mathf.Sqrt(Vector3.Angle (mainCamera.transform.forward, rigidBody.velocity)+7f+mouseDrag) * 0.2f;
		//5 controls the top speed, 0.2 controls maximum clamp when turning
		if (momentum < 0.001f) momentum = 0.001f; //insanity check
		if (momentum > adjacentSolid) momentum = adjacentSolid; //insanity check
		if (adjacentSolid < 1f) adjacentSolid = 1f; //insanity check
		desiredMove /= (adjacentSolid + mouseDrag);
		//we're adding the move to the extent that we're near a surface
		rigidBody.drag = momentum / adjacentSolid; //1f + mouseDrag
		//alternately, we have high drag if we're near a surface and little in the air

		rigidBody.AddForce (desiredMove, ForceMode.Impulse);
		//apply the player move

		speedSmoothing = 1.5f / (rigidBody.velocity.magnitude + 4f);
		//this makes it settle down when moving real fast
		float tempSpeedRelatedBank = ((maximumBank/momentum)/Mathf.Sqrt(speedSmoothing)) / Mathf.Pow (altitude, 3f);
		//this complicated mess gives us bank angles that aren't directly related to how jittery things are
		
		if (nearGround > tempSpeedRelatedBank) nearGround = tempSpeedRelatedBank;
		else nearGround = Mathf.Lerp(nearGround, tempSpeedRelatedBank, speedSmoothing);
		//we lerp the nearGround parameter here, because it'll be more fluid if it's in FixedUpdate
		//we're also forcing it to be small if it's becoming small: if it's large that's when it's crazytown
		if (QualitySettings.maximumLODLevel == 0)
			chaseTilt = Mathf.Lerp (chaseTilt, -Input.GetAxis ("MouseX") / ((mouseSensitivity / fps) * 45f), speedSmoothing);
		//we are applying mouse input at full crank, so we have to compensate in order to get bank angles like we expect
		else
			chaseTilt = Mathf.Lerp (chaseTilt, -Input.GetAxis ("JoystickLookLeftRight") / 4f, speedSmoothing);
		//chaseTilt is de-jittering the bank effect. Depends what kind of control system
		//we apply it in Update, though.

	}



	IEnumerator SlowUpdates () {
		if (transform.position.x < 1) {
			transform.position = new Vector3 (1, transform.position.y, transform.position.z);
			rigidBody.velocity = new Vector3 (Mathf.Abs (rigidBody.velocity.x), rigidBody.velocity.y, rigidBody.velocity.z);
		}

		if (Cursor.lockState != CursorLockMode.Locked) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		//the notorious cursor code! Kills builds on Unity 5.2 and up

		timeBetweenGuardians *= 0.9997f;
		probableGuilt *= 0.9997f;
		//with this factor we scale how sensitive guardians are to bots bumping each other


		cameraZoom = Mathf.Sqrt (rigidBody.velocity.magnitude + 1f);
		headlight.spotAngle = (baseFOV*1.5f) + cameraZoom;
		//control the headlight to give the impression of tunnel vision at speed
		if (altitude < 1f) {
			backgroundSound.whooshLowCut = Mathf.Lerp (backgroundSound.whooshLowCut, 0.001f, 0.5f);
			backgroundSound.whoosh = (rigidBody.velocity.magnitude * Mathf.Sqrt (rigidBody.velocity.magnitude) * 0.000008f);
		} else {
			backgroundSound.whooshLowCut = Mathf.Lerp (backgroundSound.whooshLowCut, 0.2f, 0.5f);
			backgroundSound.whoosh = (rigidBody.velocity.magnitude * Mathf.Sqrt (rigidBody.velocity.magnitude) * 0.000005f);
		}
		yield return new WaitForSeconds(.016f);

		if (transform.position.z < 1) {
			transform.position = new Vector3 (transform.position.x, transform.position.y, 1);
			rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, Mathf.Abs (rigidBody.velocity.z));
		}
		mainCamera.fieldOfView = baseFOV + (cameraZoom*0.5f);
		backgroundSound.brightness = (transform.position.y / 900.0f) + 0.2f;
		yield return new WaitForSeconds(.016f);

		if (transform.position.x > 3999) {
			transform.position = new Vector3 (3999, transform.position.y, transform.position.z);
			rigidBody.velocity = new Vector3 (-Mathf.Abs (rigidBody.velocity.x), rigidBody.velocity.y, rigidBody.velocity.z);
		}
		wireframeCamera.fieldOfView = baseFOV - 0.5f + (cameraZoom*0.5f);
		float recip = 1.0f / backgroundSound.gain;
		recip = Mathf.Lerp ((float)recip, altitude, 0.5f);
		recip = Mathf.Min (100.0f, Mathf.Sqrt (recip + 12.0f));
		yield return new WaitForSeconds(.016f);

		if (transform.position.z > 3999) {
			transform.position = new Vector3 (transform.position.x, transform.position.y, 3999);
			rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, -Mathf.Abs (rigidBody.velocity.z));
		}
		///walls! We bounce off the four walls of the world rather than falling out of it
		skyboxCamera.fieldOfView = baseFOV - 1f + cameraZoom;
		backgroundSound.gain = 1.0f / recip;

		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
		fps = Mathf.Lerp (fps, 1.0f / deltaTime, 1.0f/fps);
		botNumber = allbots.transform.childCount;
		if (botNumber > totalBotNumber) botNumber = totalBotNumber;
		//it insists on finding gameObjects when we've killed bots, so we force it to be what we want
		//with this we can tweak sensitivity to things like bot "activityRange"

		if ((fps > 30f) && botNumber < totalBotNumber) {
				ourlevel.GetComponent<SetUpBots>().SpawnBot(-1,false);
		} //generate a bot if we don't have 500 and our FPS is at least 30. Works for locked framerate too as that's bound to 60
		//uses totalBotNumber because if we start killing them, the top number goes down!


		if (fps != prevFps) {
			fpsMesh.text = string.Format ("fps:{0:0.}", fps);
			prevFps = fps;
		}
		if (botNumber != prevBotNumber) {
			botsMesh.text = string.Format("bots:{0:0.}", botNumber);
			prevBotNumber = botNumber;
		}
		//screen readouts. Even in SlowUpdates doing stuff with strings is expensive, so we check to make sure
		//it's necessary

		chooseGuardian += 1;
		if (chooseGuardian > 3) chooseGuardian = 0;
		//we'll just keep this spinning, it's as good as randomizing but much cheaper

		creepToRange -= 0.01f;
		if (creepToRange < 1f) creepToRange = 1800f;
		//bots cluster closer and closer into a big bot party, until suddenly bam! They all flee to the outskirts. Then they start migrating in again.
		//More interesting than the following the player distance.

		activityRange = fps * 8f;
		if (activityRange > 480f) activityRange = 480f;
		//we are dynamically ramping the zone where we automatically park bots.

		yield return new WaitForSeconds(.016f);
	}
}

