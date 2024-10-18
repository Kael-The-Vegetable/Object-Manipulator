#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    private PlayerController _player;

    private bool _showMovementInfo;
    private bool _showDebugInfo;
    private bool _showDebugControls;
    private bool _showInputControls;
    private bool _showLookControls;
    private bool _showGrabControls;
    
    #region Serialized Properties
    // general variables
    private SerializedProperty _pBody;
    
    // link variables
    private SerializedProperty _pEvents;
    private SerializedProperty _pKeyboardName;

    // movement variables
    private SerializedProperty _pMoveDir;
    private SerializedProperty _pTrueMoveDir;
    private SerializedProperty _pCanJump;
    private SerializedProperty _pIsGrounded;
    private SerializedProperty _pGroundRayDistance;
    private SerializedProperty _pGroundLayer;

    // look variables
    private SerializedProperty _pLookTarget;
    private SerializedProperty _pLookUp;
    private SerializedProperty _pLookDown;
    private SerializedProperty _pModel;

    // grab variables
    private SerializedProperty _pInteractLayer;
    private SerializedProperty _pObjectDistance;
    private SerializedProperty _pMaxDistance;
    private SerializedProperty _pDesiredPlace;

    // debug variables
    private SerializedProperty _pShowGroundCast;
    private SerializedProperty _pShowGrabCast;
    #endregion

    private void OnEnable()
    {
        // for public members
        _player = (PlayerController)target;

        // for general private variables
        _pBody   = serializedObject.FindProperty("_body");

        // for input private variables
        _pEvents       = serializedObject.FindProperty("_events");
        _pKeyboardName = serializedObject.FindProperty("_nameOfKeyboardMouse");

        // for movement-based private variables
        _pMoveDir           = serializedObject.FindProperty("_moveDir");
        _pTrueMoveDir       = serializedObject.FindProperty("_trueMoveDir");
        _pCanJump           = serializedObject.FindProperty("_canJump");
        _pIsGrounded        = serializedObject.FindProperty("_isGrounded");
        _pGroundRayDistance = serializedObject.FindProperty("_groundRayDistance");
        _pGroundLayer       = serializedObject.FindProperty("_groundLayer");

        // for look private variables
        _pLookTarget = serializedObject.FindProperty("_lookTarget");
        _pLookUp     = serializedObject.FindProperty("_maxLookUpAngle");
        _pLookDown   = serializedObject.FindProperty("_maxLookDownAngle");
        _pModel      = serializedObject.FindProperty("_model");

        // for grab private variables
        _pInteractLayer  = serializedObject.FindProperty("_interactableLayer");
        _pObjectDistance = serializedObject.FindProperty("_objDistance");
        _pMaxDistance    = serializedObject.FindProperty("_maxObjDistance");
        _pDesiredPlace   = serializedObject.FindProperty("_desiredPlace");

        // for debug private variables
        _pShowGroundCast = serializedObject.FindProperty("_showGroundRay");
        _pShowGrabCast   = serializedObject.FindProperty("_showGrabRay");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_pBody);
        if (_pBody.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Warning! Make sure the Player Controller has a Rigidbody attached to the same object or is put here.", MessageType.Warning);
        }
        EditorGUILayout.Separator();

        #region Input
        _showInputControls = EditorGUILayout.Foldout(_showInputControls, "Input Settings");
        if (_showInputControls)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_pEvents);
            EditorGUILayout.PropertyField(_pKeyboardName);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Separator();
        #endregion

        #region Movement
        _showMovementInfo = EditorGUILayout.BeginFoldoutHeaderGroup(_showMovementInfo, "Movement Settings");
        if (_showMovementInfo)
        {
            EditorGUI.indentLevel++;
            _player.speed    = EditorGUILayout.FloatField("Player Speed", _player.speed);
            _player.maxSpeed = EditorGUILayout.FloatField("Max Speed", _player.maxSpeed);
            
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_pCanJump);
            if (_pCanJump.boolValue)
            {
                EditorGUI.indentLevel++;
                _player.jumpStrength = EditorGUILayout.FloatField("Jump Strength", _player.jumpStrength);
                EditorGUILayout.PropertyField(_pGroundRayDistance);
                EditorGUILayout.PropertyField(_pGroundLayer);
                if (_pGroundLayer.intValue == 0)
                {
                    EditorGUILayout.HelpBox("Warning! The ground cast will see nothing as there is no Ground Layer set.", MessageType.Warning);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Separator();
        #endregion

        #region Looking
        _showLookControls = EditorGUILayout.BeginFoldoutHeaderGroup(_showLookControls, "Look Settings");
        if (_showLookControls)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_pLookTarget);
            if (_player.LookTarget != null)
            {
                _player.lookStrength = EditorGUILayout.FloatField("Look Strength", _player.lookStrength);
                EditorGUILayout.PropertyField(_pLookUp);
                EditorGUILayout.PropertyField(_pLookDown);
                EditorGUILayout.PropertyField(_pModel);
                if (_pModel.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("You should have a Model that is separate from the player so that the model can rotate independantly of the camera.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("You require a GameObject to be assigned to this variable for looking functionality.", MessageType.Error);
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Separator();
        #endregion

        #region Grabbing
        _showGrabControls = EditorGUILayout.BeginFoldoutHeaderGroup(_showGrabControls, "Grab Object Settings");
        if (_showGrabControls)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_pInteractLayer);
            EditorGUILayout.PropertyField(_pDesiredPlace);
            if (_pDesiredPlace.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("You require a GameObject that is where grabbed objects will try to go.", MessageType.Error);
            }
            else
            {
                EditorGUILayout.PropertyField(_pMaxDistance);
                EditorGUILayout.PropertyField(_pObjectDistance);
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Separator();
        #endregion

        #region Debug Controls
        _showDebugControls = EditorGUILayout.BeginFoldoutHeaderGroup(_showDebugControls, "Debug Settings");
        if (_showDebugControls)
        {
            EditorGUI.indentLevel++;
            if (_pCanJump.boolValue)
            {
                EditorGUILayout.PropertyField(_pShowGroundCast);
            }
            EditorGUILayout.PropertyField(_pShowGrabCast);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Separator();
        #endregion

        #region Debug Info
        _showDebugInfo = EditorGUILayout.BeginFoldoutHeaderGroup(_showDebugInfo, "Debug Info");
        if (_showDebugInfo)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_pMoveDir);
            EditorGUILayout.PropertyField(_pTrueMoveDir);
            if (_pCanJump.boolValue)
            {
                EditorGUILayout.PropertyField(_pIsGrounded);
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        // apply changes
        serializedObject.ApplyModifiedProperties();
    }
}
#endif