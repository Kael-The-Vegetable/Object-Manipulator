using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Moveable : MonoBehaviour, Interactable
{
    [SerializeField] private Rigidbody _body;
    [SerializeField][Range(1, 10)] private float _speed;
    [SerializeField][Range(0, 1)] private float _rotationLerp;
    private float _originalDrag;
    public Transform DesiredPlace 
    { 
        get => _desiredPlace; 
        set
        {
            _desiredPlace = value;
            if (value != null)
            {
                _body.useGravity = false;
                _body.drag = 1;
            }
            else
            {
                _body.useGravity = true;
                _body.drag = _originalDrag;
            }
        }
    }
    [SerializeField][ReadOnly] private Transform _desiredPlace = null;

    void Awake()
    {
        if (_body == null)
        {
            _body = GetComponent<Rigidbody>();
            _originalDrag = _body.drag;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_desiredPlace != null)
        {
            //movement
            Vector3 newPosDelta = _desiredPlace.position - transform.position;
            transform.Translate(newPosDelta * Time.deltaTime * _speed, Space.World);
            
            //rotation
            Quaternion desiredRotation = Quaternion.Euler(_desiredPlace.localEulerAngles);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, _rotationLerp);
        }
    }
}
