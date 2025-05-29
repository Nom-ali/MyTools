using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreateBorderCollider))]
public class CreateBorderColliderEditor : Editor
{
    SerializedProperty borderCollider;
    SerializedProperty addOffset;
    SerializedProperty topOffset;
    SerializedProperty bottomOffset;
    SerializedProperty leftOffset;
    SerializedProperty rightOffset;
    SerializedProperty colliderThickness;
    SerializedProperty targetCamera;

    void OnEnable()
    {
        borderCollider = serializedObject.FindProperty("borderCollider");
        addOffset = serializedObject.FindProperty("addOffset");
        topOffset = serializedObject.FindProperty("topOffset");
        bottomOffset = serializedObject.FindProperty("bottomOffset");
        leftOffset = serializedObject.FindProperty("leftOffset");
        rightOffset = serializedObject.FindProperty("rightOffset");
        colliderThickness = serializedObject.FindProperty("colliderThickness");
        targetCamera = serializedObject.FindProperty("targetCamera");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(targetCamera);
        EditorGUILayout.PropertyField(borderCollider);

        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(colliderThickness);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(addOffset);

        var offsetFlags = (CreateBorderCollider.BorderColliderType)addOffset.intValue;

        EditorGUI.indentLevel++;
        if (offsetFlags.HasFlag(CreateBorderCollider.BorderColliderType.Top))
            EditorGUILayout.PropertyField(topOffset);

        if (offsetFlags.HasFlag(CreateBorderCollider.BorderColliderType.Bottom))
            EditorGUILayout.PropertyField(bottomOffset);

        if (offsetFlags.HasFlag(CreateBorderCollider.BorderColliderType.Left))
            EditorGUILayout.PropertyField(leftOffset);

        if (offsetFlags.HasFlag(CreateBorderCollider.BorderColliderType.Right))
            EditorGUILayout.PropertyField(rightOffset);
        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }
}
