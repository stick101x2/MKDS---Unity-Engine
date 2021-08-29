using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Func 
{
    public static bool lowQuality
    {
        get
        {
            if (QualitySettings.GetQualityLevel() == 0)
            {
                return true;
            }

            return false;
        }
    }
    public static float CalculateAngle(Vector3 from, Vector3 to)
    {
        return Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.z;
    }

    public static float Remap(this float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        var fromAbs = from - fromMin;
        var fromMaxAbs = fromMax - fromMin;

        var normal = fromAbs / fromMaxAbs;

        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;

        var to = toAbs + toMin;

        return to;
    }
}
