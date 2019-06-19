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
		SteamVR_Fade.Start(Color.black, fadeDuration);
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
