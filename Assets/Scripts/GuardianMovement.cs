using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]

public class GuardianMovement : MonoBehaviour
{

	private AudioSource audiosource;
	public AudioClip[] earthquakes;
	public AudioClip BotCrashTinkle;
	private AudioSource externalSource;
	private Rigidbody myRigidbody;
	private Material guardianMiddle;
	private Material guardianSurface;
	private Material guardianAura;
	private Material guardianFarAura;
	private float steps = 768;
	private float swing = 0.01f;
	private float steplength;
	private float quantized; //these four are the settings for the quantize effect
	private float churnCoreVisuals = 1f;
	private float churnMiddleVisuals = 1f;
	private float churnSurfaceVisuals = 1f;
	private float churnFarVisuals = 1f;
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
	private GameObject devnotes;
	WaitForSeconds guardianWait = new WaitForSeconds (0.01f);

	void Awake ()
	{
		audiosource = GetComponent<AudioSource> ();
		myRigidbody = GetComponent<Rigidbody> ();
		guardianMiddle = transform.Find ("Middle Layer").GetComponent<Renderer> ().material;
		guardianSurface = transform.Find ("Surface Sphere").GetComponent<Renderer> ().material;
		guardianAura = transform.Find ("Aura").GetComponent<Renderer> ().material;
		guardianFarAura = transform.Find ("FarAura").GetComponent<Renderer> ().material;
		earthquakeLight = GameObject.FindGameObjectWithTag ("overheadLight");
		externalSource = earthquakeLight.GetComponent<AudioSource> ();
		//the guardian's one of the more important sounds
		locationTarget = Vector3.zero;
		ourhero = GameObject.FindGameObjectWithTag ("Player");
		playermovement = ourhero.GetComponent<PlayerMovement> ();
		level = GameObject.FindGameObjectWithTag ("Level");
		logo = GameObject.FindGameObjectWithTag ("counterpartlogo");
		devnotes = GameObject.FindGameObjectWithTag ("instructionScreen");
		setupbots = level.GetComponent<SetUpBots> ();
		StartCoroutine ("SlowUpdates");
		guardianMiddle.SetColor ("_TintColor", new Color (1f, 1f, 1f, 1f));
		guardianSurface.SetColor ("_TintColor", new Color (1f, 1f, 1f, 1f));
		guardianAura.SetColor ("_TintColor", new Color (1f, 1f, 1f, 1f));
		//note that it is NOT '_Color' that we are setting with the material dialog in Unity!
		//since we have a glowing wireframe around a black hole, the tint is always brightest
	}

	void OnCollisionEnter (Collision col)
	{
		float crashScale = Mathf.Sqrt (Vector3.Distance (transform.position, ourhero.transform.position));
		if (setupbots.gameEnded == true) {
			guardianCooldown = 0f;
			//we aren't messing with it. Hopefully this can give a unbreaking end music play
		} else {
			if (!externalSource.isPlaying) {
				externalSource.clip = earthquakes [Random.Range (0, earthquakes.Length)];
				externalSource.pitch = 0.45f - (crashScale * 0.008f);
				if (Physics.Linecast (transform.position, (ourhero.transform.position + (transform.position - ourhero.transform.position).normalized)) == false) {
					//returns true if there's anything in the way. false means line of sight.
					externalSource.reverbZoneMix = crashScale * 0.006f;
					externalSource.volume = 12f / crashScale;
				} else {
					//occluded, more distant
					externalSource.reverbZoneMix = crashScale * 0.008f;
					externalSource.volume = 10f / crashScale;
				}
				externalSource.priority = 3;
				steplength = playermovement.backgroundMusic.clip.length / steps; //number of quantization steps in the entire loop's length
				quantized = (Mathf.Ceil (playermovement.backgroundMusic.time / steplength) * steplength) + (swing * steplength);
				externalSource.PlayDelayed (quantized - playermovement.backgroundMusic.time);
			}


			if (col.gameObject.tag == "Player") {
				ourhero.GetComponent<SphereCollider> ().material.staticFriction = 0.2f;
				ourhero.GetComponent<Rigidbody> ().freezeRotation = false;
				ourhero.GetComponent<Rigidbody> ().angularDrag = 0.6f;
				setupbots.gameEnded = true;
				setupbots.killed = true;
				GameObject.FindGameObjectWithTag ("Level").GetComponent<AudioSource> ().Stop ();

				Destroy (playermovement);
				externalSource.clip = BotCrashTinkle;
				externalSource.reverbZoneMix = 0f;
				externalSource.pitch = 0.08f;
				externalSource.priority = 3;
				externalSource.volume = 1f;
				externalSource.Play ();
				//
				logo.GetComponent<Text> ().text = "Game Over";
				devnotes.GetComponent<Text> ().text = " ";
				guardianCooldown = 0f;
				PlayerPrefs.SetInt ("levelNumber", 1);
				PlayerPrefs.SetInt ("shots", 1000);
				PlayerPrefs.Save ();
			}
			//player is unkillable if they've already won
		}
	} //entire collision


	void OnParticleCollision (GameObject shotBy)
	{
		guardianCooldown = 2f;
		if (shotBy.CompareTag ("playerPackets")) {
			locationTarget = ourhero.transform.position;
		}
	}
	
	void FixedUpdate ()
	{
		churnCoreVisuals -= (0.001f + (guardianCooldown * 0.001f));
		if (churnCoreVisuals < 0f)
			churnCoreVisuals += 1f;
		churnMiddleVisuals -= (0.0013f + (guardianCooldown * 0.0013f));
		if (churnMiddleVisuals < 0f)
			churnMiddleVisuals += 1f;
		churnSurfaceVisuals -= (0.0017f + (guardianCooldown * 0.0017f));
		if (churnSurfaceVisuals < 0f)
			churnSurfaceVisuals += 1f;
		churnFarVisuals -= (0.0021f + (guardianCooldown * 0.0021f));
		if (churnFarVisuals < 0f)
			churnFarVisuals += 1f;
		//the churning activity gets more intense as the thing animates

		guardianMiddle.mainTextureOffset = new Vector2 (0, churnCoreVisuals); //middle is a coarser layer
		guardianSurface.mainTextureOffset = new Vector2 (0, churnMiddleVisuals); //surface is low-poly
		guardianAura.mainTextureOffset = new Vector2 (0, churnSurfaceVisuals); //aura is high-poly but fast
		guardianFarAura.mainTextureOffset = new Vector2 (0, churnFarVisuals); //aura is high-poly but fast
	}

	IEnumerator SlowUpdates ()
	{
		while (true) {
			if (guardianCooldown > 1f)
				guardianCooldown -= 0.02f;
			if (churnCoreVisuals * 2f < guardianCooldown)
				locationTarget = ourhero.transform.position;
			//alternate way to deal with hyper guardians?
			yield return guardianWait;

			Vector3 rawMove = locationTarget - transform.position;
			rawMove = rawMove.normalized * 40f * guardianCooldown;
			myRigidbody.AddForce (rawMove);
			if (guardianCooldown > 7f)
				myRigidbody.velocity *= 0.86f;
			if (guardianCooldown > 4f)
				guardianCooldown = 3f;
			//safeguard against crazy psycho zapping around

			if (guardianCooldown < 0f) {
				guardianCooldown = 0f;
				rawMove = locationTarget - transform.position;
				rawMove = rawMove.normalized * playermovement.terrainHeight;
				myRigidbody.AddForce (rawMove);
			}
			yield return guardianWait;

			float pitch = 0.5f / Mathf.Sqrt (Vector3.Distance (transform.position, ourhero.transform.position));
			audiosource.pitch = pitch;
			audiosource.priority = 4;
			audiosource.reverbZoneMix = 0.6f - pitch;
			float targetVolume = 0.05f;
			if (Physics.Linecast (transform.position, (ourhero.transform.position + (transform.position - ourhero.transform.position).normalized)) == false) {
				//returns true if there's anything in the way. false means line of sight.
				targetVolume = 0.15f;
			} else {
				//since there's something in the way, let's tame the beast
				guardianCooldown *= 0.98f;
			}
			if (setupbots.gameEnded == true)
				targetVolume = 0f;

			audiosource.volume = Mathf.Lerp (audiosource.volume, targetVolume, 0.1f);
			//ramp up fast but switch off more slowly.

			yield return guardianWait;
		}
	}
}
