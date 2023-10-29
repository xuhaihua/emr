using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EMR.Struct;

public class BuiltInCurve
{
    public static ICurve GetCurve(MotionCurve curveType)
    {
        switch (curveType)
        {
            case MotionCurve.Linear:
                return LinearCurve.Instance;

            case MotionCurve.InSine:
                return InSineCurve.Instance;
            case MotionCurve.OutSine:
                return OutSineCurve.Instance;
            case MotionCurve.InOutSine:
                return InOutSineCurve.Instance;


            case MotionCurve.InQuad:
                return InQuadCurve.Instance;
            case MotionCurve.OutQuad:
                return OutQuadCurve.Instance;
            case MotionCurve.InOutQuad:
                return InOutQuadCurve.Instance;


            case MotionCurve.InCubic:
                return InCubicCurve.Instance;
            case MotionCurve.OutCubic:
                return OutCubicCurve.Instance;
            case MotionCurve.InOutCubic:
                return InOutCubicCurve.Instance;


            case MotionCurve.InQuart:
                return InQuartCurve.Instance;
            case MotionCurve.OutQuart:
                return OutQuartCurve.Instance;
            case MotionCurve.InOutQuart:
                return InOutQuartCurve.Instance;


            case MotionCurve.InQuint:
                return InQuintCurve.Instance;
            case MotionCurve.OutQuint:
                return OutQuintCurve.Instance;
            case MotionCurve.InOutQuint:
                return InOutQuintCurve.Instance;


            case MotionCurve.InExpo:
                return InExpoCurve.Instance;
            case MotionCurve.OutExpo:
                return OutExpoCurve.Instance;
            case MotionCurve.InOutExpo:
                return InOutExpoCurve.Instance;


            case MotionCurve.InCirc:
                return InCircCurve.Instance;
            case MotionCurve.OutCirc:
                return OutCircCurve.Instance;
            case MotionCurve.InOutCirc:
                return InOutCircCurve.Instance;


            case MotionCurve.InBack:
                return InBackCurve.Instance;
            case MotionCurve.OutBack:
                return OutBackCurve.Instance;
            case MotionCurve.InOutBack:
                return InOutBackCurve.Instance;


            case MotionCurve.InBounce:
                return InBounceCurve.Instance;
            case MotionCurve.OutBounce:
                return OutBounceCurve.Instance;
            case MotionCurve.InOutBounce:
                return InOutBounceCurve.Instance;


            case MotionCurve.InElastic:
                return InElasticCurve.Instance;
            case MotionCurve.OutElastic:
                return OutElasticCurve.Instance;
            case MotionCurve.InOutElastic:
                return InOutElasticCurve.Instance;


            default:
                throw new System.Exception("Invalid MotionCurve value");
        }
    }

}
