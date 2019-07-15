using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLocked : MonoBehaviour
{
  private Animator anim;
  public GameObject[] symbols;
  public int[] targetCode;

  // Start is called before the first frame update
  void Start()
  {
    anim = GetComponent<Animator>();
    //anim.Play("AN_Door_open", 0, 0);
  }

  // Update is called once per frame
  void Update()
  {

  }
}
