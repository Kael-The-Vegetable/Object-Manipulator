using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookToMove : MonoBehaviour
{
    [Range(0, 100)] public float rotationPower;
    public void UpdateRotation(Quaternion q)
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, q, rotationPower * Time.deltaTime);
    }
    public void SetRotation(Quaternion q)
    {
        transform.rotation = q;
    }
}
