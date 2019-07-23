using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System;

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

	private AudioSource audioSourceClick;
	public AudioClip[] clickSounds;
    [Range(0f,1f)]
	public float volumeClick;

    private AudioSource audioSourceChalk;
    public AudioClip[] chalkSounds;
    [Range(0f, 1f)]
    public float volumeChalk;
    private bool soundOn;
    private int lastSound = 0;

    System.Random rnd = new System.Random();

	void Start()
	{
		ChalkAction.AddOnStateDownListener(OnTriggerPressed, handType);
		ChalkAction.AddOnStateUpListener(OnTriggerReleased, handType);
		TouchpadAction.AddOnStateDownListener(OnTouchpadPressed, handType);
		TouchpadAction.AddOnStateUpListener(OnTouchpadReleased, handType);
		Debug.Log("Added listener");

		symbolBits = GameObject.FindGameObjectsWithTag("SymbolBit");

        //AudioSource Setup for clicks
        audioSourceClick = gameObject.AddComponent<AudioSource>();
        audioSourceClick.loop = false;
        audioSourceClick.volume = volumeClick;
        audioSourceClick.playOnAwake = false;

        //AudioSource Setup for chalk
        audioSourceChalk = gameObject.AddComponent<AudioSource>();
        audioSourceChalk.loop = false;
        audioSourceChalk.volume = volumeChalk;
        audioSourceChalk.playOnAwake = false;
    }

	public void OnTouchpadPressed(SteamVR_Action_Boolean ChalkAction, SteamVR_Input_Sources source)
	{
		if(lastSymbolBit)
		{
			GameObject currentBit = lastSymbolBit;
			currentBit.GetComponent<SymbolBit>().toggleActive();

			//click sound
			int i = rnd.Next(0, clickSounds.Length);
            audioSourceClick.clip = clickSounds[i];
            audioSourceClick.Play();
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
		soundOn = false;
		if (chalkLines[chalkLines.Count - 1].IsEmpty())
		{
			Destroy(chalkLines[chalkLines.Count - 1].gameObject);
			chalkLines.RemoveAt(chalkLines.Count - 1);
		}
		Debug.Log("Trigger was released");
	}

	void FixedUpdate()
	{
        StartCoroutine(playChalk());
		if(paint)
		{
			Vector3 handPos = GameObject.Find("RightHand").transform.position;
			Debug.Log("controller height: " + handPos.y);
			if(handPos.y <= (0.15f + 0.13f))
			{
                soundOn = true;
                chalk.Add(handPos);
				Debug.Log("adding point to chalk line");
			}
			else
			{
                soundOn = false;
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

    IEnumerator playChalk()
    {
        if (soundOn)
        {
            if (!audioSourceChalk.isPlaying)
            {
                //chalk sound
                do
                {
                    int i = rnd.Next(0, chalkSounds.Length);
                } while (i == lastSound);
                lastSound = i;
                audioSourceChalk.clip = chalkSounds[i];
                audioSourceChalk.Play();
            }
        }
        else
        {
            if (audioSourceChalk.isPlaying)
            {
                audioSourceChalk.Stop();
            }
        }

        yield return null;
    }
}
