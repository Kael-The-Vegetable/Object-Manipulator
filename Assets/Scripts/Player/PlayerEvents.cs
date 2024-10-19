using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PlayerEvents
{
    [field:SerializeField] public string ActionName { get; private set; }

    public enum SubscribeType { Performed, Canceled, Both }
    [field:SerializeField] public SubscribeType SubscribeTo { get; private set; }

    public enum EventMethods { OnMove, OnJump, OnEnableLook, OnLook, OnGrab, OnEnableMoveMode, OnObjectMove, OnEnableRotationMode }
    [field:SerializeField] public EventMethods Method { get; private set; }

    public enum ActionMaps { Player, Manipulate, Both }
    [field:SerializeField] public ActionMaps ActionMap { get; private set; }
}
