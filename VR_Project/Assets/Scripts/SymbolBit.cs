using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SymbolBit : MonoBehaviour
{
    public int bitIndex = 0;
    public float baseBrightness = 1.5f;
    public float highlightBrightnessOffset = .5f;
    private bool active = false;
    private float lastTime = 0;
    public bool editable = true;
    Action updateCallback;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void registerCallback(Action callback)
    {
        updateCallback = callback;
    }

    public bool isActive()
    {
        return active;
    }

    public void toggleActive()
    {
        if(editable)
            setActive(!active);
    }

    public void setActive(bool isActive)
    {
        Debug.Log("setting bit " + bitIndex + "to " + isActive);
        active = isActive;
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material mat = new Material(renderer.sharedMaterial);
        mat.SetColor("_EmissionColor", new Color(active ? baseBrightness : 0,
                                                 active ? baseBrightness : 0,
                                                 active ? baseBrightness : 0));
        renderer.sharedMaterial = mat;
        
        if(updateCallback != null)
            updateCallback();
    }

    public void setHighlighted(bool isHighlighted)
    {
        if(!editable)
            return;
        
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material mat = renderer.material;
        if(active)
            mat.SetColor("_EmissionColor", new Color(isHighlighted ? baseBrightness - highlightBrightnessOffset : baseBrightness,
                                                     isHighlighted ? baseBrightness - highlightBrightnessOffset : baseBrightness,
                                                     isHighlighted ? baseBrightness - highlightBrightnessOffset : baseBrightness));
        else
            mat.SetColor("_EmissionColor", new Color(isHighlighted ? highlightBrightnessOffset : 0,
                                                     isHighlighted ? highlightBrightnessOffset : 0,
                                                     isHighlighted ? highlightBrightnessOffset : 0));
    }

    public void setNumber(int number)
    {
        Debug.Log("number: " + number + ", mask: " + (1 << bitIndex) + ", res: " + ((1 << bitIndex) & number));
        setActive(((1 << bitIndex) & number) > 0);
    }

    public int getNumber()
    {
        if(active)
            return 1 << bitIndex;

        return 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if((other.gameObject.name == "LeftHand" || other.gameObject.name == "RightHand") && Time.time > (lastTime + .05f))
            setActive(!active);

        
    }

    private void OnTriggerExit(Collider other)
    {
        lastTime = Time.time;
    }
}
