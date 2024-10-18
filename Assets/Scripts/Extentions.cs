using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extentions
{
    public static Vector3 Flatten(this Vector3 v)
    {
        float xzMag = Mathf.Sqrt(v.x * v.x + v.z * v.z);
        
        if (xzMag == 0)
        { 
            return new Vector3(0, 0, 0); 
        }

        float scale = v.magnitude / xzMag;

        return new Vector3(v.x * scale, 0, v.z * scale);
    }
}
