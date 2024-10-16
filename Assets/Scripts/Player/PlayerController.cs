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
    [SerializeField] private List<PlayerEvents> _events = new List<PlayerEvents>();
    public List<PlayerEvents> Events 
    { 
        get => _events;
        set
        {
            LinkControls(false);
            _events = value;
            LinkControls(true);
        }
    }
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

    #region Look Variables
    [SerializeField][ReadOnly] private bool _lookEnabled = false;
    [Min(0)] public float lookStrength;
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
                    for (int i = 0; i < _events.Count; i++)
                    {
                        LinkMethod(_events[i].Method, _events[i].ActionName, _events[i].SubscribeTo, true);
                    }
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
            for (int i = 0; i < _events.Count; i++)
            {
                LinkMethod(_events[i].Method, _events[i].ActionName, _events[i].SubscribeTo, false);
            }
        }
        _isLinked = false;
    }
    private void LinkMethod(PlayerEvents.EventMethods method, string actionName, 
                            PlayerEvents.SubscribeType type, bool linkUp)
    {
        BasicInputController methodToUse = null;
        switch (method)
        {
            case PlayerEvents.EventMethods.OnMove:
                methodToUse = OnMove;
                break;
            case PlayerEvents.EventMethods.OnJump:
                methodToUse = OnJump;
                break;
            case PlayerEvents.EventMethods.OnEnableLook:
                methodToUse = OnEnableLook;
                break;
            case PlayerEvents.EventMethods.OnLook:
                methodToUse = OnLook;
                break;
        }
        switch (type)
        {
            case PlayerEvents.SubscribeType.Performed:
                if (linkUp)
                {
                    GameManager.Instance.PlayerInput.actions[actionName].performed += methodToUse.Invoke;
                }
                else
                {
                    GameManager.Instance.PlayerInput.actions[actionName].performed -= methodToUse.Invoke;
                }
                break;
            case PlayerEvents.SubscribeType.Canceled:
                if (linkUp)
                {
                    GameManager.Instance.PlayerInput.actions[actionName].canceled  += methodToUse.Invoke;
                }
                else
                {
                    GameManager.Instance.PlayerInput.actions[actionName].canceled  -= methodToUse.Invoke;
                }
                break;
            case PlayerEvents.SubscribeType.Both:
                if (linkUp)
                {
                    GameManager.Instance.PlayerInput.actions[actionName].performed += methodToUse.Invoke;
                    GameManager.Instance.PlayerInput.actions[actionName].canceled  += methodToUse.Invoke;
                }
                else
                {
                    GameManager.Instance.PlayerInput.actions[actionName].performed -= methodToUse.Invoke;
                    GameManager.Instance.PlayerInput.actions[actionName].canceled  -= methodToUse.Invoke;
                }
                break;
        }
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

    #region Input Controls
    private delegate void BasicInputController(InputAction.CallbackContext ctx);
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
    public void OnEnableLook(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _lookEnabled = true;
        }
        else if (ctx.canceled)
        {
            _lookEnabled = false;
        }
    }
    public void OnLook(InputAction.CallbackContext ctx)
    {
        if (_lookEnabled)
        {

        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (_showGroundRay && _canJump)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, -transform.up * _groundRayDistance);
        }
    }
}
