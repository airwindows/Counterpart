using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BackgroundSound))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
	public GameObject ourlevel;
	public static int levelNumber;
	public static int maxlevelNumber;
	public static int playerScore;
	public static Vector3 playerPosition = new Vector3 (403f, 2000f, 521f);
	public static Quaternion playerRotation = new Quaternion (0f, 0f, 0f, 0f);
	public static float initialTurn = 0f;
	public static float initialUpDown = 0f;
	public Camera mainCamera;
	public Camera wireframeCamera;
	public Camera skyboxCamera;
	public Material overlayBoxMat;
	public Material overlayBoxMat2;
	public GameObject maxbotsText;
	public GameObject countdownText;
	public Text maxbotsTextObj;
	public Text countdownTextObj;
	public GameObject cameraDolly;
	public ParticleSystem particlesystem;
	public AnimationCurve slopeCurveModifier = new AnimationCurve (new Keyframe (-90.0f, 1.0f), new Keyframe (0.0f, 1.0f), new Keyframe (90.0f, 0.0f));
	public int yourMatch;
	public Color32[] yourBrain;
	public float activityRange = 30f;
	private int brainPointer;
	private float altitude = 1f;
	private Rigidbody rigidBody;
	private Vector3 startPosition;
	private Vector3 endPosition;
	private float stepsBetween;
	private LayerMask onlyTerrains;
	private SphereCollider sphereCollider;
	private AudioSource audiosource;
	public float yourMatchDistance = 9999;
	public bool yourMatchOccluded = true;
	private int pingTimer = 2;
	private bool pingGeiger = false;
	private BackgroundSound backgroundSound;
	public AudioClip botBeep;
	public float baseFOV = 68f;
	public float mouseSensitivity;
	public float mouseDrag = 0f;
	public float baseJump;
	public float maximumBank = 1f;
	private Vector3 desiredMove = Vector3.zero;
	private bool releaseJump = true;
	private float moveLimit = 0f;
	private float deltaTime = 0f;
	private float xRot = 0f;
	private float yRot = 0f;
	private float yCorrect = 0f;
	private float zRot = 0f;
	private bool reflected;
	public float terrainHeight;
	public float clampRotateAngle = Mathf.PI * 2f;
	public Vector3 desiredAimOffsetPosition;
	public int botNumber;
	public int totalBotNumber;
	private int blurHack;
	private Quaternion blurHackQuaternion;
	private GameObject allbots;
	private GameObject guardian;
	private GuardianMovement guardianmovement;
	public Vector3 locationOfCounterpart;
	private RaycastHit hit;
	private Vector2 input = Vector2.zero;
	private float cameraZoom = 0f;
	public float creepToRange;
	public float startAtRange;
	public float creepRotAngle = 1f;
	public int residueSequence = 1;
	private float velCompensated = 0.00025f;
	private Vector3 positionOffset = new Vector3 (20f, -20f, 0f);
	private GameObject level;
	private SetUpBots setupbots;
	public Renderer ourbody;
	private bool supportsRenderTextures;
	WaitForSeconds playerWait = new WaitForSeconds (0.015f);

	void Awake ()
	{
		backgroundSound = GetComponent<BackgroundSound> ();
		rigidBody = GetComponent<Rigidbody> ();
		sphereCollider = GetComponent<SphereCollider> ();
		audiosource = GetComponent<AudioSource> ();
		audiosource.priority = 1;
		//stuff bolted onto the player is always most important
		allbots = GameObject.FindGameObjectWithTag ("AllBots").gameObject;
		guardian = GameObject.FindGameObjectWithTag ("GuardianN").gameObject;
		ourbody = transform.FindChild ("PlayerBody").GetComponent<Renderer> ();
		supportsRenderTextures = SystemInfo.supportsRenderTextures;
		guardianmovement = guardian.GetComponent<GuardianMovement> ();
		onlyTerrains = 1 << LayerMask.NameToLayer ("Wireframe");
		level = GameObject.FindGameObjectWithTag ("Level");
		setupbots = level.GetComponent<SetUpBots> ();
		locationOfCounterpart = Vector3.zero;
		levelNumber = PlayerPrefs.GetInt ("levelNumber", 1);
		reflected = false;
		residueSequence = (int)Mathf.Pow (levelNumber, 4);
		startAtRange = ((Mathf.Pow (residueSequence, 2) % Mathf.Pow (PlayerMovement.levelNumber, 2)) % 300) + 10;
		creepRotAngle = (residueSequence % 359);
		botNumber = (int)(Mathf.Pow (PlayerMovement.levelNumber, 2) % 900) + 2;
		if (botNumber < 700)
			botNumber = botNumber % 300;
		//more likely to get WTF crowds, but typically it's more moderate
		totalBotNumber = botNumber;
		creepToRange = ((residueSequence % botNumber) % 468) + Mathf.Sqrt (botNumber);
		terrainHeight = ((Mathf.Pow (residueSequence, 2) % 20) + Mathf.Pow (Mathf.Pow (residueSequence, 5) % levelNumber, 2)) % 700;
		if (terrainHeight < 680)
			terrainHeight = terrainHeight % 400;
		//leave a few WTF levels in there but most will be usable
	}

	void Start ()
	{
		if (Physics.Raycast (playerPosition, Vector3.down, out hit))
			playerPosition = hit.point + Vector3.up;
		transform.position = playerPosition;
		startPosition = transform.position;
		endPosition = transform.position;
		stepsBetween = 0f;
		blurHack = 0;
		guardianmovement.locationTarget = new Vector3 (500f + (Mathf.Sin (Mathf.PI / 180f * creepRotAngle) * 400f), 1000f, 500f + (Mathf.Cos (Mathf.PI / 180f * creepRotAngle) * 400f));
		guardian.transform.position = guardianmovement.locationTarget;
		//set up the scary monster to be faaaar away to start. It will circle.
		StartCoroutine ("SlowUpdates");
		//start this only once with a continuous loop inside the coroutine
	}

	void OnApplicationQuit ()
	{
		if (setupbots.gameEnded != true) {
			PlayerPrefs.SetInt ("levelNumber", 1);
			PlayerPrefs.Save ();
			//if we are quitting, it's like a total reset. Arcade mode.
			//BUT, if we're quitting out of the win screen we can resume.
		}
	}

	void Update ()
	{
		//Each frame we run Update, regardless of what game control/physics is doing. This is the fundamental 'tick' of the game but it's wildly time-variant: it goes as fast as possible.
		cameraDolly.transform.localPosition = Vector3.Lerp (startPosition, endPosition, stepsBetween);
		stepsBetween += (Time.deltaTime * Time.fixedDeltaTime);
		input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		if (Input.GetButton ("MouseForward"))
			input.y = 1;

		float tempMouse = Input.GetAxisRaw ("MouseX");
		if (tempMouse > 0)
			tempMouse = (Mathf.Sqrt (tempMouse + 81f) - 9f) / mouseSensitivity;
		if (tempMouse < 0)
			tempMouse = -(Mathf.Sqrt (-tempMouse + 81f) - 9f) / mouseSensitivity;
		initialTurn = Mathf.Lerp(initialTurn, initialTurn-tempMouse, 0.618f);
		mouseDrag = Mathf.Abs (tempMouse); //maximum of h and v is used later as a physics drag factor
		
		tempMouse = Input.GetAxisRaw ("MouseY");
		if (tempMouse > 0)
			tempMouse = (Mathf.Sqrt (tempMouse + 81f) - 9f) / mouseSensitivity;
		if (tempMouse < 0)
			tempMouse = -(Mathf.Sqrt (-tempMouse + 81f) - 9f) / mouseSensitivity;
		initialUpDown = Mathf.Lerp(initialUpDown, initialUpDown+tempMouse, 0.618f);
		if (Mathf.Abs (tempMouse) > mouseDrag)
			mouseDrag = Mathf.Abs (tempMouse); //physics drag factor

		if (initialTurn < 0f)
			initialTurn += clampRotateAngle;
		if (initialTurn > clampRotateAngle)
			initialTurn -= clampRotateAngle;
		initialUpDown = Mathf.Clamp (initialUpDown, -1.5f, 1.5f);
		//mouse is instantaneous so it can be in Update.

		if (supportsRenderTextures) {
			blurHack += 1;
			if (blurHack > 1)
				blurHack = 0;
			blurHackQuaternion = wireframeCamera.transform.localRotation;
			if (blurHack == 0) {
				blurHackQuaternion.y = velCompensated;
				blurHackQuaternion.x = velCompensated;
				wireframeCamera.transform.localPosition = positionOffset * -velCompensated;
			}
			if (blurHack == 1) {
				blurHackQuaternion.y = -velCompensated;
				blurHackQuaternion.x = -velCompensated;
				wireframeCamera.transform.localPosition = positionOffset * velCompensated;
			}
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
		
		if ((Input.GetButton ("KeyboardJump") || Input.GetButton ("MouseJump")) && releaseJump) {
			if (Physics.Raycast (transform.position, Vector3.down, out hit)) {
				rigidBody.AddForce (Vector3.up * baseJump / Mathf.Pow (hit.distance, 3), ForceMode.Impulse);
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
		playerPosition = transform.position;
		playerRotation = transform.rotation;
		Vector3 groundContactNormal;
		Vector3 rawMove = mainCamera.transform.forward * input.y + mainCamera.transform.right * input.x;
		float adjacentSolid = 99999;
		
		releaseJump = true;
		//it's FixedUpdate, so release the jump in Update again so it can be retriggered.
		
		particlesystem.transform.localPosition = Vector3.forward * (1f + (rigidBody.velocity.magnitude * Time.fixedDeltaTime));
		if (Input.GetButton ("KeyboardTalk") || Input.GetButton ("MouseTalk")) {
			if (!particlesystem.isPlaying)
				particlesystem.Play ();
			particlesystem.Emit (1);
		}
		if (Physics.SphereCast (transform.position, sphereCollider.radius, Vector3.down, out hit, 99999f, onlyTerrains)) {
			groundContactNormal = hit.normal;
			desiredMove = Vector3.ProjectOnPlane (rawMove, groundContactNormal).normalized;
			//set this up here so the fuel can take advantage of the ground proximity
		} else {
			groundContactNormal = Vector3.down;
			desiredMove = Vector3.ProjectOnPlane (rawMove, groundContactNormal).normalized;
		}
		pingTimer += 1;
		if (pingTimer > yourMatchDistance) {
			pingTimer = 0;
			pingGeiger = true;
		}
		//only need to update the ping timer
		
		if (Physics.Raycast (transform.position, Vector3.down, out hit, 99999f, onlyTerrains)) {
			altitude = hit.distance;
			if (adjacentSolid > altitude)
				adjacentSolid = altitude;
		} else {
			if (Physics.Raycast (transform.position + (Vector3.up * 9999f), Vector3.down, out hit, 99999f, onlyTerrains)) {
				transform.position = hit.point + Vector3.up;
				rigidBody.velocity += Vector3.up;
				altitude = 1;
			}
		}

		if (adjacentSolid < 1f) {
			float bumpUp = transform.position.y + ((1f - adjacentSolid) / 32f);
			transform.position = new Vector3 (transform.position.x, bumpUp, transform.position.z);
			//this keeps us off the ground
			reflected = false;
			adjacentSolid = 1f;
		}
		//thus we can only maneuver if we are near a surface
		
		adjacentSolid *= adjacentSolid;
		adjacentSolid *= adjacentSolid;
		
		if (desiredMove.y > 0f) {
			float angle = Vector3.Angle (groundContactNormal, Vector3.up);
			moveLimit = slopeCurveModifier.Evaluate (angle);
			//apply a slope based limiting factor
			
			desiredMove *= moveLimit;
		}

		float momentum = Mathf.Sqrt (Vector3.Angle (mainCamera.transform.forward, rigidBody.velocity) + 2f + mouseDrag) * 0.1f;
		//3 controls the top speed, 0.1 controls maximum clamp when turning
		if (momentum < 0.001f)
			momentum = 0.001f; //insanity check
		if (adjacentSolid < 1f)
			adjacentSolid = 1f; //insanity check
		if (momentum > adjacentSolid)
			momentum = adjacentSolid; //insanity check

		momentum += (rigidBody.velocity.sqrMagnitude * 0.0001f);

		desiredMove /= (adjacentSolid + mouseDrag);
		//we're adding the move to the extent that we're near a surface
		rigidBody.drag = momentum / adjacentSolid;
		//alternately, we have high drag if we're near a surface and little in the air
		
		rigidBody.AddForce (desiredMove, ForceMode.Impulse);

		stepsBetween = 0f;
		//zero out the step-making part and start over
		startPosition = Vector3.zero;
		endPosition = rigidBody.velocity * Time.fixedDeltaTime;
		//we see if this will work. Certainly we want to scale it to fixedDeltaTime as we're in FixedUpdate
	}

	IEnumerator SlowUpdates ()
	{
		while (true) {
			if (Cursor.lockState != CursorLockMode.Locked) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			//the notorious cursor code! Kills builds on Unity 5.2 and up

			if (reflected == false && (transform.position.y < -1f || transform.position.y > (terrainHeight + 1000f))) {
				rigidBody.velocity = -rigidBody.velocity;
				reflected = true;
			}

			if (setupbots.gameEnded && (Input.GetButton ("NextLevel"))) {
				//trigger new level load on completing of level
				//we have already updated the score and saved prefs
				Application.LoadLevel ("Scene");
			}
					
			if ((audiosource.clip != botBeep) && audiosource.isPlaying) {
				//we are playing the smashing sounds of the giant guardians
			} else {
				audiosource.pitch = 3f;
				if (pingGeiger == true) {
					pingGeiger = false;
					//reset trigger for our slow update ping
					if (ourlevel.GetComponent<SetUpBots> ().gameEnded == false) {
						if (audiosource.clip != botBeep)
							audiosource.clip = botBeep;
						if (yourMatchOccluded) {
							audiosource.volume = 0.275f;
							audiosource.reverbZoneMix = 1f;
						} else {
							audiosource.volume = 0.3f;
							audiosource.reverbZoneMix = 0.3f;
						}
						if (yourMatchDistance > 100) {
							audiosource.volume = 0.25f;
							audiosource.reverbZoneMix = 2.0f;
							if (yourMatchDistance > 200) {
								audiosource.volume = 0.2f;
								audiosource.reverbZoneMix = 4.0f;
							}
						}
						audiosource.Play ();
					}
				}
				//this is our geiger counter for our bot
			}
			if (botNumber < totalBotNumber) {
				ourlevel.GetComponent<SetUpBots> ().SpawnBot (-1);
			} //generate a bot if we don't have 500 and our FPS is at least 58. Works for locked framerate too as that's bound to 60
			//uses totalBotNumber because if we start killing them, the top number goes down!
			//thus, if we have insano framerates, the bots can spawn incredibly fast, but it'll sort of ride the wave if it begins to chug
			
			yield return playerWait;

			cameraZoom = (Mathf.Sqrt (rigidBody.velocity.magnitude + 2f) * 2f) + (initialUpDown * 4f) + (playerPosition.y / 200f);
			//elaborate zoom goes wide angle for looking up, and for high ground

			if (transform.position.y < -1) {
				//rigidBody.velocity *= 0.99f;
				guardianmovement.guardianCooldown = 4f;
				guardianmovement.locationTarget = transform.position;
				//call the guardian!
				backgroundSound.whoosh *= 0.999f;
				backgroundSound.gain *= 0.999f;
				backgroundSound.brightness *= 0.999f;
				//dead players don't whoosh, nor do they whoosh falling out of the world
			} else {
				if (altitude < 1f) {
					backgroundSound.whooshLowCut = Mathf.Lerp (backgroundSound.whooshLowCut, 0.001f, 0.5f);
					backgroundSound.whoosh = (rigidBody.velocity.magnitude * Mathf.Sqrt (rigidBody.velocity.magnitude) * 0.00005f);
				} else {
					backgroundSound.whooshLowCut = Mathf.Lerp (backgroundSound.whooshLowCut, 0.2f, 0.5f);
					backgroundSound.whoosh = (rigidBody.velocity.magnitude * Mathf.Sqrt (rigidBody.velocity.magnitude) * 0.00003f);
				}
				backgroundSound.brightness = (transform.position.y / 900.0f) + 0.1f;
			}

			mainCamera.fieldOfView = baseFOV + (cameraZoom * 0.5f);
			yield return playerWait;

			wireframeCamera.fieldOfView = baseFOV + (cameraZoom * 0.5f);
			float recip = 1.0f / backgroundSound.gain;
			recip = Mathf.Lerp ((float)recip, altitude, 0.5f);
			recip = Mathf.Min (100.0f, Mathf.Sqrt (recip + 10.0f));

			if (botNumber < totalBotNumber) {
				ourlevel.GetComponent<SetUpBots> ().SpawnBot (-1);
			} //generate a bot if we don't have 500 and our FPS is at least 58. Works for locked framerate too as that's bound to 60
			//uses totalBotNumber because if we start killing them, the top number goes down!
			//thus, if we have insano framerates, the bots can spawn incredibly fast, but it'll sort of ride the wave if it begins to chug

			yield return playerWait;

			if (transform.position.y < -10) {
				transform.position = new Vector3 (transform.position.x, -100, transform.position.z);
				playerPosition = transform.position;
			}

			skyboxCamera.fieldOfView = baseFOV + (cameraZoom * 0.7f);

			backgroundSound.gain = 1.0f / recip;

			botNumber = allbots.transform.childCount;
			if (botNumber > totalBotNumber)
				botNumber = totalBotNumber;
			//it insists on finding gameObjects when we've killed bots, so we force it to be what we want
			//with this we can tweak sensitivity to things like bot "activityRange"

			deltaTime += (Time.deltaTime - deltaTime) * 0.01f;

			if (botNumber < totalBotNumber) {
				ourlevel.GetComponent<SetUpBots> ().SpawnBot (-1);
			} //generate a bot if we don't have 500 and our FPS is at least 58. Works for locked framerate too as that's bound to 60
			//uses totalBotNumber because if we start killing them, the top number goes down!
			//thus, if we have insano framerates, the bots can spawn incredibly fast, but it'll sort of ride the wave if it begins to chug
			yield return playerWait;
		}
	}
}

