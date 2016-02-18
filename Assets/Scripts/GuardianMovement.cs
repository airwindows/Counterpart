using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]

public class GuardianMovement : MonoBehaviour {

	private AudioSource audiosource;
	public AudioClip[] earthquakes;
	private AudioSource externalSource;
	private Rigidbody myRigidbody;
	private Material guardianCore;
	private Material guardianMiddle;
	private Material guardianSurface;
	private float churnCoreVisuals = 1f;
	private float churnMiddleVisuals = 1f;
	private float churnSurfaceVisuals = 1f;
	private GameObject earthquakeLight;
	private GameObject ourhero;
	public Vector3 locationTarget;
	private Vector3 guardianMotion;
	public float guardianCooldown = 0f;
	private RaycastHit hit;
	private PlayerMovement playermovement;
	private SetUpBots setupbots;
	private GameObject level;
	private GameObject logo;

	WaitForSeconds guardianWait = new WaitForSeconds(0.02f);


	void Awake ()
	{
		audiosource = GetComponent<AudioSource>();
		myRigidbody = GetComponent<Rigidbody>();
		guardianCore = transform.Find ("Core").GetComponent<Renderer> ().material;
		guardianMiddle = transform.Find ("Middle Layer").GetComponent<Renderer> ().material;
		guardianSurface = transform.Find ("Surface Sphere").GetComponent<Renderer> ().material;
		earthquakeLight = GameObject.FindGameObjectWithTag ("overheadLight");
		externalSource = earthquakeLight.GetComponent<AudioSource> ();
		//the guardian's one of the more important sounds
		locationTarget = Vector3.zero;
		ourhero = GameObject.FindGameObjectWithTag ("Player");
		playermovement = ourhero.GetComponent<PlayerMovement>();
		level = GameObject.FindGameObjectWithTag ("Level");
		logo = GameObject.FindGameObjectWithTag ("counterpartlogo");
		setupbots = level.GetComponent<SetUpBots>();
		StartCoroutine ("SlowUpdates");
	}

	void OnCollisionEnter(Collision col) {
		float crashScale = Mathf.Sqrt (Vector3.Distance (transform.position, ourhero.transform.position));
		if (setupbots.gameEnded == true) {
			guardianCooldown = 0f;
			//we aren't messing with it. Hopefully this can give a unbreaking end music play
		} else {
			if (!externalSource.isPlaying) {
				externalSource.reverbZoneMix = crashScale * 0.00022f;
				externalSource.clip = earthquakes [Random.Range (0, earthquakes.Length)];
				externalSource.pitch = 0.34f - (crashScale * 0.0033f);
				externalSource.volume = 8f / crashScale;
				externalSource.priority = 3;
				externalSource.Play ();
			}
			if (col.gameObject.tag == "Player") {
				ourhero.GetComponent<SphereCollider> ().material.staticFriction = 0.2f;
				ourhero.GetComponent<Rigidbody> ().freezeRotation = false;
				ourhero.GetComponent<Rigidbody> ().angularDrag = 0.6f;
				setupbots.gameEnded = true;
				setupbots.killed = true;
				Destroy (playermovement);
				logo.GetComponent<Text>().text = "Game Over";
				guardianCooldown = 0f;
			}
			//player is unkillable if they've already won
		}


	} //entire collision
	
	void FixedUpdate () {
		churnCoreVisuals -= (0.01f + (guardianCooldown * 0.001f));
		if (churnCoreVisuals < 0f) churnCoreVisuals += 1f;
		churnMiddleVisuals -= (0.012f + (guardianCooldown * 0.001f));
		if (churnMiddleVisuals < 0f) churnMiddleVisuals += 1f;
		churnSurfaceVisuals -= (0.014f + (guardianCooldown * 0.001f));
		if (churnSurfaceVisuals < 0f) churnSurfaceVisuals += 1f;
		//the churning activity gets more intense as the thing animates

		guardianCore.mainTextureOffset = new Vector2 (0, churnCoreVisuals); //core is the high res one
		guardianMiddle.mainTextureOffset = new Vector2 (0, churnMiddleVisuals); //middle is a coarser layer
		guardianSurface.mainTextureOffset = new Vector2 (0, churnSurfaceVisuals); //surface is low-poly

		Color guardianGlow = new Color (1f, 1f, 1f, 0.06f + (guardianCooldown * guardianCooldown * 0.02f));
		guardianCore.SetColor("_TintColor", guardianGlow);
		guardianMiddle.SetColor("_TintColor", guardianGlow);
		guardianSurface.SetColor("_TintColor", guardianGlow);
		//note that it is NOT '_Color' that we are setting with the material dialog in Unity!
		}

	IEnumerator SlowUpdates () {
		while (true) {
			locationTarget = Vector3.Lerp (locationTarget, ourhero.transform.position, PlayerMovement.guardianHostility);
			if (guardianCooldown > 1)
				guardianCooldown = Mathf.Lerp (guardianCooldown, Mathf.Sqrt (guardianCooldown), 0.01f);
			//this ought to make it go for the player pretty hard if they kill bots.
			//otherwise, it just goes to the source of the altercation which might include you.
			yield return guardianWait;

			Vector3 rawMove = locationTarget - transform.position;
			rawMove = rawMove.normalized * 200f * guardianCooldown;
			myRigidbody.AddForce (rawMove);
			guardianCooldown -= (0.05f / myRigidbody.velocity.magnitude);
			//rapidly cool off if it's holding position over a bot, not so much when chasing
			if (guardianCooldown < 0f) {
				guardianCooldown = 0f;
				locationTarget = new Vector3 (2000f + (Mathf.Sin (Mathf.PI / 180f * playermovement.creepRotAngle) * 2000f), 100f, 2000f + (Mathf.Cos (Mathf.PI / 180f * playermovement.creepRotAngle) * 2000f));
				rawMove = locationTarget - transform.position;
				rawMove = rawMove.normalized * 100f;
				myRigidbody.AddForce (rawMove);
			}
			yield return guardianWait;

			float pitch = 0.5f / Mathf.Sqrt (Vector3.Distance (transform.position, ourhero.transform.position));
			audiosource.pitch = pitch;
			audiosource.priority = 4;
			audiosource.volume = Mathf.Lerp (audiosource.volume, 0.3f + guardianCooldown, 0.001f + (guardianCooldown * 0.1f));
			//ramp up fast but switch off more slowly.
			if (Physics.Linecast (transform.position, ourhero.transform.position))
				audiosource.volume = 0.2f + (guardianCooldown);
			if (setupbots.gameEnded == true)
				audiosource.volume = 0f;

			yield return guardianWait;
		}
	}
}
