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
    
    #region Serialized Properties
    // general variables
    private SerializedProperty _pBody;
    private SerializedProperty _pLinked;
    
    // movement variables
    private SerializedProperty _pMoveDir;
    private SerializedProperty _pCanJump;
    private SerializedProperty _pIsGrounded;
    private SerializedProperty _pGroundRayDistance;
    private SerializedProperty _pGroundLayer;

    // debug variables
    private SerializedProperty _pShowGroundCast;
    #endregion

    private void OnEnable()
    {
        // for public members
        _player = (PlayerController)target;

        // for general private variables
        _pBody = serializedObject.FindProperty("_body");
        _pLinked = serializedObject.FindProperty("_isLinked");

        // for movement-based private variables
        _pMoveDir           = serializedObject.FindProperty("_moveDir");
        _pCanJump           = serializedObject.FindProperty("_canJump");
        _pIsGrounded        = serializedObject.FindProperty("_isGrounded");
        _pGroundRayDistance = serializedObject.FindProperty("_groundRayDistance");
        _pGroundLayer       = serializedObject.FindProperty("_groundLayer");

        // for debug private variables
        _pShowGroundCast = serializedObject.FindProperty("_showGroundRay");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_pBody);
        EditorGUILayout.Space();
        _showMovementInfo = EditorGUILayout.BeginFoldoutHeaderGroup(_showMovementInfo, "Movement Settings");
        if (_showMovementInfo)
        {
            EditorGUI.indentLevel++;
            _player.speed    = EditorGUILayout.FloatField("Player Speed", _player.speed);
            _player.maxSpeed = EditorGUILayout.FloatField("Max Speed", _player.maxSpeed);
            EditorGUILayout.PropertyField(_pCanJump);
            if (_pCanJump.boolValue)
            {
                EditorGUI.indentLevel++;
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