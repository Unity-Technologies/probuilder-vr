using UnityEngine;
using UnityEngine.InputNew;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(ActionMapSlot), true)]
public class ActionMapSlotDrawer : PropertyDrawer
{
	const int k_ActiveWidth = 13;
	const int k_BlockWidth = 50;

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		position = EditorGUI.PrefixLabel(position, label);

		EditorGUIUtility.labelWidth = k_BlockWidth - k_ActiveWidth;
		EditorGUI.indentLevel = 0;

		Rect pos = position;
		pos.width = k_ActiveWidth;
		EditorGUI.PropertyField(pos, property.FindPropertyRelative("active"), GUIContent.none);

		pos = position;
		pos.xMin += k_ActiveWidth + 4;
		pos.xMax -= k_BlockWidth + 10;
		EditorGUI.PropertyField(pos, property.FindPropertyRelative("actionMap"), GUIContent.none);

		pos = position;
		pos.xMin = pos.xMax - k_BlockWidth;
		EditorGUI.PropertyField(pos, property.FindPropertyRelative("blockSubsequent"), new GUIContent("Block"));
	}
}
