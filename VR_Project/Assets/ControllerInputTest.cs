using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerInputTest : MonoBehaviour
{

	public SteamVR_Input_Sources handType;
	public SteamVR_Action_Boolean action;


	private PaintChalk chalk;

	void Start()
	{
		chalk = new PaintChalk();
		action.AddOnStateDownListener(OnTriggerPressed, handType);
		Debug.Log("Added listener");
	}

	public void OnTriggerPressed(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
	{

		if(chalk == null){
			chalk = new PaintChalk();
			action.AddOnStateDownListener(OnTriggerPressed, handType);
			Debug.Log("Added listener");
		}

		chalk.AddPoint(new Vector3(Random.Range(-5, 5), 0.01f, Random.Range(-5, 5)));
		Debug.Log("Trigger was pressed");
	}

}
