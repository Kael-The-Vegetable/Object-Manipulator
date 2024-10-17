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
    #endregion

    #region Link Variables
    // I had to split these as for some reason both actionmaps are considered active when I have events linked.
    private bool _playerIsLinked = false;
    private bool _manipulateIsLinked = false;
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
    [SerializeField] private string _nameOfKeyboardMouse;
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
    [SerializeField][ReadOnly] private Vector2 _lookDelta;
    [SerializeField] private Transform _lookTarget;
    public Transform LookTarget { get => _lookTarget; set => _lookTarget = value; }
    [Min(0)] public float lookStrength;
    [Range(-90, 0)][SerializeField] private float _maxLookUpAngle;
    [Range(0, 90)][SerializeField] private float _maxLookDownAngle;
    #endregion

    #region Grab Variables
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField][Min(0)] private float _maxObjDistance;
    [SerializeField][Min(0)] private float _objDistance;
    [SerializeField] private Transform _desiredPlace;
    [SerializeField][ReadOnly] private Object _grabbed;
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
            if (GameManager.Instance != null)
            {
                StartCoroutine(UnLink());
            }
            _playerIsLinked = false;
            _manipulateIsLinked = false;
            
        }
    }
    private IEnumerator Link()
    {
        yield return null;
        if (GameManager.Instance != null)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                
                if (
                    (_events[i].ActionMap != PlayerEvents.ActionMapUsed.Manipulate 
                    && GameManager.Instance.PlayerInput
                        .currentActionMap.name == "Player")
                  ||
                    (_events[i].ActionMap != PlayerEvents.ActionMapUsed.Player 
                    && GameManager.Instance.PlayerInput
                        .currentActionMap.name == "Manipulate"))
                {
                    LinkMethod(_events[i].Method, _events[i].ActionName, _events[i].SubscribeTo, _events[i].ActionMap, true);
                }
            }
            if (GameManager.Instance.PlayerInput.currentActionMap.name == "Player")
            {
                _playerIsLinked = true;
            }
            else
            {
                _manipulateIsLinked = true;
            }
        }
    }
    private IEnumerator UnLink()
    {
        for (int i = 0; i < _events.Count; i++)
        {
            yield return null;
            LinkMethod(_events[i].Method, _events[i].ActionName, _events[i].SubscribeTo, _events[i].ActionMap, false);
        }
    }
    private void LinkMethod(PlayerEvents.EventMethods method, string actionName, 
                            PlayerEvents.SubscribeType type, 
                            PlayerEvents.ActionMapUsed actionMap, bool linkUp)
    {
        BasicInputController methodToUse = null;
        int ID = 0;

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
            case PlayerEvents.EventMethods.OnGrab:
                methodToUse = OnGrab;
                break;
        }

        if (actionMap != PlayerEvents.ActionMapUsed.Both)
        {
            switch (actionMap)
            {
                case PlayerEvents.ActionMapUsed.Player:
                    ID = 0;
                    break;
                case PlayerEvents.ActionMapUsed.Manipulate:
                    ID = 1;
                    break;
            }
            switch (type)
            {
                case PlayerEvents.SubscribeType.Performed:
                case PlayerEvents.SubscribeType.Canceled:
                    LinkHelper(actionName, methodToUse, type, linkUp, ID);
                    break;
                case PlayerEvents.SubscribeType.Both:
                    LinkHelper(actionName, methodToUse, PlayerEvents.SubscribeType.Performed, linkUp, ID);
                    LinkHelper(actionName, methodToUse, PlayerEvents.SubscribeType.Canceled, linkUp, ID);
                    break;
            }
        }
        else
        {
            if (GameManager.Instance.PlayerInput.currentActionMap.name == "Player")
            {
                ID = 0;
            }
            else
            {
                ID = 1;
            }
            switch (type)
            {
                case PlayerEvents.SubscribeType.Performed:
                case PlayerEvents.SubscribeType.Canceled:
                    LinkHelper(actionName, methodToUse, type, linkUp, ID);
                    break;
                case PlayerEvents.SubscribeType.Both:
                    LinkHelper(actionName, methodToUse, PlayerEvents.SubscribeType.Performed, linkUp, ID);
                    LinkHelper(actionName, methodToUse, PlayerEvents.SubscribeType.Canceled, linkUp, ID);
                    break;
            }
        }
    }
    private void LinkHelper(string actionName, BasicInputController method, PlayerEvents.SubscribeType action, bool linkUp, int actionMapID)
    {
        switch (action)
        {
            case PlayerEvents.SubscribeType.Performed:
                if (linkUp)
                {
                    InputAction foundAction = GameManager.Instance.PlayerInput.actions.actionMaps[actionMapID].FindAction(actionName);
                    if (foundAction != null)
                    {
                        foundAction.performed += method.Invoke;
                    }
                }
                else
                {
                    InputAction foundAction = GameManager.Instance.PlayerInput.actions.actionMaps[actionMapID].FindAction(actionName);
                    if (foundAction != null)
                    {
                        foundAction.performed -= method.Invoke;
                    }
                }
                break;
            case PlayerEvents.SubscribeType.Canceled:
                if (linkUp)
                {
                    InputAction foundAction = GameManager.Instance.PlayerInput.actions.actionMaps[actionMapID].FindAction(actionName);
                    if (foundAction != null)
                    {
                        foundAction.canceled += method.Invoke;
                    }
                }
                else
                {
                    InputAction foundAction = GameManager.Instance.PlayerInput.actions.actionMaps[actionMapID].FindAction(actionName);
                    if (foundAction != null)
                    {
                        foundAction.canceled -= method.Invoke;
                    }
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

        Look(_lookDelta);
    }

    public void Look(Vector2 delta)
    {
        if (_lookTarget == null)
        {
            Debug.LogError("You must set a Look target for camera movement to function.");
            return;
        }

        transform.rotation *= Quaternion.AngleAxis(delta.x * lookStrength, Vector3.up);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        _lookTarget.rotation *= Quaternion.AngleAxis(delta.y * lookStrength, Vector3.right);


        //clamp the up/down axis
        Vector3 angles = _lookTarget.localEulerAngles;
        angles.z = 0;
        angles.y = 0;

        if (angles.x > 180 && angles.x < 360 + _maxLookUpAngle)
        {
            angles.x = 360 + _maxLookUpAngle;
        }
        else if (angles.x < 180 && angles.x > _maxLookDownAngle)
        {
            angles.x = _maxLookDownAngle;
        }
        _lookTarget.localEulerAngles = angles;
    }

    private IEnumerator WaitBeforeChangeActionMap(string mapToUse)
    {
        yield return null;
        
    }

    #region Input Controls
    private delegate void BasicInputController(InputAction.CallbackContext ctx);
    public void OnMove(InputAction.CallbackContext ctx)
    {
        Debug.Log(ctx.ReadValue<Vector2>());
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
        if (ctx.canceled ||
            GameManager.Instance.PlayerInput.currentControlScheme != _nameOfKeyboardMouse ||        
           (GameManager.Instance.PlayerInput.currentControlScheme == _nameOfKeyboardMouse && _lookEnabled))
        {
            _lookDelta = ctx.ReadValue<Vector2>();
        }
    }
    public void OnGrab(InputAction.CallbackContext ctx)
    {
        
        if (GameManager.Instance.PlayerInput.currentActionMap.name == "Player")
        {
            Ray ray = Camera.main.ScreenPointToRay(
                Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hitInfo, _maxObjDistance + 9, _interactableLayer))
            {
                if (hitInfo.transform.TryGetComponent(out _grabbed))
                {
                    _objDistance = hitInfo.distance - 9;

                    _desiredPlace.localPosition = new Vector3(
                        _desiredPlace.localPosition.x,
                        _desiredPlace.localPosition.y, 
                        _objDistance);

                    _grabbed.DesiredPlace = _desiredPlace;

                    GameManager.Instance.PlayerInput.SwitchCurrentActionMap("Manipulate");
                    if (!_manipulateIsLinked)
                    {
                        LinkControls(true);
                    }
                }
            }
        }
        else
        {
            if (_grabbed != null)
            {
                _grabbed.DesiredPlace = null;
                _grabbed = null;
            }
            GameManager.Instance.PlayerInput.SwitchCurrentActionMap("Player");
            if (!_playerIsLinked)
            {
                LinkControls(true);
            }
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
