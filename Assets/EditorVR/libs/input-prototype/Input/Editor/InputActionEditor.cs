using UnityEngine;
using UnityEngine.InputNew;
using UnityEditor;

[CustomEditor(typeof(InputAction))]
public class InputActionEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		EditorGUILayout.HelpBox("Select the main Action Map asset to edit actions.", MessageType.Info);
	}
}
