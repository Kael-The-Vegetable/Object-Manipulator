#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    private PlayerController _player;

    private bool _showMovementInfo = true;
    private bool _showDebugInfo = true;
    private bool _showDebugControls = true;
    private bool _showInputControls = true;
    private bool _showLookControls = true;
    
    #region Serialized Properties
    // general variables
    private SerializedProperty _pBody;
    private SerializedProperty _pLinked;
    
    private SerializedProperty _pEvents;
    private SerializedProperty _pKeyboardName;

    // movement variables
    private SerializedProperty _pMoveDir;
    private SerializedProperty _pCanJump;
    private SerializedProperty _pIsGrounded;
    private SerializedProperty _pGroundRayDistance;
    private SerializedProperty _pGroundLayer;

    // look variables
    private SerializedProperty _pLookTarget;
    private SerializedProperty _pLookUp;
    private SerializedProperty _pLookDown;

    // debug variables
    private SerializedProperty _pShowGroundCast;
    #endregion

    private void OnEnable()
    {
        // for public members
        _player = (PlayerController)target;

        // for general private variables
        _pBody   = serializedObject.FindProperty("_body");
        _pLinked = serializedObject.FindProperty("_isLinked");

        // for input private variables
        _pEvents       = serializedObject.FindProperty("_events");
        _pKeyboardName = serializedObject.FindProperty("_nameOfKeyboardMouse");

        // for movement-based private variables
        _pMoveDir           = serializedObject.FindProperty("_moveDir");
        _pCanJump           = serializedObject.FindProperty("_canJump");
        _pIsGrounded        = serializedObject.FindProperty("_isGrounded");
        _pGroundRayDistance = serializedObject.FindProperty("_groundRayDistance");
        _pGroundLayer       = serializedObject.FindProperty("_groundLayer");

        // for look private variables
        _pLookTarget = serializedObject.FindProperty("_lookTarget");
        _pLookUp     = serializedObject.FindProperty("_maxLookUpAngle");
        _pLookDown   = serializedObject.FindProperty("_maxLookDownAngle");

        // for debug private variables
        _pShowGroundCast = serializedObject.FindProperty("_showGroundRay");
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

        _showInputControls = EditorGUILayout.Foldout(_showInputControls, "Input Settings");
        if (_showInputControls)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_pEvents);
            EditorGUILayout.PropertyField(_pKeyboardName);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Separator();

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
            }
            else
            {
                EditorGUILayout.HelpBox("You require a GameObject to be assigned to this variable for looking functionality.", MessageType.Error);
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Separator();

        _showDebugControls = EditorGUILayout.BeginFoldoutHeaderGroup(_showDebugControls, "Debug Settings");
        if (_showDebugControls)
        {
            EditorGUI.indentLevel++;
            if (_pCanJump.boolValue)
            {
                EditorGUILayout.PropertyField(_pShowGroundCast);
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Separator();

        _showDebugInfo = EditorGUILayout.BeginFoldoutHeaderGroup(_showDebugInfo, "Debug Info");
        if (_showDebugInfo)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_pLinked);
            EditorGUILayout.PropertyField(_pMoveDir);
            if (_pCanJump.boolValue)
            {
                EditorGUILayout.PropertyField(_pIsGrounded);
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        // apply changes
        serializedObject.ApplyModifiedProperties();
    }
}
#endif