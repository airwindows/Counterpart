using UnityEngine;
using System.Collections;

public class SetUpBots : MonoBehaviour {
	
	public GameObject botPrefab;
	public GameObject botParent;
	public GameObject baseTerrain;
	private GameObject ourhero;
	private PlayerMovement playermovement;
	public Texture2D[] botTexture;
	public bool gameEnded = true;
	public bool killed = true;
	public int yourMatch;
	private float distance;
	private float spacing;
	private float degrees;
	private Vector3 spawnLocationA;
	private Vector3 spawnLocationB;
	private Vector3 spawnLocationC;


	//Texture array must be set up in the editor as LoadAll no worky
	//to do this, lock the inspector and then select all textures and drag them onto the array
	//this is also a single script that remains attached to the level

	void Awake () {
		//preserve setup state between levels
		botParent = GameObject.FindGameObjectWithTag ("AllBots");
		ourhero = GameObject.FindGameObjectWithTag ("Player");
		playermovement = ourhero.GetComponent<PlayerMovement>();
	}
	
	void Start () {
		RaycastHit hit;
		gameEnded = false;
		killed = false;
		baseTerrain.GetComponent<Terrain> ().terrainData.size = new Vector3 (4000f, (float)PlayerMovement.levelNumber*2f, 4000f);
		//must be after Awake so we can get the level number in Player

		PlayerMovement.playerPosition = new Vector3 (PlayerMovement.playerPosition.x, 99999f, PlayerMovement.playerPosition.z);
		if (Physics.Raycast (PlayerMovement.playerPosition, Vector3.down, out hit)) PlayerMovement.playerPosition = hit.point + Vector3.up;
		playermovement.transform.position = PlayerMovement.playerPosition;
		//we only need to access position, because position might need to be updated.
		//rotation and the mouse look offsets are static but fine where they are.

		int randBots = botTexture.Length;
		if (randBots > PlayerMovement.levelNumber) randBots = PlayerMovement.levelNumber;
		//for the setup, we will grind through only as many bots as fit within the level number
		//so if we're on level 2, you're always one of the first 2, though additional ones could be anything
		//if they're spawned through the extra-bot-spawn mechanics

		playermovement.totalBotNumber = PlayerMovement.levelNumber;
		playermovement.creepToRange = Mathf.Min (1800, PlayerMovement.levelNumber);
		//we'll have the bot range clamped

		yourMatch = Random.Range(0, randBots);
		playermovement.yourMatch = yourMatch;
		playermovement.yourBrain = botTexture [yourMatch].GetPixels32 ();

		Texture2D ourTexture = botTexture [yourMatch];
		playermovement.ourbody.material.mainTexture = ourTexture;
		playermovement.ourbody.material.color = new Color (0.5f, 0.5f, 0.5f);
		//we have now established that you are a particular bot

		SpawnBot (yourMatch, true);
		//and we spawn your counterpart.

		//Now, anything else is random
		
		for (int i = 0; i < randBots; i++) {
			if (i != yourMatch) {
				SpawnBot (i, false);
			}
		}
		//we generate one of every bot, to start off with, skipping the counterpart that we've just explicitly made.
		//The rest will be generated on the fly by the program, until it hits framerate and can't maintain 60fps.
	}
	
	public void SpawnBot(int index, bool onEdge)
	{    
		GameObject myBot;
		GameObject myDolly;
		RaycastHit hit;

		if (index == -1) {
			index = Random.Range(0, botTexture.Length);
			if (index == yourMatch) return;
		}
		//if we're passed -1 by the respawn mechanic in PlayerMovement,
		//we switch to a random index and if it's your match, return without creating anything

		myBot = botPrefab;
		myBot.name = "Bot";
		myBot = Instantiate (botPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		myDolly = myBot.transform.FindChild("Dolly").gameObject;

		Texture2D texture = botTexture[index];
		
		Renderer renderer = myDolly.GetComponent<Renderer>();
		renderer.material.mainTexture = texture;

		BotMovement botmovement = myBot.GetComponent<BotMovement>();
		botmovement.botBrain = texture.GetPixels32();
		//now we preload the instance's script with the 'brain' we want
		botmovement.yourMatch = index;
		//and the bots must know what number they are, because you'll match with them

		int step = Random.Range(0,botmovement.botBrain.Length); //let's try having them all very scattered, unless they begin to meet

		spacing = Random.Range (playermovement.creepToRange / 3f, playermovement.creepToRange);
		degrees = Random.Range (0f, 360f);
		spawnLocationA = new Vector3 (1614f + (Mathf.Sin (Mathf.PI / 180f * degrees) * spacing), 99999f, 2083f + (Mathf.Cos (Mathf.PI / 180f * degrees) * spacing));
		spacing = Random.Range (playermovement.creepToRange / 2f, playermovement.creepToRange);
		degrees = Random.Range (0f, 360f);
		spawnLocationB = new Vector3 (1614f + (Mathf.Sin (Mathf.PI / 180f * degrees) * spacing), 99999f, 2083f + (Mathf.Cos (Mathf.PI / 180f * degrees) * spacing));
		spacing = Random.Range (1f, playermovement.creepToRange);
		degrees = Random.Range (0f, 360f);
		spawnLocationC = new Vector3 (1614f + (Mathf.Sin (Mathf.PI / 180f * degrees) * spacing), 99999f, 2083f + (Mathf.Cos (Mathf.PI / 180f * degrees) * spacing));
		//three different locations, increasingly weighted towards the farthest reaches of creepToRange, mean that bots will tend to be scattered but
		//can still turn up very local to the player

		if (Vector3.Distance(ourhero.transform.position, spawnLocationB) > Vector3.Distance(ourhero.transform.position, spawnLocationA)) spawnLocationA = spawnLocationB;
	    if (Vector3.Distance(ourhero.transform.position, spawnLocationC) > Vector3.Distance(ourhero.transform.position, spawnLocationA)) spawnLocationA = spawnLocationC;

		if (Physics.Raycast (spawnLocationA, Vector3.down, out hit)) {
			spawnLocationA = hit.point + Vector3.up;
			//if there's a terrain and we're not too far, spawn bot
		}
		else {
			if (index != yourMatch) Destroy (myBot.transform.gameObject);
			else spawnLocationA = hit.point + Vector3.up;
			//exception: if it's the counterpart, spawn anyway, it won't be culled
		}
		myBot.transform.position = spawnLocationA;
		botmovement.botTarget = spawnLocationC;
		botmovement.step = step;
		botmovement.brainPointer = Random.Range(0, botmovement.botBrain.Length);
		botmovement.jumpCounter = botmovement.brainPointer;
		botmovement.withinRange = true;
		myBot.transform.SetParent (botParent.transform);
	}

	
	public struct HSLColor {
		public float h; public float s; public float l; public float a;
		
		
		public HSLColor(float h, float s, float l, float a) {
			this.h = h; this.s = s; this.l = l; this.a = a; }
		
		public HSLColor(float h, float s, float l) {
			this.h = h; this.s = s; this.l = l; this.a = 1f; }
		
		public HSLColor(Color c) {
			HSLColor temp = FromRGBA(c);
			h = temp.h; s = temp.s; l = temp.l; a = temp.a; }
		
		public static HSLColor FromRGBA(Color c) {		
			float h, s, l, a; a = c.a;
			
			float cmin = Mathf.Min(Mathf.Min(c.r, c.g), c.b);
			float cmax = Mathf.Max(Mathf.Max(c.r, c.g), c.b);
			
			l = (cmin + cmax) / 2f;
			
			if (cmin == cmax) {
				s = 0;
				h = 0;
			} else {
				float delta = cmax - cmin;
				
				s = (l <= .5f) ? (delta / (cmax + cmin)) : (delta / (2f - (cmax + cmin)));
				
				h = 0;
				
				if (c.r == cmax) {
					h = (c.g - c.b) / delta;
				} else if (c.g == cmax) {
					h = 2f + (c.b - c.r) / delta;
				} else if (c.b == cmax) {
					h = 4f + (c.r - c.g) / delta;
				}
				
				h = Mathf.Repeat(h * 60f, 360f);
			}
			
			return new HSLColor(h, s, l, a);
		}
		
		
		public Color ToRGBA() {
			float r, g, b, a;
			a = this.a;
			
			float m1, m2;
			
			m2 = (l <= .5f) ? (l * (1f + s)) : (l + s - l * s);
			m1 = 2f * l - m2;
			
			if (s == 0f) {
				r = g = b = l;
			} else {
				r = Value(m1, m2, h + 120f);
				g = Value(m1, m2, h);
				b = Value(m1, m2, h - 120f);
			}
			
			return new Color(r, g, b, a);
		}
		
		static float Value(float n1, float n2, float hue) {
			hue = Mathf.Repeat(hue, 360f);
			
			if (hue < 60f) {
				return n1 + (n2 - n1) * hue / 60f;
			} else if (hue < 180f) {
				return n2;
			} else if (hue < 240f) {
				return n1 + (n2 - n1) * (240f - hue) / 60f;
			} else {
				return n1;
			}
		}
		
		public static implicit operator HSLColor(Color src) {
			return FromRGBA(src);
		}
		
		public static implicit operator Color(HSLColor src) {
			return src.ToRGBA();
		}
		
	}

	public static Color HSVToRGB(float H, float S, float V)
	{
		Color white = Color.white;
		if (S == 0f)
		{
			white.r = V;
			white.g = V;
			white.b = V;
		}
		else if (V == 0f)
		{
			white.r = 0f;
			white.g = 0f;
			white.b = 0f;
		}
		else
		{
			white.r = 0f;
			white.g = 0f;
			white.b = 0f;
			float num = H * 6f;
			int num2 = (int)Mathf.Floor(num);
			float num3 = num - (float)num2;
			float num4 = V * (1f - S);
			float num5 = V * (1f - S * num3);
			float num6 = V * (1f - S * (1f - num3));
			int num7 = num2;
			switch (num7 + 1)
			{
			case 0:
				white.r = V;
				white.g = num4;
				white.b = num5;
				break;
			case 1:
				white.r = V;
				white.g = num6;
				white.b = num4;
				break;
			case 2:
				white.r = num5;
				white.g = V;
				white.b = num4;
				break;
			case 3:
				white.r = num4;
				white.g = V;
				white.b = num6;
				break;
			case 4:
				white.r = num4;
				white.g = num5;
				white.b = V;
				break;
			case 5:
				white.r = num6;
				white.g = num4;
				white.b = V;
				break;
			case 6:
				white.r = V;
				white.g = num4;
				white.b = num5;
				break;
			case 7:
				white.r = V;
				white.g = num6;
				white.b = num4;
				break;
			}
			white.r = Mathf.Clamp(white.r, 0f, 1f);
			white.g = Mathf.Clamp(white.g, 0f, 1f);
			white.b = Mathf.Clamp(white.b, 0f, 1f);
		}
		return white;
	}
}