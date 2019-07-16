﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLocked : MonoBehaviour
{
  private Animator anim;
  private bool solved = false;
  public GameObject[] symbols;
  public GameObject middleSymbol;
  int[] targetCode = new int[]{127, 127, 127, 127, 127, 127};

  // Start is called before the first frame update
  void Start()
  {
    anim = GetComponent<Animator>();
    //anim.Play("AN_Door_open", 0, 0);

    foreach(var symbol in symbols)
      symbol.GetComponent<SymbolPanel>().registerCallback(updateDoor);
  }

  public bool checkCode()
  {
    for(int i = 0; i < symbols.Length; ++i)
    {
      Debug.Log("symbol: " + i.ToString() + " target: " + targetCode[i] + " current: " + symbols[i].GetComponent<SymbolPanel>().getNumber());
      if(symbols[i].GetComponent<SymbolPanel>().getNumber() != targetCode[i])
        return false;
    }

    return true;
  }

  public void updateDoor()
  {
    Debug.Log("checking code");
    if(!solved && checkCode())
    {
      solved = true;
      anim.Play("AN_Door_open", 0, 0);
    }
  }

  // Update is called once per frame
  void Update()
  {

  }
}