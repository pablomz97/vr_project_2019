using System;
using UnityEngine;

public class ImpactSound : MonoBehaviour
{
    private AudioSource source;
    public AudioClip[] sound;
    [Range(0f,1f)]
    public float volume;
    System.Random rnd = new System.Random();
    // Start is called before the first frame update
    void Start()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.loop = false;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 1;
        source.minDistance = 6;
        source.volume = volume;
        source.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        int i = rnd.Next(0, sound.Length);
        source.clip = sound[i];
        source.Play();
    }
}
