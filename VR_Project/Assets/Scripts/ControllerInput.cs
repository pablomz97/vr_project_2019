using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerInput : MonoBehaviour
{

	public SteamVR_Input_Sources handType;
	public SteamVR_Action_Boolean ChalkAction;
	public SteamVR_Action_Boolean TouchpadAction;
	public SteamVR_Action_Vibration hapticAction;

	public Material chalkMat;

	private List<PaintChalk> chalkLines = new List<PaintChalk>();
	private PaintChalk chalk;

	private bool paint;
	private GameObject[] symbolBits;
	private GameObject lastSymbolBit;
	private Vector3 colliderPosition;

	void Start()
	{
		ChalkAction.AddOnStateDownListener(OnTriggerPressed, handType);
		ChalkAction.AddOnStateUpListener(OnTriggerReleased, handType);
		TouchpadAction.AddOnStateDownListener(OnTouchpadPressed, handType);
		TouchpadAction.AddOnStateUpListener(OnTouchpadReleased, handType);
		Debug.Log("Added listener");

		symbolBits = GameObject.FindGameObjectsWithTag("SymbolBit");
		
	}

	public void OnTouchpadPressed(SteamVR_Action_Boolean ChalkAction, SteamVR_Input_Sources source)
	{
		if(lastSymbolBit)
		{
			GameObject currentBit = lastSymbolBit;
			currentBit.GetComponent<SymbolBit>().toggleActive();
		}
	}

	public void OnTouchpadReleased(SteamVR_Action_Boolean ChalkAction, SteamVR_Input_Sources source)
	{

	}

	public void OnTriggerPressed(SteamVR_Action_Boolean ChalkAction, SteamVR_Input_Sources source)
	{
		chalk = new PaintChalk(hapticAction, chalkMat);
		chalkLines.Add(chalk);
		paint = true;
		Debug.Log("Trigger was pressed");
	}

	public void OnTriggerReleased(SteamVR_Action_Boolean ChalkAction, SteamVR_Input_Sources source)
	{
		paint = false;
		if (chalkLines[chalkLines.Count - 1].IsEmpty())
		{
			Destroy(chalkLines[chalkLines.Count - 1].gameObject);
			chalkLines.RemoveAt(chalkLines.Count - 1);
		}
		Debug.Log("Trigger was released");
	}

	void FixedUpdate()
	{
		if(paint)
		{
			Vector3 handPos = GameObject.Find("RightHand").transform.position;
			Debug.Log("controller height: " + handPos.y);
			if(handPos.y <= (0.15f + 0.13f))
			{
				chalk.Add(handPos);
				Debug.Log("adding point to chalk line"); 
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

		if(lastSymbolBit)
			lastSymbolBit.GetComponent<SymbolBit>().setHighlighted(false);
		
		colliderPosition = transform.Find("Sphere").position;
		Collider[] hitColliders = Physics.OverlapSphere(colliderPosition, 0.05f);

		lastSymbolBit = null;
		if(hitColliders.Length != 0)
		{
			float minDist = float.MaxValue;
			GameObject minObj = null;
			for(int i = 0; i < hitColliders.Length; ++i)
			{
				if(hitColliders[i].gameObject.tag == "SymbolBit")
				{
					float dist = Vector3.Distance(colliderPosition, hitColliders[i].gameObject.transform.position);
					if(dist < minDist)
					{
						minDist = dist;
						minObj = hitColliders[i].gameObject;
					}
				}
			}
			if(minObj)
			{
				lastSymbolBit = minObj;
				minObj.gameObject.GetComponent<SymbolBit>().setHighlighted(true);
			}
		}
	}
}
