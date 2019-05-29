using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class CollisionFading : MonoBehaviour
{

	public float fadeDuration;

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("entered");
		SteamVR_Fade.Start(Color.black, fadeDuration);
	}

	private void OnTriggerExit(Collider other)
	{
		Debug.Log("exited");
		SteamVR_Fade.Start(Color.clear, fadeDuration);
	}
}
