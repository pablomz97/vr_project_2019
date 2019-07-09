using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymbolBit : MonoBehaviour
{
    public int bitIndex = 0;
    public float baseBrightness = 1.4f;
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
        active = isActive;
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material mat = renderer.material;
        mat.SetColor("_EmissionColor", new Color(active ? baseBrightness : 0,
                                                 active ? baseBrightness : 0,
                                                 active ? baseBrightness : 0));
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
