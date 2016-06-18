using UnityEngine;
using System;// Needed for Math

public class BackgroundSound : MonoBehaviour
{

	public float gain = 1.0f;
	public int lowcut = 35;
	public float brightness = 0.1f;
	public float whoosh = 0.0f;
	public float whooshLowCut = 0.0f;
	private float whooshIIR = 0.0f;
	private double previousRandom = 0.0;
	private System.Random RandomNumber = new System.Random ();
	private int position;
	private int quadratic;
	private int[] dcut = new int[] {
		3,
		3,
		3,
		5,
		7,
		11,
		13,
		17,
		19,
		23,
		27,
		31,
		53,
		71,
		113,
		131,
		173,
		191,
		233,
		293,
		311,
		373,
		419,
		431,
		479,
		541,
		593,
		613,
		673,
		719,
		733,
		797,
		839,
		907,
		971,
		1013,
		1031,
		1087,
		1091,
		1151
	};
	private float appliedBrightness = 0.000001f;
	private float appliedGain = 0.000001f;
	private double noise = 0.0;
	private double noiseA = 0.0;
	private double noiseB = 0.0;
	private float[] b = new float[10];
	private float[] f = new float[10];
	private float movingaverage = 0.0f;
	private float outputWhoosh = 0.0f;
	private float whooshLowCutB;
	private float temp = 0.0f;
	private Boolean flip = true;
	
	void OnAudioFilterRead (float[] data, int channels)
	{
		if (lowcut < 4)
			lowcut = 4;
		//min setting, rain
		if (lowcut > 39)
			lowcut = 39;
		if (brightness < 0.1f)
			brightness = 0.1f;
		if (brightness > 1.0f)
			brightness = 0.1f;
		if (whoosh < 0.0f)
			whoosh = 0.0f;
		if (whoosh > 0.5f)
			whoosh = 0.0f;
		if (whooshLowCut < 0.0f)
			whooshLowCut = 0.0f;
		if (whooshLowCut > 0.5f)
			whooshLowCut = 0.0f;
		if (gain < 0.0f)
			gain = 0.0f;
		if (gain > 1.0f)
			gain = 0.1f;
		//sanity checking because who knows what we'll get handed
		//note we have wrap-around values where if they're going to go insanely loud,
		//we kill it entirely.
		appliedBrightness = Mathf.Pow (brightness, 3f);
		appliedGain = Mathf.Pow (gain * 0.5f, 2f);
		//give us a better adjustment range

		whooshLowCutB = (1.0f - whooshLowCut);

		movingaverage = 1.0f - appliedBrightness;
		movingaverage *= 9.0f;
		movingaverage += 1.0f;
		temp = movingaverage;
		//looking for appliedBrightness to be 0 at full dark, 1.0 at full bright
		if (temp > 1.0f) {
			f [0] = 1.0f;
			temp -= 1.0f;
		} else {
			f [0] = temp;
			temp = 0.0f;
		}
		if (temp > 1.0f) {
			f [1] = 1.0f;
			temp -= 1.0f;
		} else {
			f [1] = temp;
			temp = 0.0f;
		}
		if (temp > 1.0f) {
			f [2] = 1.0f;
			temp -= 1.0f;
		} else {
			f [2] = temp;
			temp = 0.0f;
		}
		if (temp > 1.0f) {
			f [3] = 1.0f;
			temp -= 1.0f;
		} else {
			f [3] = temp;
			temp = 0.0f;
		}
		if (temp > 1.0f) {
			f [4] = 1.0f;
			temp -= 1.0f;
		} else {
			f [4] = temp;
			temp = 0.0f;
		}
		if (temp > 1.0f) {
			f [5] = 1.0f;
			temp -= 1.0f;
		} else {
			f [5] = temp;
			temp = 0.0f;
		}
		if (temp > 1.0f) {
			f [6] = 1.0f;
			temp -= 1.0f;
		} else {
			f [6] = temp;
			temp = 0.0f;
		}
		if (temp > 1.0f) {
			f [7] = 1.0f;
			temp -= 1.0f;
		} else {
			f [7] = temp;
			temp = 0.0f;
		}
		if (temp > 1.0f) {
			f [8] = 1.0f;
			temp -= 1.0f;
		} else {
			f [8] = temp;
			temp = 0.0f;
		}
		if (temp > 1.0f) {
			f [9] = 1.0f;
			temp -= 1.0f;
		} else {
			f [9] = temp;
			temp = 0.0f;
		}
		//there, now we have a neat little moving average with remainders
		
		if (movingaverage < 1.0f)
			movingaverage = 1.0f;
		f [0] /= movingaverage;
		f [1] /= movingaverage;
		f [2] /= movingaverage;
		f [3] /= movingaverage;
		f [4] /= movingaverage;
		f [5] /= movingaverage;
		f [6] /= movingaverage;
		f [7] /= movingaverage;
		f [8] /= movingaverage;
		f [9] /= movingaverage;
		//and now it's neatly scaled, too



		for (var i = 0; i < data.Length; i = i + channels) {

			flip = !flip;

			quadratic -= 1;
			if (quadratic < 0) {
				position = position + 1;
				quadratic = position * position;
				quadratic = quadratic % 170003;
				quadratic *= quadratic;
				quadratic = quadratic % 17011;
				quadratic *= quadratic;
				quadratic = quadratic % dcut [lowcut];
				quadratic *= quadratic;
				quadratic = quadratic % lowcut;
				//sets density of the centering force
				if (noiseA < 0) {
					flip = true;
				} else {
					flip = false;
				}
			}

			noise = RandomNumber.NextDouble ();

			previousRandom *= (0.5 + whooshLowCut);
			previousRandom += (noise - 0.5);
			whooshIIR = (whooshIIR * whooshLowCutB) + ((float)previousRandom * whooshLowCut);
			outputWhoosh = (float)previousRandom - whooshIIR;

			//now previousRandom is our cyberbike speed whoosh

			noise *= appliedGain;


			if (noise < 0.0)
				noise = -noise;

			if (flip) {
				noiseA += noise;
			} else {
				noiseA -= noise;
			}

			if (noiseA < -0.9) noiseA = -0.9;
			if (noiseA > 0.9) noiseA = 0.9;

			noiseB *= (1.0f - appliedBrightness);
			noiseB += (noiseA * appliedBrightness);


			b [9] = b [8];
			b [8] = b [7];
			b [7] = b [6];
			b [6] = b [5];
			b [5] = b [4];
			b [4] = b [3];
			b [3] = b [2];
			b [2] = b [1];
			b [1] = b [0];
			b [0] = (float)noiseB;

			noiseB *= f [0];
			noiseB += (b [1] * f [1]);
			noiseB += (b [2] * f [2]);
			noiseB += (b [3] * f [3]);
			noiseB += (b [4] * f [4]);
			noiseB += (b [5] * f [5]);
			noiseB += (b [6] * f [6]);
			noiseB += (b [7] * f [7]);
			noiseB += (b [8] * f [8]);
			noiseB += (b [9] * f [9]);
			//apply the moving average to darken this

			noiseB += (outputWhoosh * whoosh);


			if (data[i] > 0) data [i] = Mathf.Sin((data[i]*4) + (float)noiseB);
			else data [i] = -Mathf.Sin(-data[i] - (float)noiseB);

			if (channels == 2){
				if (data[i+1] > 0) data [i+1] = Mathf.Sin((data[i+1]*4) + (float)noiseB);
				else data [i+1] = -Mathf.Sin(-data[i+1] - (float)noiseB);
			}
		}
	}
} 

/*
 * To hear this you’ll need to drag it onto an object in the hierarchy, such as the camera.
 * Be sure there's an audio listener in the scene!
 * 
 * Here is an example of it built into a player controller, to vary based on height above ground and general altitude.
 * Trim here is just to smooth sudden volume bursts with the Lerp in the main routine.
 * If BackgroundSound is part of the same object whose script is talking to it, you can declare it as private and
 * refer to it directly-

		private BackgroundSound backgroundSound;
		private float trim;

  * and not drag stuff from the hierarchy to make it work. If BackgroundSound is somewhere else, you'd declare it
 * public like this and drag the object that contains its script to this place in the Inspector.

		public BackgroundSound backgroundSound;

 * This part below went in a player controller's Update, as the controller was using Update for its stuff.
 * The specifics can be adjusted as you see fit: here we are making brightness be an adjusted factor of
 * how high overall we are, and the gain comes from a raycast straight down. The height won't change abruptly
 * but the raycast might, so we lerp it. Use 0.01f to really slow down volume changes, or use
 * 			trim = hit.distance;
 * to make sharp transitions in sound. You can lerp the brightness as well, if you're using that.
 * This generator also does spaceship rumbles, rain, gunshots and explosions if you work it right!
 * You can simply retrigger the sound or increment it further to show rapid overlapping shots or explosions:
 * if brightness is being punched up and then lerping down, a successive shot might add to what's there rather
 * than just going to the 'trigger' level and heading down.
 * 
			backgroundSound = GetComponent ("BackgroundSound") as BackgroundSound;
			backgroundSound.brightness = (rigidbodyFirstPersonController.transform.position.y / 900.0) + 0.2;
			RaycastHit hit;
			if (Physics.Raycast(transform.position, -Vector3.up, out hit)) {
				trim = Mathf.Lerp(trim, hit.distance, 0.1f);
				if (trim > 400) trim = 400;
				trim = Mathf.Sqrt(trim+12.0f);
				backgroundSound.gain = 1.0 / trim;
			}

*/

