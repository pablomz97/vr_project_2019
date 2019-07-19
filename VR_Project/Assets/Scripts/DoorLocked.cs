using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLocked : MonoBehaviour
{
  private Animator anim;
  private bool solved = false;
  public GameObject[] symbols;
  public GameObject middleSymbol;
  private float timestamp;
  private bool codeChecked;
  public int[] targetCode = new int[]{127, 127, 127, 127, 127, 127};

  // Start is called before the first frame update
  void Start()
  {
    anim = GetComponent<Animator>();
    //anim.Play("AN_Door_open", 0, 0);

    foreach(var symbol in symbols)
      symbol.GetComponent<SymbolPanel>().registerCallback(UpdateDoor);
  }

  public bool checkCode()
  {
    bool correct = true;
    for(int i = 0; i < symbols.Length; ++i)
    {
      Debug.Log("symbol: " + i.ToString() + " target: " + targetCode[i] + " current: " + symbols[i].GetComponent<SymbolPanel>().getNumber());
      if(symbols[i].GetComponent<SymbolPanel>().getNumber() != targetCode[i])
      {
        correct = false;
      }
      else
      {
          symbols[i].GetComponent<SymbolPanel>().markSolved();
      }
    }

    return correct;
  }

  public void UpdateDoor()
  {
	timestamp = Time.time;
	codeChecked = false;
  }

  // Update is called once per frame
  void FixedUpdate()
  {
	if (!codeChecked && Time.time - timestamp > 3)
	{
	  Debug.Log("checking code");
	  if (!solved && checkCode())
	  {
	    solved = true;
	    anim.Play("AN_Door_open", 0, 0);
	    Destroy(transform.Find("Door_Collider").gameObject);
	  }
	  codeChecked = true;
	}
  }
}
