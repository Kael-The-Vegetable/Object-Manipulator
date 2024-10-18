using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interactable
{
    public Transform DesiredPlace { get; set; }
    public GameObject gameObject { get; }
}

public interface ICombinable
{
    
}
