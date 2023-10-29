using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICurve
{
    // 0 <= x <= 1
    float Evaluate(float x);
}
