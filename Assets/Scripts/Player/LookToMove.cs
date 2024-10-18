using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookToMove : MonoBehaviour
{
    [Range(0, 1)] public float lerpVal;
    public void UpdateRotation(Quaternion q)
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, q, lerpVal);
    }
    public void SetRotation(Quaternion q)
    {
        transform.rotation = q;
    }
}
