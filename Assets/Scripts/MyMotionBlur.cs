using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[AddComponentMenu("Image Effects/Blur/My Motion Blur (SingleOverlay)")]
	[RequireComponent(typeof(Camera))]
	public class MyMotionBlur : ImageEffectBase
	{
		[Range(0.0f, 1.0f)]
		public float blurAmount = 0.2f;

		private RenderTexture accumTexture;
		private RenderTexture echoTexture;
		private RenderTexture echoTwoTexture;

		override protected void Start()
		{
			if (!SystemInfo.supportsRenderTextures)
			{
				enabled = false;
				return;
			}
			base.Start();
		}
		
		override protected void OnDisable()
		{
			base.OnDisable();
			DestroyImmediate(accumTexture);
			DestroyImmediate(echoTexture);
			DestroyImmediate(echoTwoTexture);
		}
		
		// Called by camera to apply image effect
		void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
			if (accumTexture == null)
			{
				DestroyImmediate(accumTexture);
				accumTexture = new RenderTexture(source.width, source.height, 0);
				accumTexture.filterMode = FilterMode.Point;
				accumTexture.hideFlags = HideFlags.HideAndDontSave;
				Graphics.Blit( source, accumTexture );
			}

			if (echoTexture == null)
			{
				DestroyImmediate(echoTexture);
				echoTexture = new RenderTexture(source.width, source.height, 0);
				echoTexture.filterMode = FilterMode.Bilinear;
				echoTexture.hideFlags = HideFlags.HideAndDontSave;
				Graphics.Blit( source, echoTexture );
				}
			
			if (echoTwoTexture == null)
			{
				DestroyImmediate(echoTwoTexture);
				echoTwoTexture = new RenderTexture(source.width, source.height, 0);
				echoTwoTexture.filterMode = FilterMode.Bilinear;
				echoTwoTexture.hideFlags = HideFlags.HideAndDontSave;
				Graphics.Blit( source, echoTwoTexture );
				}



			// Setup the texture and floating point values in the shader
			accumTexture.MarkRestoreExpected();
			echoTexture.MarkRestoreExpected();
			echoTwoTexture.MarkRestoreExpected();

			Graphics.Blit (source, accumTexture);
			//start fresh with the live image

			material.SetTexture("_MainTex", accumTexture);
			material.SetFloat("_AccumOrig", blurAmount);
			Graphics.Blit (echoTwoTexture, accumTexture, material);
			DestroyImmediate (echoTwoTexture);
			material.SetTexture("_MainTex", accumTexture);
			material.SetFloat("_AccumOrig", blurAmount);
			Graphics.Blit (echoTexture, accumTexture, material);

			Graphics.Blit(source, echoTwoTexture);
			Graphics.Blit(accumTexture, echoTwoTexture, material);
			Graphics.Blit(source, echoTexture);
			//generating the reverb effect

			Graphics.Blit (accumTexture, destination);
			DestroyImmediate (accumTexture);
			//output. Then we do the flare stuff in the accumtexture which is applied more subtly
		}
	}
}
