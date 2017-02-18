using UnityEngine;
using UnityEngine.UI;
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
	private GameObject logo;
	private GameObject talk;
	private GameObject highscore;
	//Texture array must be set up in the editor as LoadAll no worky
	//to do this, lock the inspector and then select all textures and drag them onto the array
	//this is also a single script that remains attached to the level

	void Awake () {
		//preserve setup state between levels
		botParent = GameObject.FindGameObjectWithTag ("AllBots");
		ourhero = GameObject.FindGameObjectWithTag ("Player");
		playermovement = ourhero.GetComponent<PlayerMovement>();
		logo = GameObject.FindGameObjectWithTag ("counterpartlogo");
		talk = GameObject.FindGameObjectWithTag ("instructionScreen");
		highscore = GameObject.FindGameObjectWithTag ("highscore");
	}
	
	void Start () {
		RaycastHit hit;
		gameEnded = false;
		killed = false;

		baseTerrain.GetComponent<Terrain> ().terrainData.size = new Vector3 (1000f, playermovement.terrainHeight, 1000f);
		//must be after Awake so we can get the level number in Player



		PlayerMovement.playerPosition = new Vector3 (PlayerMovement.playerPosition.x, 4999f, PlayerMovement.playerPosition.z);
		if (Physics.Raycast (PlayerMovement.playerPosition, Vector3.down, out hit)) PlayerMovement.playerPosition = hit.point + Vector3.up;
		playermovement.transform.position = PlayerMovement.playerPosition;
		//we only need to access position, because position might need to be updated.
		//rotation and the mouse look offsets are static but fine where they are.

		int randBots = botTexture.Length;
		if (randBots > playermovement.totalBotNumber) randBots = playermovement.totalBotNumber;
		//for the setup, we will grind through only as many bots as fit within the bot number
		//so if we're on level 2, you're always one of the first 2, though additional ones could be anything
		//if they're spawned through the extra-bot-spawn mechanics

		yourMatch = Random.Range(0, botTexture.Length);
		playermovement.yourMatch = yourMatch;
		playermovement.yourBrain = botTexture [yourMatch].GetPixels32 ();
		logo.GetComponent<Text>().text = string.Format ("Level {0:0.}: Catch ", PlayerMovement.levelNumber) + botTexture[yourMatch].ToString().Substring(0, botTexture[yourMatch].ToString().Length - 24);
		highscore.GetComponent<Text>().text = string.Format ("High Score: {0:0.}", PlayerMovement.maxlevelNumber);

		//And now we put in the chatty little asides: scroll down to get past them
		switch (PlayerMovement.levelNumber) {
		case 1: talk.GetComponent<Text> ().text = "Touch the bot you seek, your counterpart, to win the level."; break;
		case 2: talk.GetComponent<Text> ().text = "Fire with the mouse to talk to other bots and be directed."; break;
		case 3: talk.GetComponent<Text> ().text = "If they haven't seen your counterpart they won't know where it is..."; break;
		case 4: talk.GetComponent<Text> ().text = "Don't smash bots or the Guardian will come!"; break;
		case 5: talk.GetComponent<Text> ().text = "Their chatter has meaning: high beeps mean it's similar to you."; break;
		case 6: talk.GetComponent<Text> ().text = "If they are very dissimilar they may not direct you..."; break;
		case 7: talk.GetComponent<Text> ().text = "Many-colored bots wander farther, solid ones stay put."; break;
		case 8: talk.GetComponent<Text> ().text = "Wandering bots may have seen your counterpart when others have not."; break;
		case 9: talk.GetComponent<Text> ().text = "Charge of the bot brigade!"; break;
		case 10: talk.GetComponent<Text> ().text = "Bots will roam far afield."; break;
		case 11: talk.GetComponent<Text> ().text = "They'll follow paths driven by the colors on them."; break;
		case 12: talk.GetComponent<Text> ().text = "So you can see how a bot will act by looking at it"; break;
		case 13: talk.GetComponent<Text> ().text = "Pro tip, question bots that have jumped high!"; break;
		case 14: talk.GetComponent<Text> ().text = "Have you fired at your Guardian today?"; break;
		case 15: talk.GetComponent<Text> ().text = "Also, I didn't tell you about the terrain..."; break;
		case 16: talk.GetComponent<Text> ().text = "Just in case this wasn't confusing enough..."; break;
		case 17: talk.GetComponent<Text> ().text = "Things should be getting hectic by now."; break;
		case 18: talk.GetComponent<Text> ().text = "For a while it gets more chaotic..."; break;
		case 19: talk.GetComponent<Text> ().text = "And MORE chaotic..."; break;
		case 20: talk.GetComponent<Text> ().text = "And sometimes you're just searching for a lone bot somewhere."; break;
		case 21: talk.GetComponent<Text> ().text = "These levels are consistent, by the way..."; break;
		case 22: talk.GetComponent<Text> ().text = "For instance, in this one, 22, the bots go out into the hills."; break;
		case 23: talk.GetComponent<Text> ().text = "This will become important."; break;
		case 24: talk.GetComponent<Text> ().text = "Some levels are notoriously bad."; break;
		case 25: talk.GetComponent<Text> ().text = "You'll see! Check out 'studio 54' and boogie down!"; break;
		case 26: talk.GetComponent<Text> ().text = "Beware smashing bots in a scrum level like this."; break;
		case 27: talk.GetComponent<Text> ().text = "And watch out if you're speeding!"; break;
		case 28: talk.GetComponent<Text> ().text = "Because the Guardian doesn't like crashes."; break;
		case 29: talk.GetComponent<Text> ().text = "Oh, did I mention the Guardian?"; break;
		case 30: talk.GetComponent<Text> ().text = "Might be your biggest problem on advanced levels."; break;
		case 31: talk.GetComponent<Text> ().text = "With touchy Guardians, bots are less friendly."; break;
		case 32: talk.GetComponent<Text> ().text = "For instance, on this level it's a little more touchy."; break;
		case 33: talk.GetComponent<Text> ().text = "This level, it's calmer."; break;
		case 34: talk.GetComponent<Text> ().text = "And on this level it's SUPER touchy, watch out!"; break;
		case 35: talk.GetComponent<Text> ().text = "Guardian doesn't care if a bot ran YOU over."; break;
		case 36: talk.GetComponent<Text> ().text = "It will blame you if there's too much crashing."; break;
		case 37: talk.GetComponent<Text> ().text = "Sparser levels like this can be calmer."; break;
		case 38: talk.GetComponent<Text> ().text = "Remember the bot must have seen your counterpart to direct you!"; break;
		case 39: talk.GetComponent<Text> ().text = "This is why finding travellers matters."; break;
		case 40: talk.GetComponent<Text> ().text = "In terrain like this, bots can get stuck and lost."; break;
		case 41: talk.GetComponent<Text> ().text = "But you can also hide from the Guardian easier."; break;
		case 42: talk.GetComponent<Text> ().text = "Super-populated levels can be a challenge too!"; break;
		case 43: talk.GetComponent<Text> ().text = "Bots say where they last saw your Counterpart."; break;
		case 44: talk.GetComponent<Text> ().text = "You might have to do some detective work!"; break;
		case 45: talk.GetComponent<Text> ().text = "Then sometimes it's more of an arcade scrum."; break;
		case 46: talk.GetComponent<Text> ().text = "If the Guardian's mad, your counterpart might try and go to you when pinged."; break;
		case 47: talk.GetComponent<Text> ().text = "It also pings at you to let you know it's there."; break;
		case 48: talk.GetComponent<Text> ().text = "Helps to compensate for a touchy Guardian."; break;
		case 49: talk.GetComponent<Text> ().text = "Terrain blocks line of sight for bots, but is cover."; break;
		case 50: talk.GetComponent<Text> ().text = "You'll learn this terrain, only the height changes."; break;
		case 51: talk.GetComponent<Text> ().text = "Explore it here before catching this bot."; break;
		case 52: talk.GetComponent<Text> ().text = "Tall or flat, the geometry's the same."; break;
		case 53: talk.GetComponent<Text> ().text = "So, are you ready for a test? Win this level."; break;
		case 54: talk.GetComponent<Text> ().text = "OH COME ON"; break;
		case 55: talk.GetComponent<Text> ().text = "If you survived that, you know how to jump."; break;
		case 56: talk.GetComponent<Text> ().text = "This might be a little more chill for you..."; break;
		case 57: talk.GetComponent<Text> ().text = "Not all the levels are terrifying."; break;
		case 58: talk.GetComponent<Text> ().text = "If you can survive 67, you have all the skills to proceed."; break;
		case 59: talk.GetComponent<Text> ().text = "Just be careful of the Guardian! This one's mean."; break;
		case 60: talk.GetComponent<Text> ().text = "Use your Geiger bot-tracker to find this guy."; break;
		case 61: talk.GetComponent<Text> ().text = "Get used to whooshing around!"; break;
		case 62: talk.GetComponent<Text> ().text = "You can go fastest by pointing at the horizon."; break;
		case 63: talk.GetComponent<Text> ().text = "However, speed risks smashing bots!"; break;
		case 64: talk.GetComponent<Text> ().text = "And sometimes the Guardian is just touchy that day..."; break;
		case 65: talk.GetComponent<Text> ().text = "Quit in the 'win screen' to return to the level you were at."; break;
		case 66: talk.GetComponent<Text> ().text = "For instance, you might want to quit here and rest up..."; break;
		case 67: talk.GetComponent<Text> ().text = "Return of OH COME ON"; break;
		case 68: talk.GetComponent<Text> ().text = "Congrats! If you've reached this level, you can go the distance!"; break;
		case 69: talk.GetComponent<Text> ().text = "Savor your expertise: now your job is discovery."; break;
		case 70: talk.GetComponent<Text> ().text = "Take a breather on this level. Happy botting!"; break;
		case 76: talk.GetComponent<Text> ().text = "So you think you're big time?"; break;
		case 77: talk.GetComponent<Text> ().text = "Calm before the storm"; break;
		case 78: talk.GetComponent<Text> ().text = "Return of the son of OH COME ON"; break;
		case 80: talk.GetComponent<Text> ().text = "Remain vigilant"; break;
		case 82: talk.GetComponent<Text> ().text = "Revenge of OH COME ON"; break;
		case 90: talk.GetComponent<Text> ().text = "This is an easy one, you've earned it"; break;
		case 92: talk.GetComponent<Text> ().text = "Hunt the Hinterlands"; break;
		case 96: talk.GetComponent<Text> ().text = "Scattered"; break;
		case 97: talk.GetComponent<Text> ().text = "Centered"; break;
		case 98: talk.GetComponent<Text> ().text = "Days of our OH COME ON"; break;
		case 99: talk.GetComponent<Text> ().text = "Have another decompress level, on me"; break;
		case 100: talk.GetComponent<Text> ().text = "From now on, I mostly comment on the insane levels"; break;
		case 103: talk.GetComponent<Text> ().text = "One Life To OH COME ON"; break;
		case 108: talk.GetComponent<Text> ().text = "Got bots?"; break;
		case 110: talk.GetComponent<Text> ().text = "OH COME ON: The Search Party"; break;
		case 116: talk.GetComponent<Text> ().text = "Now this is a BOT MOB"; break;
		case 120: talk.GetComponent<Text> ().text = "Calm"; break;
		case 128: talk.GetComponent<Text> ().text = "Don't get used to it"; break;
		case 130: talk.GetComponent<Text> ().text = "Time for a big heaping plate of OH COME ON"; break;
		case 131: talk.GetComponent<Text> ().text = "OH COME ON, AGAIN?"; break;
		case 132: talk.GetComponent<Text> ().text = "Blessed Reprieve"; break;
		case 134: talk.GetComponent<Text> ().text = "OH COME ON, feel the noize"; break;
		case 147: talk.GetComponent<Text> ().text = "You might want to take a rest"; break;
		case 148: talk.GetComponent<Text> ().text = "Tales of Topographic OH COME ON"; break;
		case 150: talk.GetComponent<Text> ().text = "THE LAST ONE"; break;
		case 151: talk.GetComponent<Text> ().text = "lol nope. Would you believe 500?"; break;
		case 160: talk.GetComponent<Text> ().text = "Vacation"; break;
		case 167: talk.GetComponent<Text> ().text = "Hope you like BOTS! Lots of bots!"; break;
		case 180: talk.GetComponent<Text> ().text = "How about a reprieve?"; break;
		case 190: talk.GetComponent<Text> ().text = "Now you're just OH COME ON that I used to know"; break;
		case 192: talk.GetComponent<Text> ().text = "Let's play Center Scrum!"; break;
		case 193: talk.GetComponent<Text> ().text = "Black Friday"; break;
		case 196: talk.GetComponent<Text> ().text = "OH COME ON, can we have something a little flatter?"; break;
		case 197: talk.GetComponent<Text> ().text = "Well thank you for that"; break;
		case 199: talk.GetComponent<Text> ().text = "Where's Waldo?"; break;
		case 200: talk.GetComponent<Text> ().text = "There he is!"; break;
		case 201: talk.GetComponent<Text> ().text = "Enjoy it while it lasts"; break;
		case 204: talk.GetComponent<Text> ().text = "Okay, it's just a tall level"; break;
		case 208: talk.GetComponent<Text> ().text = "...and the joke wore thin hundreds of levels ago..."; break;
		case 209: talk.GetComponent<Text> ().text = "...so let's just have a conversation held only in the..."; break;
		case 222: talk.GetComponent<Text> ().text = "...levels formerly known as oh come on..."; break;
		case 226: talk.GetComponent<Text> ().text = "...because you're either barking mad or..."; break;
		case 227: talk.GetComponent<Text> ().text = "...really starved for easter eggs, which means I should..."; break;
		case 239: talk.GetComponent<Text> ().text = "...come up with stuff to appease your frightening tenacity..."; break;
		case 246: talk.GetComponent<Text> ().text = "...and reward you for your efforts. (cont)"; break;
		case 278: talk.GetComponent<Text> ().text = "...did I mention your tenacity? :D"; break;
		case 290: talk.GetComponent<Text> ().text = "...Honestly, you're as singular as this bot here..."; break;
		case 311: talk.GetComponent<Text> ().text = "...We meet among these merciless mountains and their beeping minons..."; break;
		case 322: talk.GetComponent<Text> ().text = "...and share our weird little connection. And in so doing..."; break;
		case 326: talk.GetComponent<Text> ().text = "...something is created."; break;
		case 335: talk.GetComponent<Text> ().text = "Admittedly not much of a something, but as FZ says..."; break;
		case 346: talk.GetComponent<Text> ().text = "'What the fuck'. :)"; break;
		case 352: talk.GetComponent<Text> ().text = "He also said 'this is a stupid song'."; break;
		case 353: talk.GetComponent<Text> ().text = "AND THAT'S THE WAY I LIKE IT"; break;
		case 371: talk.GetComponent<Text> ().text = "A little green rosetta..."; break;
		case 373: talk.GetComponent<Text> ().text = "A little green rosetta..."; break;
		case 383: talk.GetComponent<Text> ().text = "Bet you thought I was going to say something about muffins ;)"; break;
		case 385: talk.GetComponent<Text> ().text = "Didn't you?"; break;
		case 391: talk.GetComponent<Text> ().text = "By the way, this approach to player motivation was inspired by..."; break;
		case 397: talk.GetComponent<Text> ().text = "Q W"; break;
		case 398: talk.GetComponent<Text> ().text = "O"; break;
		case 424: talk.GetComponent<Text> ().text = "P! (I thought we'd never get to that last letter)"; break;
		case 427: talk.GetComponent<Text> ().text = "By the way, have I admired your tenacity lately?"; break;
		case 436: talk.GetComponent<Text> ().text = "It's really the most marvellous tenacity. You are to be"; break;
		case 447: talk.GetComponent<Text> ().text = "pitied. I mean, applauded!"; break;
		case 456: talk.GetComponent<Text> ().text = "Or perhaps remonstrated with?"; break;
		case 460: talk.GetComponent<Text> ().text = "You do realize I have to stop somewhere, yes?"; break;
		case 467: talk.GetComponent<Text> ().text = "I've only got levels up to 500."; break;
		case 490: talk.GetComponent<Text> ().text = "After that, I'll have to ask you to leave."; break;
		case 491: talk.GetComponent<Text> ().text = "Seriously, you've no business surviving this long."; break;
		case 501: talk.GetComponent<Text> ().text = "Okay, fine! This LAST ONE. Then it's a Viking death for you!"; break;
		}

		//now we can do the actual work, chat is done
		Texture2D ourTexture = botTexture [yourMatch];
		playermovement.ourbody.material.mainTexture = ourTexture;
		playermovement.ourbody.material.color = new Color (0.5f, 0.5f, 0.5f);
		//we have now established that you are a particular bot

		SpawnBot (yourMatch);
		//and we spawn your counterpart.

		//Now, anything else is random
		
		for (int i = 1; i < randBots; i++) {
			if (i != yourMatch) {
				SpawnBot (i);
			}
		}
		//we generate one of every bot, to start off with, skipping the counterpart that we've just explicitly made.
		playermovement.botNumber = botParent.transform.childCount;

		//The rest will be generated on the fly by the program, until it hits framerate and can't maintain 60fps.
	}
	
	public void SpawnBot(int index)
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

		int step = Random.Range(0,botmovement.botBrain.Length); 

		int brainR = botmovement.botBrain [step].r;
		int brainG = botmovement.botBrain [step].g;
		int brainB = botmovement.botBrain [step].b;

		Color c = new Color (brainR, brainG, brainB);
		HSLColor color = HSLColor.FromRGBA (c);
		//this is giving us 360 degree hue, and then saturation and luminance.
		float botDistance = Mathf.Abs (1f - color.s) * playermovement.startAtRange;
		if (botDistance > 400f) botDistance = 400f;
		float adjustedHueAngle = color.h;
		Vector3 spawnLocation = new Vector3 (403f + (Mathf.Sin (Mathf.PI / 180f * adjustedHueAngle) * botDistance), 4999f, 521f + (Mathf.Cos (Mathf.PI / 180f * adjustedHueAngle) * botDistance));
		//aim bot at target

		if (Physics.Raycast (spawnLocation, Vector3.down, out hit, 99999f))
			spawnLocation = hit.point + Vector3.up;

		myBot.transform.position = spawnLocation;
		botmovement.botTarget = spawnLocation;
		botmovement.step = step;
		botmovement.brainPointer = step;
		botmovement.jumpCounter = step;
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