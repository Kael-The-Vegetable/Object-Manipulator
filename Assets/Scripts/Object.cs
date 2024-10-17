using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Object : MonoBehaviour, Interactable
{
    [SerializeField] private Rigidbody _body;
    [SerializeField][Range(1, 10)] private float _speed;
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
            Vector3 newPosDelta = _desiredPlace.position - transform.position;
            Debug.Log(newPosDelta);
            transform.Translate(newPosDelta * Time.deltaTime * _speed, Space.World);
        }
    }
}
