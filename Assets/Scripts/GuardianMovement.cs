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
	private float guilt = 0f;
	private RaycastHit hit;
	private PlayerMovement playermovement;
	private SetUpBots setupbots;
	private GameObject level;
	private GameObject logo;

	void Awake ()
	{
		audiosource = GetComponent<AudioSource>();
		myRigidbody = GetComponent<Rigidbody>();
		guardianCore = transform.Find ("Core").GetComponent<Renderer> ().material;
		guardianMiddle = transform.Find ("Middle Layer").GetComponent<Renderer> ().material;
		guardianSurface = transform.Find ("Surface Sphere").GetComponent<Renderer> ().material;
		earthquakeLight = GameObject.FindGameObjectWithTag ("overheadLight");
		externalSource = earthquakeLight.GetComponent<AudioSource> ();
		locationTarget = Vector3.zero;
		ourhero = GameObject.FindGameObjectWithTag ("Player");
		playermovement = ourhero.GetComponent<PlayerMovement>();
		level = GameObject.FindGameObjectWithTag ("Level");
		logo = GameObject.FindGameObjectWithTag ("counterpartlogo");
		setupbots = level.GetComponent<SetUpBots>();
	}

	void OnCollisionEnter(Collision col) {
		float crashScale = Mathf.Sqrt (Vector3.Distance (transform.position, ourhero.transform.position));
		if (setupbots.gameEnded == true) {
			//we aren't messing with it. Hopefully this can
		} else {
			if (!externalSource.isPlaying) {
				externalSource.reverbZoneMix = crashScale * 0.00018f;
				externalSource.clip = earthquakes [Random.Range (0, earthquakes.Length)];
				externalSource.pitch = 0.36f - (crashScale * 0.0032f);
				externalSource.volume = 9f / crashScale;
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

		Color guardianGlow = new Color (1f, 1f, 1f, 0.04f + (guardianCooldown * guardianCooldown * 0.031f));
		guardianCore.SetColor("_TintColor", guardianGlow);
		guardianMiddle.SetColor("_TintColor", guardianGlow);
		guardianSurface.SetColor("_TintColor", guardianGlow);
		//note that it is NOT '_Color' that we are setting with the material dialog in Unity!

		StartCoroutine ("SlowUpdates");

	}

	IEnumerator SlowUpdates () {

		guilt = playermovement.probableGuilt;

		if (guardianCooldown * guilt > 2.0) {
			if (guardianCooldown * guilt > 3.0) {
				locationTarget = ourhero.transform.position;
				//if you make them angry over 3, forget it, they're on you and only you
				//this is where they start to turn white with rage
			} else {
				locationTarget = Vector3.Lerp(locationTarget, ourhero.transform.position, (guardianCooldown-1f)/2f);
				//if guardian anger is over 2, gradually target the player. It dissipates.
			}
		}

		Vector3 rawMove = locationTarget - transform.position;
		rawMove = rawMove.normalized * 180f * guardianCooldown;
		myRigidbody.AddForce (rawMove);
		guardianCooldown -= (0.0005f / myRigidbody.velocity.magnitude);
		//rapidly cool off if it's holding position over a bot, not so much when chasing
		if (guardianCooldown < 0f) {
			guardianCooldown = 0f;
			if (this.tag == "GuardianN") locationTarget = new Vector3(1870f, 314f, 1760f);
			if (this.tag == "GuardianS") locationTarget = new Vector3(2007f, 292f, 1978f);
			rawMove = locationTarget - transform.position;
			rawMove = rawMove.normalized * 80f;
			myRigidbody.AddForce (rawMove);
		}
		yield return new WaitForSeconds (.01f);

		float pitch = 0.5f / Mathf.Sqrt(Vector3.Distance (transform.position, ourhero.transform.position));
		audiosource.pitch = pitch;
		audiosource.volume = 0.3f + guardianCooldown;

		if (Physics.Linecast (transform.position, ourhero.transform.position))
				audiosource.volume = 0.2f + (guardianCooldown);

		yield return new WaitForSeconds (.01f);

	}
}
