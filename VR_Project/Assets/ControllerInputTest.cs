using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerInputTest : MonoBehaviour
{

	public SteamVR_Input_Sources handType;
	public SteamVR_Action_Boolean action;


	private PaintChalk chalk = new PaintChalk();

	void OnEnable()
	{
		action.AddOnStateDownListener(OnTriggerPressed, handType);
	}

	void OnDisable()
	{
		action.RemoveOnStateDownListener(OnTriggerPressed, handType);
	}

	private void OnTriggerPressed(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
	{
		chalk.AddPoint(new Vector3(Random.Range(-2, 2), 0.01f, Random.Range(-2, 2)));
		Debug.Log("Trigger was pressed");
	}

}
