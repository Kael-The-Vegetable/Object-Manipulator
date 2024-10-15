using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    #region General Variables
    [SerializeField] 
    private Rigidbody _body;
    [SerializeField][ReadOnly] private bool _isLinked = false;
    #endregion

    #region Movement Variables
    // movement
    [SerializeField][ReadOnly] private Vector2 _moveDir;
    [SerializeField][ReadOnly] private Vector3 _trueMoveDir;
    [Min(0)] public float speed;
    [Min(0)] public float maxSpeed;

    // jump detection
    [SerializeField] private bool _canJump = true;
    [SerializeField][ReadOnly] private bool _isGrounded = false;
    [SerializeField] private float _groundRayDistance;
    [SerializeField] private LayerMask _groundLayer;
    [Min(0)] public float jumpStrength;

    
    #endregion

    #region Debug Variables
    [SerializeField] private bool _showGroundRay;
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
                    GameManager.Instance.PlayerInput.actions["Jump"].performed += OnJump;
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
            GameManager.Instance.PlayerInput.actions["Jump"].performed -= OnJump;
        }
        _isLinked = false;
    }
    #endregion

    private void FixedUpdate()
    {
        if (_canJump)
        {
            _isGrounded = Physics.Raycast(transform.position, -transform.up, _groundRayDistance, _groundLayer);
        }

        _trueMoveDir = transform.forward * _moveDir.y + transform.right * _moveDir.x;

        if (_body.velocity.magnitude < maxSpeed)
        {
            _body.AddForce(_trueMoveDir * speed * (1 - _body.velocity.magnitude / maxSpeed));
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveDir = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (_isGrounded)
        {
            _body.AddForce(transform.up * jumpStrength, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        if (_showGroundRay && _canJump)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, -transform.up * _groundRayDistance);
        }
    }
}
