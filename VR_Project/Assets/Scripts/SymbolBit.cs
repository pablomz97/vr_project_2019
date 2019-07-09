using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymbolBit : MonoBehaviour
{
    public int bitIndex = 0;
    public float baseBrightness = 1.5f;
    private bool active = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setActive(bool isActive)
    {
        Debug.Log("setting bit " + bitIndex + "to " + isActive);
        active = isActive;
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material mat = renderer.material;
        mat.SetColor("_EmissionColor", new Color(active ? baseBrightness : 0,
                                                 active ? baseBrightness : 0,
                                                 active ? baseBrightness : 0));
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
        if(other.gameObject.name == "LeftHand" || other.gameObject.name == "RightHand")
            setActive(!active);
    }
}
