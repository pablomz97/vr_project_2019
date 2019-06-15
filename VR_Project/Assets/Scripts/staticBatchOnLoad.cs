using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class staticBatchOnLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
      StaticBatchingUtility.Combine(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //updates all static data which can be updated at runtime, this should be called once all objects have been placed at their final location
    void Bake()
    {
      ReflectionProbe probeComponent = gameObject.transform.Find("ReflectionProbe").gameObject.GetComponent<ReflectionProbe>();

      if (probeComponent != null)
        probeComponent.RenderProbe();
    }
}
