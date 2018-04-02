using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]

public class GuardianMovement : MonoBehaviour
{

	private AudioSource audiosource;
	private LayerMask onlyTerrains;
	public AudioClip[] earthquakes;
	public AudioClip BotCrashTinkle;
	private AudioSource externalSource;
	private Rigidbody myRigidbody;
	private Vector3 rawMove;
	private Transform guardianMiddle;
	private Transform guardianSurface;
	private Transform guardianAura;
	private Transform guardianFarAura;
	private float steps = 768;
	private float swing = 0.01f;
	private float steplength;
	private float quantized; //these four are the settings for the quantize effect
	public float auraSize = 0f;
	private GameObject earthquakeLight;
	private GameObject ourhero;
	private BotMovement targetbotbrain;
	private Renderer targetbotcolor;
	public Vector3 locationTarget;
	public bool afterPlayer;
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
		onlyTerrains = 1 << LayerMask.NameToLayer ("Wireframe");
		guardianMiddle = transform.Find ("Middle Layer");
		guardianSurface = transform.Find ("Surface Sphere");
		guardianAura = transform.Find ("Aura");
		guardianFarAura = transform.Find ("FarAura");
		earthquakeLight = GameObject.FindGameObjectWithTag ("overheadLight");
		externalSource = earthquakeLight.GetComponent<AudioSource> ();
		//the guardian's one of the more important sounds
		locationTarget = Vector3.zero;
		afterPlayer = false;
		ourhero = GameObject.FindGameObjectWithTag ("Player");
		playermovement = ourhero.GetComponent<PlayerMovement> ();
		level = GameObject.FindGameObjectWithTag ("Level");
		logo = GameObject.FindGameObjectWithTag ("counterpartlogo");
		devnotes = GameObject.FindGameObjectWithTag ("instructionScreen");
		setupbots = level.GetComponent<SetUpBots> ();
		StartCoroutine ("SlowUpdates");
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
				steplength = playermovement.backgroundMusic.clip.length / steps / 32f; //number of quantization steps in the entire loop's length
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
				if (QualitySettings.maximumLODLevel == 2) {
					PlayerPrefs.SetInt ("levelNumber", 1);
					//if we're on hardcore mode, we reset the level.
					//Failing to do this means we'll just be retrying the level again next game.
				} else {
					devnotes.GetComponent<Text> ().text = "Press e to retry!";
					//telegraph that we're still in easy mode and you can try again
				}
				PlayerPrefs.Save ();
			}

			if (col.gameObject.tag == "Bot") {
				targetbotbrain = col.gameObject.GetComponent<BotMovement> ();
				if (targetbotbrain != null && targetbotbrain.yourMatch != playermovement.yourMatch) {
					Destroy (targetbotbrain);
					targetbotcolor = col.gameObject.transform.FindChild ("Dolly").GetComponent<Renderer> ();
					targetbotcolor.material.color = new Color (0.1f, 0.1f, 0.1f);
					col.gameObject.GetComponent<SphereCollider> ().material.staticFriction = 0.2f;
					col.gameObject.GetComponent<Rigidbody> ().freezeRotation = false;
					col.gameObject.GetComponent<Rigidbody> ().angularDrag = 0.6f;
				}
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
		guardianMiddle.localScale = new Vector3 (1, 1, 1) * (auraSize + 1.0f);
		guardianSurface.localScale = new Vector3 (3, 3, 3) * (auraSize + 0.333333f);
		guardianAura.localScale = new Vector3 (9, 9, 9) * (auraSize + 0.11111f);
		guardianFarAura.localScale = new Vector3 (27, 27, 27) * (auraSize + 0.037037f);

		if (Physics.Raycast (transform.position, Vector3.up, out hit, 9999f, onlyTerrains)) {
			transform.position = hit.point + Vector3.up;
			myRigidbody.velocity += Vector3.up;
		}
	}

	IEnumerator SlowUpdates ()
	{
		while (true) {
			rawMove = locationTarget - transform.position;
			rawMove = rawMove.normalized * 30f * guardianCooldown;
			if (Physics.Raycast (transform.position, Vector3.down, out hit, 9999f, onlyTerrains) && hit.distance < 11f) {
				myRigidbody.AddForce (rawMove);
				if (guardianCooldown > 5f)
					myRigidbody.velocity *= 0.9f;
				if (guardianCooldown > 3f)
					guardianCooldown = 3f;
				//safeguard against crazy psycho zapping around
				
				if (guardianCooldown < 0f) {
					guardianCooldown = 0f;
					afterPlayer = false;
					rawMove = locationTarget - transform.position;
					rawMove = rawMove.normalized * playermovement.terrainHeight;
					myRigidbody.AddForce (rawMove);
				}
			}

			if (setupbots.gameEnded == true && setupbots.killed == true && QualitySettings.maximumLODLevel == 1 && Input.GetButton ("NextLevel")) {
				//trigger new level load if player's dead and we're in easy mode and you go for next (same) level
				Application.LoadLevel ("Scene");
			}
			yield return guardianWait;

			float pitch = 0.5f / Mathf.Sqrt (Vector3.Distance (transform.position, ourhero.transform.position));
			audiosource.pitch = pitch;
			audiosource.priority = 4;
			audiosource.reverbZoneMix = 0.6f - pitch;
			float targetVolume = 0.025f;
			if (Physics.Linecast (transform.position, (ourhero.transform.position + (transform.position - ourhero.transform.position).normalized)) == false) {
				//returns true if there's anything in the way. false means line of sight.
				targetVolume = 0.15f;
				if (afterPlayer == true) {
					locationTarget.x = Mathf.Lerp (locationTarget.x, ourhero.transform.position.x, (playermovement.attractAttention + 0.002f));
					locationTarget.y = Mathf.Lerp (locationTarget.y, ourhero.transform.position.y, (playermovement.attractAttention + 0.002f));
					locationTarget.z = Mathf.Lerp (locationTarget.z, ourhero.transform.position.z, (playermovement.attractAttention + 0.002f));
					//if you're moving a lot or jumping the Guardian can chase you.
					auraSize = Mathf.Lerp (auraSize, guardianCooldown, 0.1f);
				}
			} else {
				//since there's something in the way, let's tame the beast
				guardianCooldown *= 0.994f;
				auraSize = guardianCooldown;
			}

			audiosource.volume = Mathf.Lerp (audiosource.volume, targetVolume, 0.1f);
			//ramp up fast but switch off more slowly.
			yield return guardianWait;

			if (transform.position.x < 0f) {
				transform.position = new Vector3 (transform.position.x + 1000f, transform.position.y, transform.position.z);
			}
			if (transform.position.x > 1000f) {
				transform.position = new Vector3 (transform.position.x - 1000f, transform.position.y, transform.position.z);
			}			
			if (transform.position.z < 0f) {
				transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z + 1000f);
			}
			if (transform.position.z > 1000f) {
				transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z - 1000f);
			}
			if (transform.position.y > 1000f || transform.position.y < -1000f) {
				transform.position = new Vector3 (transform.position.x, 400f, transform.position.z);
			}
			if (Physics.Raycast (transform.position, Vector3.up, out hit, 9999f, onlyTerrains)) {
				transform.position = hit.point + Vector3.up;
			}
		}
	}
}
