using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerInput : MonoBehaviour
{

	public SteamVR_Input_Sources handType;
	public SteamVR_Action_Boolean action;
	public SteamVR_Action_Vibration hapticAction;

	public Material chalkMat;

	private List<PaintChalk> chalkLines = new List<PaintChalk>();
	private PaintChalk chalk;

	private bool paint;

	void Start()
	{
		action.AddOnStateDownListener(OnTriggerPressed, handType);
		action.AddOnStateUpListener(OnTriggerReleased, handType);
		Debug.Log("Added listener");
	}

	public void OnTriggerPressed(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
	{
		chalk = new PaintChalk(hapticAction, chalkMat);
		chalkLines.Add(chalk);
		paint = true;
		Debug.Log("Trigger was pressed");
	}

	public void OnTriggerReleased(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
	{
		paint = false;
		if (chalkLines[chalkLines.Count - 1].IsEmpty())
		{
			Destroy(chalkLines[chalkLines.Count - 1].gameObject);
			chalkLines.RemoveAt(chalkLines.Count - 1);
		}
		Debug.Log("Trigger was released");
	}

	void FixedUpdate(){
		if(paint){
			Vector3 handPos = GameObject.Find("RightHand").transform.position;
			if(handPos.y <= 0.15f)
			{
				chalk.Add(handPos);
			}
			else
			{
				if (chalkLines[chalkLines.Count - 1].IsEmpty())
				{
					Destroy(chalkLines[chalkLines.Count - 1].gameObject);
					chalkLines.RemoveAt(chalkLines.Count - 1);
				}
				chalk = new PaintChalk(hapticAction, chalkMat);
				chalkLines.Add(chalk);
			}
		}
	}

}
