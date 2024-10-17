using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Object : MonoBehaviour, Interactable
{
    [SerializeField] private Rigidbody _body;
    [SerializeField][Range(0, 1)] private float _lerpValue;
    public Transform DesiredPlace 
    { 
        get => _desiredPlace; 
        set
        {
            _desiredPlace = value;
            if (value != null)
            {
                _body.useGravity = false;
            }
            else
            {
                _body.useGravity = true;
            }
        }
    }
    [SerializeField][ReadOnly] private Transform _desiredPlace = null;

    void Awake()
    {
        if (_body == null)
        {
            _body = GetComponent<Rigidbody>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_desiredPlace != null)
        {
            Vector3 lerpedVector = Vector3.Lerp(transform.position, _desiredPlace.position, _lerpValue);
            transform.position = lerpedVector;
        }
    }
}
