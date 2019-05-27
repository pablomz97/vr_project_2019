using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerInputTest : MonoBehaviour
{

	public SteamVR_Input_Sources handType;
	public SteamVR_Action_Boolean action;
	public SteamVR_Action_Vibration hapticAction;

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
		chalk = new PaintChalk(hapticAction);
		chalkLines.Add(chalk);
		paint = true;
		Debug.Log("Trigger was pressed");
	}

	public void OnTriggerReleased(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
	{
		paint = false;
		Debug.Log("Trigger was released");
	}

	void FixedUpdate(){
		if(paint){
			Vector3 handPos = GameObject.Find("RightHand").transform.position;
			chalk.Add(handPos);
		}
	}

}
