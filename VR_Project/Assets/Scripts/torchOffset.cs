using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class torchOffset : MonoBehaviour
{
    //public bool updateTransform = true;
    public Vector3 position;
    public Quaternion rotation;

    //public Vector3 actualPosition;
    //public Quaternion actualRotation;

    // Start is called before the first frame update
    void Start()
    {
        position = new Vector3(0f,-0.35f,0.05f);
        rotation = Quaternion.Euler(-77,0,0) * Quaternion.Euler(0,50,0);

        this.gameObject.transform.localPosition = position;
        this.gameObject.transform.localRotation = rotation;
    }

    // Update is called once per frame
    /*
    void Update()
    {
        actualPosition = this.gameObject.transform.localPosition;
        actualRotation = this.gameObject.transform.localRotation;
        if(updateTransform){
            this.gameObject.transform.localPosition = position;
            this.gameObject.transform.localRotation = rotation;
        }
    }
    */
}
