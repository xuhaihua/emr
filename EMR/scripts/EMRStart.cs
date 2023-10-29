using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;


public class EMRStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var service = new EMR.EMRService(MixedRealityToolkit.Instance, "MainService", 10, null);
        MixedRealityServiceRegistry.AddService<EMR.EMRService>(service, MixedRealityToolkit.Instance);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
