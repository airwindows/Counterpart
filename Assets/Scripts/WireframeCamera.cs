using UnityEngine;
using System.Collections;

/// <summary>
/// This class may be attached to a camera, and
/// adjusts its rendering to use wireframe mode.
/// </summary>
public class WireframeCamera : MonoBehaviour
{	
	public bool wireframe = true;

	void OnPreRender ()
	{
		if (wireframe) {
			GL.wireframe = true;
		}
	}

	void OnPostRender ()
	{
		if (wireframe) {
			GL.wireframe = false;
		}
	}
}