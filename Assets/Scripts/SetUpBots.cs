﻿using UnityEngine;
using System.Collections;

public class SetUpBots : MonoBehaviour {
	
	public GameObject botPrefab;
	public GameObject botParent;
	private GameObject ourhero;
	private PlayerMovement playermovement;
	public Texture2D[] botTexture;
	public bool gameEnded = true;
	public bool killed = true;
	public int yourMatch;


	//must be set up in the editor as LoadAll no worky
	//to do this, lock the inspector and then select all textures and drag them onto the array
	//this is also a single script that remains attached to the level
	
	void Start () {

		gameEnded = false;
		killed = false;
		//we've only just begun!
		botParent = GameObject.FindGameObjectWithTag ("AllBots");

		yourMatch = Random.Range(0, botTexture.Length);
		ourhero = GameObject.FindGameObjectWithTag ("Player");
		playermovement = ourhero.GetComponent<PlayerMovement>();
		playermovement.yourMatch = yourMatch;
		playermovement.yourBrain = botTexture [yourMatch].GetPixels32 ();
		//we have now established that you are a particular bot

		//Component[] renderers = ourhero.GetComponentsInChildren(typeof(Renderer));
		//foreach(Renderer renderer in renderers) renderer.material.mainTexture = botTexture[yourMatch];
		//this will texture everything, including particles, as you
		
		
		for (int i = 0; i < botTexture.Length; i++) {
			if (i != yourMatch) {
				SpawnBot (i, false);
			}
			else SpawnBot (i, true);
		}
		//we generate one of every bot, to start off with
		//The rest will be generated on the fly by the program, until it hits framerate and can't maintain 60fps.
	}
	
	public void SpawnBot(int index, bool onEdge)
	{    
		GameObject myBot;
		RaycastHit hit;

		if (index == -1) {
			index = Random.Range(0, botTexture.Length);
			if (index == yourMatch) return;
		} //we switch to a random index and if it's your match, return without creating anything

		myBot = botPrefab;
		myBot.name = "Bot";
		myBot = Instantiate (botPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		Texture2D texture = botTexture[index];
		
		Renderer renderer = myBot.GetComponent<Renderer>();
		renderer.material.mainTexture = texture;

		BotMovement botmovement = myBot.GetComponent<BotMovement>();
		botmovement.botBrain = texture.GetPixels32();
		//now we preload the instance's script with the 'brain' we want
		botmovement.yourMatch = index;
		//and the bots must know what number they are, because you'll match with them

		int step = Random.Range(0,botmovement.botBrain.Length); //let's try having them all very scattered, unless they begin to meet
		Color c = new Color(botmovement.botBrain[step].r, botmovement.botBrain[step].g, botmovement.botBrain[step].b);
		HSLColor color = HSLColor.FromRGBA(c); //this is giving us 360 degree hue, and then saturation and luminance.

		float botDistance = (Mathf.Abs(color.s)+1f) * playermovement.creepToRange;
		if (onEdge) botDistance = 1800f;
		float adjustedHueAngle = color.h + playermovement.creepRotAngle;
		Vector3 spawnLocation = new Vector3 (1580f + (Mathf.Sin (Mathf.PI / 180f * adjustedHueAngle) * botDistance), 1f, 2190f + (Mathf.Cos (Mathf.PI / 180f * adjustedHueAngle) * botDistance));

		if (Physics.Raycast (spawnLocation, Vector3.up, out hit)) spawnLocation = hit.point + Vector3.up;
		myBot.transform.position = spawnLocation;


		if (botmovement.yourMatch == playermovement.yourMatch ) {
			myBot.GetComponent<Rigidbody>().isKinematic = false;
			//if it's your match, they can move even at great distances
		} else {
			myBot.GetComponent<Rigidbody>().isKinematic = false;
			//everything else starts out with the physics engine not running yet! the LOD also disables the bots.
		}

		botmovement.botTarget = spawnLocation;
		botmovement.step = 1;
		botmovement.brainPointer = Random.Range(0, botmovement.botBrain.Length);
		botmovement.withinRange = true;
		//we randomize the step so the bot pairs aren't synced to start with
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
			
			//	Note: there is a typo in the 2nd International Edition of Foley and
			//	van Dam's "Computer Graphics: Principles and Practice", section 13.3.5
			//	(The HLS Color Model). This incorrectly replaces the 1f in the following
			//	line with "l", giving confusing results.
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