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
		}
		
		// Called by camera to apply image effect
		void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
			// Create the accumulation texture
			if (accumTexture == null)
			{
				DestroyImmediate(accumTexture);
				accumTexture = new RenderTexture(source.width, source.height, 0);
				accumTexture.filterMode = FilterMode.Point;
				accumTexture.hideFlags = HideFlags.HideAndDontSave;
				Graphics.Blit( source, accumTexture );
				//immediately make a texture if we don't have one
			}

			// Setup the texture and floating point values in the shader
			material.SetTexture("_MainTex", accumTexture);
			accumTexture.MarkRestoreExpected();

			Graphics.Blit (accumTexture, destination);
			DestroyImmediate(accumTexture);
			//output. Then we do the flare stuff in the accumtexture which is applied more subtly

			RenderTexture blurbuffer = RenderTexture.GetTemporary(source.width/4, source.height/4, 0);
			blurbuffer.filterMode = FilterMode.Bilinear;
			Graphics.Blit(source, blurbuffer);
			material.SetFloat("_AccumOrig", blurAmount * 0.5f);
			Graphics.Blit (blurbuffer, accumTexture, material);
			RenderTexture.ReleaseTemporary (blurbuffer);

			blurbuffer = RenderTexture.GetTemporary(source.width, source.height, 0);
			blurbuffer.filterMode = FilterMode.Point;
			Graphics.Blit(source, blurbuffer);
			material.SetFloat("_AccumOrig", blurAmount);
			Graphics.Blit (blurbuffer, accumTexture, material);
			RenderTexture.ReleaseTemporary (blurbuffer);
			//layer the full res one
		}
	}
}
