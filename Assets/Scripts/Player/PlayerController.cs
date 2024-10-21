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
    private enum PlayerMode
    {
        Move,
        MoveObject,
        RotateObject,
        MoveRotateObject
    }
    private PlayerMode _mode = PlayerMode.Move;
    private bool _IsOnMoveMode { get => _mode == PlayerMode.Move; }
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
    private string[] _nameOfActionMaps;
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
    [SerializeField] private LookToMove _model;
    #endregion

    #region Grab Variables
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField][Min(0)] private float _maxObjDistance;
    private float _ObjDistance
    {
        get => _originalDesiredPlace.magnitude;
        set
        {
            float temp = value / _ObjDistance;
            _originalDesiredPlace *= temp;
        }
    }
    private Quaternion _ObjRotation
    {
        get
        {
            return Quaternion.LookRotation(_originalDesiredPlace);
        }
    }
    [SerializeField] private Vector3 _originalDesiredPlace;
    [SerializeField] private Transform _desiredPlace;
    private Interactable _grabbed;
    private Collider _playerCollider;
    #endregion

    #region Manipulate Variables
    private Vector3 _moveObjectDir;
    [SerializeField][Min(0)] private float _moveObjectSpeed;
    private Vector3 _rotateObjectDelta;
    private float _rotateObjectSpeed;
    #endregion

    #region Debug Variables
    [SerializeField] private bool _showGroundRay;
    [SerializeField] private bool _showGrabRay;
    #endregion

    private void Awake()
    {
        if (_body == null)
        {
            _body = GetComponent<Rigidbody>();
        }
        if (_model == null)
        {
            _model = GetComponentInChildren<LookToMove>();
        }
        _playerCollider = GetComponentInChildren<Collider>();
        int mapCount = GameManager.Instance.PlayerInput.actions.actionMaps.Count;
        _nameOfActionMaps = new string[mapCount];
        for(int i = 0; i < mapCount; i++)
        {
            _nameOfActionMaps[i] = GameManager.Instance.PlayerInput.actions.actionMaps[i].name;
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
                UnLink();
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
                string actionMap = _events[i].ActionMap.ToString();
                if (
                    (actionMap != _nameOfActionMaps[1]
                    && GameManager.Instance.PlayerInput
                        .currentActionMap.name == _nameOfActionMaps[0])
                  ||
                    (actionMap != _nameOfActionMaps[0]
                    && GameManager.Instance.PlayerInput
                        .currentActionMap.name == _nameOfActionMaps[1]))
                {
                    LinkMethod(_events[i].Method, _events[i].ActionName, _events[i].SubscribeTo, actionMap, true);
                }
            }
            if (GameManager.Instance.PlayerInput.currentActionMap.name == _nameOfActionMaps[0])
            {
                _playerIsLinked = true;
            }
            else if (GameManager.Instance.PlayerInput.currentActionMap.name == _nameOfActionMaps[1])
            {
                _manipulateIsLinked = true;
            }
        }
    }
    private void UnLink()
    {
        for (int i = 0; i < _events.Count; i++)
        {
            LinkMethod(_events[i].Method, _events[i].ActionName, _events[i].SubscribeTo, _events[i].ActionMap.ToString(), false);
        }
    }
    private void LinkMethod(PlayerEvents.EventMethods method, string actionName, 
                            PlayerEvents.SubscribeType type, 
                            string map, bool linkUp)
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
            case PlayerEvents.EventMethods.OnEnableMoveMode:
                methodToUse = OnEnableMoveMode;
                break;
            case PlayerEvents.EventMethods.OnObjectMove:
                methodToUse = OnObjectMove;
                break;
            case PlayerEvents.EventMethods.OnEnableRotationMode:
                methodToUse = OnEnableRotationMode;
                break;
        }

        if (map == "Both")
        {
            bool found = false;
            for (int i = 0; i < _nameOfActionMaps.Length && !found; i++)
            {
                if (GameManager.Instance.PlayerInput.currentActionMap.name == _nameOfActionMaps[i])
                {
                    found = true;
                    ID = i;
                }
            }
        }
        else
        {
            bool found = false;
            for (int i = 0; i < _nameOfActionMaps.Length && !found; i++)
            {
                if (map == _nameOfActionMaps[i])
                {
                    found = true;
                    ID = i;
                }
            }
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
        #region Grabbed Object
        if (_grabbed != null)
        {
            MoveObject(_moveObjectDir);

            Vector3 difference = _desiredPlace.position - _lookTarget.position;

            _model.UpdateRotation(Quaternion.LookRotation(difference.Flatten()));

            #region Ensure Object Doesn't Go Through Ground
            Debug.DrawRay(_lookTarget.position, difference, Color.red);
            if (Physics.Raycast(_lookTarget.position, difference.normalized, out RaycastHit hitInfo, difference.magnitude, _groundLayer))
            {
                _desiredPlace.localPosition = _ObjRotation * Vector3.forward * hitInfo.distance;
            }
            else if (_desiredPlace.position.magnitude < _ObjDistance)
            {
                if (Physics.Raycast(_lookTarget.position, difference.normalized, out hitInfo, _ObjDistance, _groundLayer))
                {
                    _desiredPlace.localPosition = _ObjRotation * Vector3.forward * hitInfo.distance;
                }
                else
                {
                    _desiredPlace.localPosition = _originalDesiredPlace;
                }
                
            }
            #endregion

            
        }
        #endregion

        #region Movement
        if (_canJump)
        {
            _isGrounded = Physics.Raycast(transform.position, -transform.up, _groundRayDistance, _groundLayer);
        }

        Move(_moveDir);
        
        #endregion

        Look(_lookDelta);
    }

    #region Move Mode Methods
    public void Move(Vector2 dir)
    {
        if (_lookTarget == null)
        {
            Debug.LogError("You require a Look Target that holds all rotation.");
            return;
        }

        if (dir.x == 0 && dir.y == 0)
        {
            return;
        }

        Vector3 forward = _lookTarget.forward.Flatten();
        Vector3 right = _lookTarget.right.Flatten();

        _trueMoveDir = forward * dir.y + right * dir.x;

        if (_grabbed == null)
        {
            _model.UpdateRotation(Quaternion.LookRotation(_trueMoveDir));
        }

        if (_body.velocity.magnitude < maxSpeed)
        { // if our velocity is not over the max
          // and we are not moving an object or rotating it, move us
            _body.AddForce(_trueMoveDir * speed * (1 - _body.velocity.magnitude / maxSpeed));
        }
    }
    public void Look(Vector2 delta)
    {
        if (_lookTarget == null)
        {
            Debug.LogError("You must set a Look target for camera movement to function.");
            return;
        }

        if (delta.x == 0 && delta.y == 0)
        {
            return;
        }

        _lookTarget.rotation *= Quaternion.AngleAxis(delta.x * lookStrength, Vector3.up);

        _lookTarget.rotation *= Quaternion.AngleAxis(delta.y * lookStrength, Vector3.right);

        //clamp the up/down axis
        Vector3 angles = _lookTarget.localEulerAngles;
        angles.z = 0;

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
    #endregion

    #region Other Mode Methods
    public void MoveObject(Vector3 dir)
    {
        if (_grabbed == null)
        {
            Debug.LogError("This method shouldn't be called if there is no object to move");
            return;
        }

        if (dir.x == 0 && dir.y == 0 && dir.z == 0)
        { // no need to move it if it already doesn't want to move
            return;
        }
        Vector3 forward = _lookTarget.forward.Flatten();
        Vector3 right = _lookTarget.right.Flatten();

        Vector3 desiredDisplacement = forward * dir.z + right * dir.x + Vector3.up * dir.y;
        _desiredPlace.Translate(dir * _moveObjectSpeed * Time.fixedDeltaTime, Space.World);
        _originalDesiredPlace = _desiredPlace.localPosition;
        if (_ObjDistance > _maxObjDistance)
        {
            _ObjDistance = _maxObjDistance;
            _desiredPlace.localPosition = _originalDesiredPlace;
        }
    }
    #endregion

    #region Input Controls
    private delegate void BasicInputController(InputAction.CallbackContext ctx);
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (_IsOnMoveMode)
        {
            _moveDir = ctx.ReadValue<Vector2>();
        }
        else
        {
            _moveDir = Vector2.zero;
        }
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
        if (_IsOnMoveMode)
        {
            bool isOnKeyboard = GameManager.Instance.PlayerInput
            .currentControlScheme == _nameOfKeyboardMouse;

            if (ctx.canceled || !isOnKeyboard || (isOnKeyboard && _lookEnabled))
            {
                _lookDelta = ctx.ReadValue<Vector2>();
            }
        }
        else
        {
            _lookDelta = Vector2.zero;
        }
    }
    public void OnGrab(InputAction.CallbackContext ctx)
    {
        if (_grabbed == null)
        {
            if (GameManager.Instance.PlayerInput.currentControlScheme == _nameOfKeyboardMouse)
            {
                Ray ray = Camera.main.ScreenPointToRay(
                Mouse.current.position.ReadValue());
                
                float maxRayDistance = _maxObjDistance + (transform.position - Camera.main.transform.position).magnitude;
                // this should get a ray that is _maxObjDistance from the player not camera.
                Debug.DrawRay(Camera.main.transform.position, ray.direction * maxRayDistance, Color.red, 10);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, maxRayDistance, _interactableLayer))
                {
                    GrabHitObject(hitInfo);
                }
            }
            else
            { // grab with controller=
                Vector3 halfExtends = Vector2.one * _maxObjDistance * 0.5f;
                
                RaycastHit[] hitInfos = Physics.BoxCastAll(_lookTarget.position, halfExtends, transform.forward, _lookTarget.rotation, _maxObjDistance, _interactableLayer);
                if (hitInfos.Length > 0)
                {
                    int closest = 0;
                    float closestDistance = float.MaxValue;
                    for (int i = 0; i < hitInfos.Length; i++)
                    {
                        if (hitInfos[i].distance < closestDistance)
                        {
                            closest = i;
                            closestDistance = hitInfos[i].distance;
                        }
                    }
                    GrabHitObject(hitInfos[closest]);
                }
            }
        }
        else
        {
            _grabbed.DesiredPlace = null;

            Physics.IgnoreCollision(_playerCollider, _grabbed.gameObject.GetComponent<Collider>(), false);

            _grabbed = null;
            GameManager.ChangeActionMap(_nameOfActionMaps[0]);
            if (!_playerIsLinked)
            {
                LinkControls(true);
            }
        }
    }
    public bool GrabHitObject(RaycastHit hitInfo)
    {
        if (hitInfo.transform.TryGetComponent(out _grabbed))
        {
            _originalDesiredPlace = (transform.position - hitInfo.transform.position);
            if (_ObjDistance > _maxObjDistance)
            {
                _ObjDistance = _maxObjDistance;
            }
            Quaternion rotation = Quaternion.AngleAxis(30, -Vector3.right);
            _desiredPlace.localPosition = rotation * Vector3.forward * _ObjDistance;
            _originalDesiredPlace = _desiredPlace.localPosition;
            _grabbed.DesiredPlace = _desiredPlace;

            Physics.IgnoreCollision(_playerCollider, _grabbed.gameObject.GetComponent<Collider>(), true);

            GameManager.ChangeActionMap(_nameOfActionMaps[1]);
            if (!_manipulateIsLinked)
            {
                LinkControls(true);
            }
            return true;
        }
        return false;
    }
    
    #region Manipulate Specific Methods
    public void OnEnableMoveMode(InputAction.CallbackContext ctx) 
    {
        if (ctx.performed)
        {
            if (_mode == PlayerMode.Move)
            {
                _mode = PlayerMode.MoveObject;
            }
            else if (_mode == PlayerMode.RotateObject)
            {
                _mode = PlayerMode.MoveRotateObject;
            }
        }
        else
        {
            if (_mode == PlayerMode.MoveObject)
            {
                _mode = PlayerMode.Move;
            }
            else if (_mode == PlayerMode.MoveRotateObject)
            {
                _mode = PlayerMode.RotateObject;
            }
            _moveObjectDir = Vector3.zero;
        }
    }
    public void OnObjectMove(InputAction.CallbackContext ctx)
    {
        if (!_IsOnMoveMode && _mode != PlayerMode.RotateObject)
        {
            _moveObjectDir = ctx.ReadValue<Vector3>().normalized;
        }
        else
        {
            _moveObjectDir = Vector3.zero;
        }
    }
    public void OnEnableRotationMode(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (_mode == PlayerMode.Move)
            {
                _mode = PlayerMode.RotateObject;
            }
            else if (_mode == PlayerMode.MoveObject)
            {
                _mode = PlayerMode.MoveRotateObject;
            }
        }
        else
        {
            if (_mode == PlayerMode.RotateObject)
            {
                _mode = PlayerMode.Move;
            }
            else if (_mode == PlayerMode.MoveRotateObject)
            {
                _mode = PlayerMode.MoveObject;
            }
        }
        
    }
    #endregion

    #endregion

    private void OnDrawGizmos()
    {
        if (_showGroundRay && _canJump)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, -transform.up * _groundRayDistance);
        }
        if (_showGrabRay && _grabbed != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_lookTarget.position, _desiredPlace.position - _lookTarget.position);
        }
    }
}
