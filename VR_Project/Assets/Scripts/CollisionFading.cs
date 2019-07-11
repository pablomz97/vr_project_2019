using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class CollisionFading : MonoBehaviour
{

	public float fadeDuration;

	private int collidingCount;

	private void OnTriggerEnter(Collider other)
	{
		//Debug.Log("entered");
		SteamVR_Fade.Start(new Color(0.65f, 0.04f, 0.04f, 1), fadeDuration);
		collidingCount++;
	}

	private void OnTriggerExit(Collider other)
	{
		//Debug.Log("exited");
		collidingCount--;
		if(collidingCount < 1){
			SteamVR_Fade.Start(Color.clear, fadeDuration);
		}
	}
}
