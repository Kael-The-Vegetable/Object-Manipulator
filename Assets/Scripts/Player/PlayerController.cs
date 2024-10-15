using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    #region General Variables
    [SerializeField] private Rigidbody _body;
    [SerializeField][ReadOnly] private bool _isLinked = false;
    #endregion

    #region Movement Variables
    [SerializeField][ReadOnly] private Vector2 _moveDir;
    [Min(0)] public float speed;
    [Min(0)] public float maxSpeed;
    #endregion

    private void Awake()
    {
        if (_body == null)
        {
            _body = GetComponent<Rigidbody>();
        }
    }

    private void OnEnable()
    {
        LinkControls(true);
    }
    private void OnDisable()
    {
        LinkControls(false);
    }

    #region Link Controls
    public void LinkControls(bool linkUp)
    {
        if (linkUp)
        {
            StartCoroutine(Link());
        }
        else
        {
            UnLink();
        }
    }
    private IEnumerator Link()
    {
        bool unableToLink = false;
        while (!_isLinked && !unableToLink)
        {
            if (GameManager.Instance != null)
            {
                try
                {
                    GameManager.Instance.PlayerInput.actions["Move"].performed += OnMove;
                    GameManager.Instance.PlayerInput.actions["Move"].canceled  += OnMove;
                    _isLinked = true;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    _isLinked = false;
                }
            }
            else
            {
                _isLinked = false;
                unableToLink = true;
            }
            yield return null;
        }
    }
    private void UnLink()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerInput.actions["Move"].performed -= OnMove;
            GameManager.Instance.PlayerInput.actions["Move"].canceled  -= OnMove;
        }
        _isLinked = false;
    }
    #endregion

    private void FixedUpdate()
    {
        Vector3 trueMoveDir = transform.forward * _moveDir.y + transform.right * _moveDir.x;
        Debug.Log(trueMoveDir);
        Debug.Log(trueMoveDir * speed * (1 - _body.velocity.magnitude / maxSpeed));
        _body.AddForce(trueMoveDir * speed * (1 - _body.velocity.magnitude / maxSpeed));
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveDir = ctx.ReadValue<Vector2>();
    }
}
