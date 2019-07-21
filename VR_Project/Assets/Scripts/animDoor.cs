using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class animDoor : MonoBehaviour
{
    private Animator anim;
    private AudioSource source;
    public AudioClip openingSound;
    public AudioClip unlockingSound;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        source = this.gameObject.AddComponent<AudioSource>();
        source.loop = false;
        source.playOnAwake = false;
        StartCoroutine(openDoor());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator openDoor()
    {

        yield return new WaitForSeconds(1);
        anim.Play("AN_Door_open", 0, 0);

        //source.clip = unlockingSound;
        source.PlayOneShot(unlockingSound);
        yield return new WaitForSeconds(1);

        //source.clip = openingSound;
        source.PlayOneShot(openingSound);
        
        yield return null;
    }
}
