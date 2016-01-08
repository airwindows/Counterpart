using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]

public class BotMovement : MonoBehaviour
{
	public Vector3 botTarget;
	public Mesh meshLOD0;
	public Mesh meshLOD1;
	public Mesh meshLOD2;
	public Mesh meshLOD3;
	public Mesh meshLOD4;
	public int yourMatch;
	public bool withinRange;
	private Vector3 rawMove;
	private Vector3 desiredMove;
	private Vector3 storedVelocity = Vector3.zero;
	private Vector3 lerpedMove = Vector3.zero;
	private Vector3 groundContactNormal;
	public AudioClip BotCrash;
	public AudioClip BotCrashTinkle;
	public AudioClip BotBeep;
	public AudioClip happyEnding;
	private bool notEnded;
	private AudioSource audioSource;
	public Color32[] botBrain;
	public int brainPointer = 0;
	public int voicePointer = 0;
	private float altitude = 1f;
	private float adjacentSolid = 99999;
	private int brainR;
	private int brainG;
	private int brainB;
	public int step;
	private SphereCollider sphereCollider;
	private Rigidbody rigidBody;
	private MeshFilter meshfilter;
	private Renderer myColor;
	private RaycastHit hit;
	private GameObject ourhero;
	private Camera maincamera;
	private PlayerMovement playermovement;
	private GameObject level;
	private SetUpBots setupbots;

	void Awake ()
	{
		rigidBody = GetComponent<Rigidbody> ();
		sphereCollider = GetComponent<SphereCollider>();
		meshfilter = GetComponent<MeshFilter>();
		myColor = GetComponent<Renderer>();
		audioSource = GetComponent<AudioSource>();
		ourhero = GameObject.FindGameObjectWithTag ("Player");
		playermovement = ourhero.GetComponent<PlayerMovement>();
		maincamera = playermovement.mainCamera;
		level = GameObject.FindGameObjectWithTag ("Level");
		setupbots = level.GetComponent<SetUpBots>();
		notEnded = true;
	}

	void OnCollisionEnter(Collision col) {
		if (col.gameObject.tag == "Player" && notEnded) {

			if (col.relativeVelocity.magnitude > 25f) {
				brainPointer += 1;
				if (brainPointer >= botBrain.Length) brainPointer = 0;
				//bots hit hard enough to crash get discombot-ulated
				audioSource.clip = BotCrash;
				audioSource.pitch = 3f - ((col.relativeVelocity.magnitude - 25f) * 0.1f);
				audioSource.volume = 0.3f + ((col.relativeVelocity.magnitude - 25f) * 0.02f);
				if (col.relativeVelocity.magnitude > 45f) {
					audioSource.clip = BotCrashTinkle;
					audioSource.pitch = 1.0f - ((col.relativeVelocity.magnitude) * 0.001f);
					if (audioSource.pitch < 0.2f) audioSource.pitch = 0.2f;
					audioSource.volume = 1f;
					if (playermovement.yourMatch == yourMatch) {
						myColor.material.color = new Color (0.35f, 0.35f, 0.35f);
						sphereCollider.material.staticFriction = 0.2f;
						Destroy (this);
						//REKKT. Bot's brain is destroyed and since it was your soulmate...
						ourhero.GetComponent<SphereCollider> ().material.staticFriction = 0.2f;
						ourhero.GetComponent<Rigidbody> ().freezeRotation = false;
						setupbots.gameEnded = true;
						Destroy (playermovement);
						//you are REKKT too!
						audioSource.clip = BotCrashTinkle;
						audioSource.pitch = 0.1f;
						audioSource.volume = 1f;
						//override with an epic fail crash
					} else {
						myColor.material.color = new Color (0.36f, 0.36f, 0.36f);
						sphereCollider.material.staticFriction = 0.2f;
						rigidBody.freezeRotation = false;
						Destroy (this);
						//REKKT. Bot's brain is destroyed, after setting its color to dim.
						playermovement.baseJump = playermovement.baseJump * 0.98f;
						playermovement.totalBotNumber = playermovement.totalBotNumber - 1;
						//if you go around killing bots you lose your jump.
					}//when over 40, decide if you kill entire game or just the other bot.
				}//also over 25
				audioSource.Play ();
				//play if over 15 or more
			} else {
				if (playermovement.yourMatch == yourMatch) {
					rigidBody.velocity = Vector3.zero;
					lerpedMove = Vector3.zero;
					playermovement.gameObject.GetComponent<Rigidbody> ().velocity = Vector3.zero;
					//freeze, in shock and delight!
					audioSource.clip = happyEnding;
					audioSource.pitch = 1f;
					audioSource.volume = 1f;
					audioSource.spatialBlend = 0f;
					//switch this bot to normal stereo, music playback
					audioSource.PlayOneShot (happyEnding, 1f);
					notEnded = false;
					//with that, we switch off the bot this is
					setupbots.gameEnded = true;
				} else {
					voicePointer += 1;
					if (voicePointer >= botBrain.Length) voicePointer = 0;
					int left = Math.Abs (playermovement.yourBrain [voicePointer].r - botBrain [voicePointer].r);
					int right = Math.Abs (playermovement.yourBrain [voicePointer].g - botBrain [voicePointer].g);
					int center = Math.Abs (playermovement.yourBrain [voicePointer].b - botBrain [voicePointer].b);
					if (notEnded && withinRange) {
						if (audioSource.clip != BotBeep) audioSource.clip = BotBeep;
						audioSource.volume = 0.3f;
						float voicePitch = Mathf.Abs(2.9f - ((center + left + right) * 0.0045f));
						//bounce the subsonic notes back up again
						audioSource.pitch = voicePitch + 0.1f;
						if (!audioSource.isPlaying) audioSource.Play ();
					}
					//bot talks without interrupting its dance, if you're gentle
				}
			} //decide if it's a hit or a kiss
			//with the player
		} else {
			BotMovement botmovement = col.gameObject.GetComponent<BotMovement>();
			if (botmovement != null) {
				if (botmovement.yourMatch == yourMatch) {
					botmovement.brainPointer = Math.Abs (brainPointer - 1);
					//offset-by-one pointer for conga line effect
					botmovement.step = 9999;
					//step is always set to what will engage the brain and do a new pointer
					if (notEnded && withinRange) {
						if (audioSource.clip != BotBeep) audioSource.clip = BotBeep;
						audioSource.volume = 0.8f;
						brainR = botBrain [brainPointer].r;
						brainG = botBrain [brainPointer].g;
						brainB = botBrain [brainPointer].b;
						float voicePitch = Mathf.Abs(2.9f - ((brainR + brainG + brainB) * 0.0045f));
						//bounce the subsonic notes back up again
						audioSource.pitch = voicePitch + 0.1f;
						if (!audioSource.isPlaying) audioSource.Play ();
					//bot makes a remark, unless it's too far to hear
					}
				}
				//upon hitting another bot, if they're the same, they sync brainwaves.
			}
			//with another bot
		} 
	} //entire collision

	void OnParticleCollision(GameObject shotBy) {
		voicePointer += 1;
		if (voicePointer >= botBrain.Length) voicePointer = 0;
		int left = Math.Abs (playermovement.yourBrain[voicePointer].r - botBrain [voicePointer].r);
		int right = Math.Abs (playermovement.yourBrain[voicePointer].g - botBrain [voicePointer].g);
		int center = Math.Abs (playermovement.yourBrain[voicePointer].b - botBrain [voicePointer].b);
		if (notEnded && withinRange) {
			if (audioSource.clip != BotBeep) audioSource.clip = BotBeep;
			audioSource.volume = 1f;
			float voicePitch = Mathf.Abs(2.9f - ((center + left + right) * 0.0045f));
			//bounce the subsonic notes back up again
			audioSource.pitch = voicePitch + 0.1f;
			if (!audioSource.isPlaying) audioSource.Play ();
		}
		rigidBody.velocity = Vector3.Lerp (rigidBody.velocity, Vector3.Lerp(ourhero.GetComponent<Rigidbody>().velocity, Vector3.zero, 0.5f), (botBrain [voicePointer].g / 127f));
		//bots that are more than 50% G (greens and whites) are cooperative and stop to talk. Dark or nongreen bots won't.
//		maincamera.transform.LookAt (Vector3.Lerp(playermovement.desiredAimOffsetPosition, myColor.bounds.center, 0.01f));
//		Quaternion lookHack = maincamera.transform.rotation;
		//lookHack.z = 1f;
//		playermovement.initialTurn = lookHack.eulerAngles.x/2f;
//		playermovement.initialUpDown = lookHack.eulerAngles.z/2f;
//		Debug.Log (playermovement.initialTurn + " - " + playermovement.initialUpDown);
	}
	
	void FixedUpdate ()
	{
		//FixedUpdate is run as many times as needed, before an Update step: or, it's skipped if framerate is super high.		
		 adjacentSolid = 99999;
		
		if (Physics.SphereCast (transform.position, sphereCollider.radius, Vector3.down, out hit, sphereCollider.radius)) groundContactNormal = hit.normal;
		else groundContactNormal = Vector3.up;
		rawMove = botTarget - transform.position;
		desiredMove = Vector3.ProjectOnPlane (rawMove, groundContactNormal).normalized;
		//this is where we're applying the desired move of the bot. Since it normalizes it, we can make
		//rawMove any damn thing we want, it doesn't matter
		
		if (Physics.Raycast (transform.position, Vector3.down, out hit)) {
			altitude = hit.distance;
			if (adjacentSolid > altitude)
				adjacentSolid = altitude;
		} else {
			if (Physics.Raycast (transform.position, Vector3.up, out hit)) {
				//we are doing a ricochet as best we can
				transform.position = hit.point + Vector3.up;
				rigidBody.velocity = Vector3.Reflect(rigidBody.velocity, hit.normal).normalized;
				altitude = 1;
			}
		}
		//bot's basic height off ground
		adjacentSolid *= adjacentSolid;
		adjacentSolid *= adjacentSolid;
		if (adjacentSolid < 1f) adjacentSolid = 1f;
		rigidBody.drag = 1 / adjacentSolid;
		//bot has high drag if near the ground and little in the air
		desiredMove /= adjacentSolid;
		//we're adding the move to the extent that we're near a surface

		desiredMove *= (0.5f + (0.0001f*brainR) - (0.002f*brainB));
		//scale everything back depending on the R factor
		lerpedMove = Vector3.Lerp (lerpedMove, desiredMove, 0.00001f + (0.005f*brainR));
		//texture red makes the bots go more hyper!

		rigidBody.AddForce (lerpedMove/adjacentSolid, ForceMode.Impulse);
		//apply the attempted bot move as adjusted

		if (botBrain.Length > 1) StartCoroutine ("SlowUpdates");
		//our heavier processing doesn't update quickly
		//we're testing to make sure it has a length, if not then it's a deleted bot
		//and we won't attempt to run any of that stuff
	}
	
	IEnumerator SlowUpdates () {

		step += 1;
		step += (botBrain [brainPointer].r / 85);
		//red bots are more agitated, to a point.
		if ((step > (350 - botBrain [brainPointer].g)) && (!setupbots.gameEnded)) audioSource.volume = 0.0f;
		//staccato: the bots can and do shorten their beeps. Green means perky short beeps, no green means longer

		if (transform.position.x < 1f) {
			transform.position = new Vector3 (1f, transform.position.y, transform.position.z);
			rigidBody.velocity = new Vector3 (Mathf.Abs (rigidBody.velocity.x), rigidBody.velocity.y, rigidBody.velocity.z);
		}
		yield return new WaitForSeconds (.01f);
		//walls! We bounce off the four walls of the world rather than falling out of it

		
		if (transform.position.z < 1f) {
			transform.position = new Vector3 (transform.position.x, transform.position.y, 1f);
			rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, Mathf.Abs (rigidBody.velocity.z));
		}

		if (setupbots.gameEnded) botTarget = ourhero.transform.position;
		//if we won, hooray! Everybody pile on the lucky bot! :D

		if (step >= botBrain [brainPointer].b) {
			step = 0;
			//here's where we do the bot manevuerings
			//note that step will always start at zero, so we can go above to where it's updated
			//and use it as our staccato mechanism
			//at 16386 slots, three updates a second, it will take 90 minutes to get through
			//blue is more serene!
			brainPointer += 1;
			if (brainPointer >= botBrain.Length) brainPointer = 0;
			brainR = botBrain [brainPointer].r;
			brainG = botBrain [brainPointer].g;
			brainB = botBrain [brainPointer].b;
			//we establish a new target location based on this color

			if (notEnded && withinRange) {
				if (audioSource.clip != BotBeep) audioSource.clip = BotBeep;
				audioSource.volume = 0.2f + ((botBrain [brainPointer].r + botBrain [brainPointer].g) / 1500f);
				//yellowness is noisiness, they get loud when they're yellower. Also makes the hyper ones noisier.
				//extreme loudness will make them blink!
				float voicePitch = Mathf.Abs(2.9f - ((brainR + brainG + brainB) * 0.0045f));
				//bounce the subsonic notes back up again
				audioSource.pitch = voicePitch + 0.1f;
				if (!audioSource.isPlaying) audioSource.Play ();
				//bot makes a remark
			}

			rigidBody.angularVelocity = Vector3.zero;
			transform.LookAt (transform.localPosition + new Vector3(brainR - 127f, brainG - 127f, brainB - 127f));
			Color c = new Color (brainR, brainG, brainB);
			SetUpBots.HSLColor color = SetUpBots.HSLColor.FromRGBA (c);
			//this is giving us 360 degree hue, and then saturation and luminance.
			float botDistance = 1000f - (Mathf.Abs(color.s)*500f);
			Vector3 spawnLocation = new Vector3 (2000f + (Mathf.Sin (Mathf.PI / 180f * color.h) * botDistance), 1f, 2000f + (Mathf.Cos (Mathf.PI / 180f * color.h) * botDistance));
			//place bot

			if (Physics.Raycast (spawnLocation, Vector3.up, out hit))
				botTarget = hit.point + Vector3.up;
			else
				botTarget = spawnLocation;
		}
		yield return new WaitForSeconds (.01f);

		if (transform.position.x > 3999f) {
			transform.position = new Vector3 (3999f, transform.position.y, transform.position.z);
			rigidBody.velocity = new Vector3 (-Mathf.Abs (rigidBody.velocity.x), rigidBody.velocity.y, rigidBody.velocity.z);
		}
		if (audioSource.volume > 0.5 && audioSource.isPlaying && notEnded) myColor.material.color = new Color (1.1f, 1.1f, 1.1f);
		else  myColor.material.color = new Color (0.72f, 0.72f, 0.72f);
		//bots light up when they are talking to you or banging, but not to play the end music
		yield return new WaitForSeconds (.01f);




		if (transform.position.z > 3999f) {
			transform.position = new Vector3 (transform.position.x, transform.position.y, 3999f);
			rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, -Mathf.Abs (rigidBody.velocity.z));
		}
		float distance = Vector3.Distance (transform.position, ourhero.transform.position);

		if (distance < 25) {
			meshfilter.mesh = meshLOD0;
		} else {
			if (distance < 50) {
				meshfilter.mesh = meshLOD1;
			} else {
				if (distance < 100) {
					meshfilter.mesh = meshLOD2;
				} else {
					if (distance < 200) {
						meshfilter.mesh = meshLOD3;
					} else {
						meshfilter.mesh = meshLOD4;
					}
				}
			}
		}
		//rolling my own LOD. With this, the render thread is so low in overhead that there's no point optimizing further: we're physics bound.

		if (playermovement.yourMatch == yourMatch) {
			playermovement.yourMatchDistance = Mathf.Sqrt(Vector3.Distance (transform.position, ourhero.transform.position))*32f;
			//this bot is your one true bot and we don't delete it or move it. We send the distance value to the 'ping' routine.
		} else {
			if (distance < playermovement.activityRange) {
				rigidBody.isKinematic = false;
				withinRange = true;
			} else {
				bool allowMovement = rigidBody.isKinematic;
				if (allowMovement) {
					rigidBody.isKinematic = false;
					rigidBody.velocity = storedVelocity;
				} else {
					storedVelocity = rigidBody.velocity;
					rigidBody.isKinematic = true;
				}
				//what we're doing is simply toggling it. This will slow them down but let them still move and get out of jammed situations
				withinRange = false;
				audioSource.Stop();
				//thus if we have bots far out of range they'll totally avoid making useless engine functions happen
				if (distance > (playermovement.fps * 50)) {
					Destroy(this.transform.gameObject);
					//if we are out of range AND the framerate's an issue AND we are not the lucky bot (this area is only for the disposables)
					//then mark this whole bot for destruction! This can rein in some frame rate chugs. At 10 fps it's killing bots as close as 500 away,
					//at full 60fps vSync you have to be 3000 away to be culled. And of course unsynced rapidly makes them uncullable.

				}
			}
		}

		yield return new WaitForSeconds (.01f);

		//this is also where we'll do dumb AI things that might be time consuming. We'll always return to this point but it takes a while
		//to iterate through it all
	}
	
}