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
    
    #region Serialized Properties
    private SerializedProperty _pBody;
    private SerializedProperty _pLinked;
    private SerializedProperty _pMoveDir;
    #endregion

    private void OnEnable()
    {
        _player = (PlayerController)target;
        _pBody = serializedObject.FindProperty("_body");
        _pLinked = serializedObject.FindProperty("_isLinked");
        _pMoveDir = serializedObject.FindProperty("_moveDir");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_pBody);
        EditorGUILayout.Space();
        _showMovementInfo = EditorGUILayout.BeginFoldoutHeaderGroup(_showMovementInfo, "Movement Info");
        if (_showMovementInfo)
        {
            EditorGUI.indentLevel++;
            _player.speed    = EditorGUILayout.FloatField("Player Speed", _player.speed);
            _player.maxSpeed = EditorGUILayout.FloatField("Max Speed", _player.maxSpeed);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        _showDebugInfo = EditorGUILayout.BeginFoldoutHeaderGroup(_showDebugInfo, "Debug Info");
        if (_showDebugInfo)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_pLinked);
            EditorGUILayout.PropertyField(_pMoveDir);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        // apply changes
        serializedObject.ApplyModifiedProperties();
    }
}
#endif