using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class staticBatchOnLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
      //StaticBatchingUtility.Combine(gameObject);
      //Bake();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //updates all static data which can be updated at runtime, this should be called once all objects have been placed at their final location
    public void Bake()
    {
      Transform rct = gameObject.transform.Find("ReflectionProbe");

      if (rct != null)
      {
        ReflectionProbe probeComponent = rct.gameObject.GetComponent<ReflectionProbe>();
        probeComponent.RenderProbe();
      }

      Transform rctg = gameObject.transform.Find("ReflectionProbeGroup");

      if(rctg != null)
      {
        foreach(Transform child in rctg)
        {
          ReflectionProbe probeComponent = child.gameObject.GetComponent<ReflectionProbe>();
          probeComponent.RenderProbe();
        }
      }

    }
}
