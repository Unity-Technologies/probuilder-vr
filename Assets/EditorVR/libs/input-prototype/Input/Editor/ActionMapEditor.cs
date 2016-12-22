using UnityEngine;
using UnityEngine.InputNew;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using Array = System.Array;

[CustomEditor(typeof(ActionMap))]
public class ActionMapEditor : Editor
{
	static class Styles
	{
		public static GUIContent iconToolbarPlus =	EditorGUIUtility.IconContent("Toolbar Plus", "Add to list");
		public static GUIContent iconToolbarMinus =	EditorGUIUtility.IconContent("Toolbar Minus", "Remove from list");
		public static GUIContent iconToolbarPlusMore =	EditorGUIUtility.IconContent("Toolbar Plus More", "Choose to add to list");
	}
	
	ActionMap m_ActionMapEditCopy;
	
	int m_SelectedScheme = 0;
	int m_SelectedDeviceIndex = 0;
	[System.NonSerialized]
	InputAction m_SelectedAction = null;
	List<string> m_PropertyNames = new List<string>();
	HashSet<string> m_PropertyBlacklist  = new HashSet<string>();
	Dictionary<string, string> m_PropertyErrors = new Dictionary<string, string>();
	InputControlDescriptor m_SelectedSource = null;
	ButtonAxisSource m_SelectedButtonAxisSource = null;
	Dictionary<Type, string[]> s_ControlTypeSourceNameArrays = new Dictionary<Type, string[]>();
	bool m_Modified = false;
	
	int selectedScheme
	{
		get { return m_SelectedScheme; }
		set
		{
			if (m_SelectedScheme == value)
				return;
			m_SelectedScheme = value;
		}
	}
	
	InputAction selectedAction
	{
		get { return m_SelectedAction; }
		set
		{
			if (m_SelectedAction == value)
				return;
			m_SelectedAction = value;
		}
	}
	
	void OnEnable()
	{
		Revert();
		RefreshPropertyNames();
		CalculateBlackList();
	}
	
	public virtual void OnDisable ()
	{
		// When destroying the editor check if we have any unapplied modifications and ask about applying them.
		if (m_Modified)
		{
			string dialogText = "Unapplied changes to ActionMap '" + serializedObject.targetObject.name + "'.";
			if (EditorUtility.DisplayDialog ("Unapplied changes", dialogText, "Apply", "Revert"))
				Apply();
		}
	}
	
	void Apply()
	{
		EditorGUIUtility.keyboardControl = 0;

		m_ActionMapEditCopy.name = target.name;
		SerializedObject temp = new SerializedObject(m_ActionMapEditCopy);
		temp.Update();
		SerializedProperty prop = temp.GetIterator();
		while (prop.Next(true))
			serializedObject.CopyFromSerializedProperty(prop);

		// Make sure references in control schemes to action map itself are stored correctly.
		for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
			serializedObject.FindProperty("m_ControlSchemes")
				.GetArrayElementAtIndex(i)
				.FindPropertyRelative("m_ActionMap").objectReferenceValue = target;
		
		serializedObject.ApplyModifiedProperties();

		var existingAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target));

		// Add action sub-assets.
		ActionMap actionMap = (ActionMap)target;
		for (int i = 0; i < m_ActionMapEditCopy.actions.Count; i++)
		{
			InputAction action = m_ActionMapEditCopy.actions[i];
			action.actionMap = actionMap;
			action.actionIndex = i;
			if (existingAssets.Contains(action))
				continue;
			AssetDatabase.AddObjectToAsset(action, target);
		}

		m_Modified = false;
		// Reimporting is needed in order for the sub-assets to show up.
		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
		
		UpdateActionMapScript();
	}
	
	void Revert ()
	{
		EditorGUIUtility.keyboardControl = 0;
		
		ActionMap original = (ActionMap)serializedObject.targetObject;
		m_ActionMapEditCopy = Instantiate<ActionMap>(original);
		m_ActionMapEditCopy.name = original.name;
		
		m_Modified = false;
	}

	void SetActionMapDirty()
	{
		EditorUtility.SetDirty(m_ActionMapEditCopy);
		m_Modified = true;
	}
	
	void RefreshPropertyNames()
	{
		// Calculate property names.
		m_PropertyNames.Clear();
		for (int i = 0; i < m_ActionMapEditCopy.actions.Count; i++)
			m_PropertyNames.Add(GetCamelCaseString(m_ActionMapEditCopy.actions[i].name, false));
		
		// Calculate duplicates.
		HashSet<string> duplicates = new HashSet<string>(m_PropertyNames.GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.Key));
		
		// Calculate errors.
		m_PropertyErrors.Clear();
		for (int i = 0; i < m_PropertyNames.Count; i++)
		{
			string name = m_PropertyNames[i];
			if (m_PropertyBlacklist.Contains(name))
				m_PropertyErrors[name] = "Invalid action name: "+name+".";
			else if (duplicates.Contains(name))
				m_PropertyErrors[name] = "Duplicate action name: "+name+".";
		}
	}
	
	void CalculateBlackList()
	{
		m_PropertyBlacklist = new HashSet<string>(typeof(ActionMapInput).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Select(e => e.Name));
	}
	
	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();

		DrawCustomNamespaceGUI();

		EditorGUILayout.Space();
		DrawControlSchemeSelection();

		if (m_ActionMapEditCopy.controlSchemes.Count > 0)
		{
			EditorGUILayout.Space();
			DrawControlSchemeGUI();

			EditorGUILayout.Space();
			DrawActionList();
			
			if (selectedAction != null)
			{
				EditorGUILayout.Space();
				DrawActionGUI();
			}
			
			EditorGUILayout.Space();
		}

		if (EditorGUI.EndChangeCheck())
		{
			SetActionMapDirty();
		}

		ApplyRevertGUI();
	}

	void DrawCustomNamespaceGUI()
	{
		EditorGUI.BeginChangeCheck();
		string customNamespace = EditorGUILayout.TextField("Custom Namespace", m_ActionMapEditCopy.customNamespace);
		if (EditorGUI.EndChangeCheck())
			m_ActionMapEditCopy.customNamespace = customNamespace;
	}

	void DrawControlSchemeSelection()
	{
		if (selectedScheme >= m_ActionMapEditCopy.controlSchemes.Count)
			selectedScheme = m_ActionMapEditCopy.controlSchemes.Count - 1;		

		// Show schemes
		EditorGUILayout.LabelField("Control Schemes");

		EditorGUIUtility.GetControlID(FocusType.Passive);
		EditorGUILayout.BeginVertical("Box");
		for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
		{
			Rect rect = EditorGUILayout.GetControlRect();
			
			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				EditorGUIUtility.keyboardControl = 0;
				selectedScheme = i;
				Event.current.Use();
			}
			
			if (selectedScheme == i)
				GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
			
			EditorGUI.LabelField(rect, m_ActionMapEditCopy.controlSchemes[i].name);
		}
		EditorGUILayout.EndVertical();
		
		// Control scheme remove and add buttons
		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Space(15 * EditorGUI.indentLevel);

			if (GUILayout.Button(Styles.iconToolbarMinus, GUIStyle.none))
				RemoveControlScheme();
			
			if (GUILayout.Button(Styles.iconToolbarPlus, GUIStyle.none))
				AddControlScheme();

			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
	}

	void AddControlScheme()
	{
		var controlScheme = new ControlScheme("New Control Scheme", m_ActionMapEditCopy);

		for (int i = 0; i < m_ActionMapEditCopy.actions.Count; i++)
			controlScheme.bindings.Add(new ControlBinding());

		m_ActionMapEditCopy.controlSchemes.Add(controlScheme);

		selectedScheme = m_ActionMapEditCopy.controlSchemes.Count - 1;
	}

	void RemoveControlScheme()
	{
		m_ActionMapEditCopy.controlSchemes.RemoveAt(selectedScheme);
		if (selectedScheme >= m_ActionMapEditCopy.controlSchemes.Count)
			selectedScheme = m_ActionMapEditCopy.controlSchemes.Count - 1;
	}

    void DrawControlSchemeGUI()
    {
        ControlScheme scheme = m_ActionMapEditCopy.controlSchemes[selectedScheme];

        EditorGUI.BeginChangeCheck();
        string schemeName = EditorGUILayout.TextField("Control Scheme Name", scheme.name);
        if (EditorGUI.EndChangeCheck())
            scheme.name = schemeName;

		for (int i = 0; i < scheme.deviceSlots.Count; i++)
		{
			var deviceSlot = scheme.deviceSlots[i];

			Rect rect = EditorGUILayout.GetControlRect();
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                m_SelectedDeviceIndex = i;
                Repaint();
            }
            if (m_SelectedDeviceIndex == i)
                GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);

            string[] tagNames = null;
            Vector2 tagMaxSize = Vector2.zero;
			if (deviceSlot.type != null && deviceSlot.type.value != null)
			{
				tagNames = InputDeviceUtility.GetDeviceTags(deviceSlot.type.value);
                if (tagNames != null)
                {
                    GUIContent content = new GUIContent();
                    for (var j = 0; j < tagNames.Length; j++)
                    {
                        content.text = tagNames[j];
                        Vector2 size = EditorStyles.popup.CalcSize(content);
                        tagMaxSize = Vector2.Max(size, tagMaxSize);
                    }
                }               
            }

            rect.width -= tagMaxSize.x; // Adjust width to leave room for tag
            EditorGUI.BeginChangeCheck();
			Type t = TypeGUI.TypeField(rect, new GUIContent("Device Type"), typeof(InputDevice), deviceSlot.type);
			if (EditorGUI.EndChangeCheck())
				deviceSlot.type = t;
            if (tagNames != null)
            {
                EditorGUI.BeginChangeCheck();
				// We want to have the ability to unset a tag after specifying one, so add an "Any" option
	            var popupTags = new string[tagNames.Length + 1];
				popupTags[0] = "Any";
				tagNames.CopyTo(popupTags, 1);
				int tagIndex = deviceSlot.tagIndex + 1;
                rect.x += rect.width;
                rect.width = tagMaxSize.x;
                tagIndex = EditorGUI.Popup(
                    rect,
                    tagIndex,
                    popupTags);
                if (EditorGUI.EndChangeCheck())
					deviceSlot.tagIndex = tagIndex - 1;
            }
        }

        // Device remove and add buttons
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space(15 * EditorGUI.indentLevel);

            if (GUILayout.Button(Styles.iconToolbarMinus, GUIStyle.none))
                RemoveDevice();

            if (GUILayout.Button(Styles.iconToolbarPlus, GUIStyle.none))
                AddDevice();

            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();

        // Pad this area with spacing so all control schemes use same heights,
        // and the actions table below doesn't move when switching control scheme.
        int maxDevices = 0;
        for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
			maxDevices = Mathf.Max(maxDevices, m_ActionMapEditCopy.controlSchemes[i].deviceSlots.Count);
		int extraLines = maxDevices - scheme.deviceSlots.Count;
        EditorGUILayout.GetControlRect(true, extraLines * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing));
    }

    void AddDevice()
	{
		ControlScheme scheme = m_ActionMapEditCopy.controlSchemes[selectedScheme];
		var deviceSlot = new DeviceSlot()
		{
			key = GetNextDeviceKey()
		};
		scheme.deviceSlots.Add(deviceSlot);
	}

	void RemoveDevice()
	{
		ControlScheme scheme = m_ActionMapEditCopy.controlSchemes[selectedScheme];
		if (m_SelectedDeviceIndex >= 0 && m_SelectedDeviceIndex < scheme.deviceSlots.Count)
		{
			scheme.deviceSlots.RemoveAt(m_SelectedDeviceIndex);
		}
	}

	int GetNextDeviceKey()
	{
		int key = 0;
		for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
		{
			var deviceSlots = m_ActionMapEditCopy.controlSchemes[i].deviceSlots;
			for (int j = 0; j < deviceSlots.Count; j++)
			{
				key = Mathf.Max(deviceSlots[j].key, key);
			}
		}

		return key + 1;
	}

	void DrawActionList()
	{
		// Show actions
		EditorGUILayout.LabelField("Actions", m_ActionMapEditCopy.controlSchemes[selectedScheme].name + " Bindings");
		EditorGUILayout.BeginVertical("Box");
		{
			foreach (var action in m_ActionMapEditCopy.actions)
			{
				DrawActionRow(action, selectedScheme);
			}
			if (m_ActionMapEditCopy.actions.Count == 0)
				EditorGUILayout.GetControlRect();
		}
		EditorGUILayout.EndVertical();
		
		// Action remove and add buttons
		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Space(15 * EditorGUI.indentLevel);

			if (GUILayout.Button(Styles.iconToolbarMinus, GUIStyle.none))
				RemoveAction();
			
			if (GUILayout.Button(Styles.iconToolbarPlus, GUIStyle.none))
				AddAction();
			
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
	}

	void AddAction()
	{
		var action = ScriptableObject.CreateInstance<InputAction>();
		action.name = "New Control";
		m_ActionMapEditCopy.actions.Add(action);
		for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
			m_ActionMapEditCopy.controlSchemes[i].bindings.Add(new ControlBinding());
		
		selectedAction = m_ActionMapEditCopy.actions[m_ActionMapEditCopy.actions.Count - 1];
		
		RefreshPropertyNames();
	}

	void RemoveAction()
	{
		int actionIndex = m_ActionMapEditCopy.actions.IndexOf(selectedAction);
		m_ActionMapEditCopy.actions.RemoveAt(actionIndex);
		for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
			m_ActionMapEditCopy.controlSchemes[i].bindings.RemoveAt(actionIndex);
		ScriptableObject.DestroyImmediate(selectedAction, true);
		
		if (m_ActionMapEditCopy.actions.Count == 0)
			selectedAction = null;
		else
			selectedAction = m_ActionMapEditCopy.actions[Mathf.Min(actionIndex, m_ActionMapEditCopy.actions.Count - 1)];
		
		RefreshPropertyNames();
	}
	
	void ApplyRevertGUI()
	{
		bool valid = true;
		if (m_PropertyErrors.Count > 0)
		{
			valid = false;
			EditorGUILayout.HelpBox(string.Join("\n", m_PropertyErrors.Values.ToArray()), MessageType.Error);
		}
		
		EditorGUI.BeginDisabledGroup(!m_Modified);
		
		GUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("Revert"))
				Revert();
			
			EditorGUI.BeginDisabledGroup(!valid);
			if (GUILayout.Button("Apply"))
				Apply();
			EditorGUI.EndDisabledGroup();
		}
		GUILayout.EndHorizontal();
		
		EditorGUI.EndDisabledGroup();
	}
	
	void DrawActionRow(InputAction action, int selectedScheme)
	{
		int actionIndex = m_ActionMapEditCopy.actions.IndexOf(action);
		
		int sourceCount = 0;
		for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
		{
			ControlBinding schemeBinding = m_ActionMapEditCopy.controlSchemes[i].bindings[actionIndex];
			int count = schemeBinding.sources.Count + schemeBinding.buttonAxisSources.Count;
			sourceCount = Mathf.Max(sourceCount, count);
		}
		int lines = Mathf.Max(1, sourceCount);
		
		float height = EditorGUIUtility.singleLineHeight * lines + EditorGUIUtility.standardVerticalSpacing * (lines - 1) + 8;
		Rect totalRect = GUILayoutUtility.GetRect(1, height);
		
		Rect baseRect = totalRect;
		baseRect.yMin += 4;
		baseRect.yMax -= 4;
		
		if (selectedAction == action)
			GUI.DrawTexture(totalRect, EditorGUIUtility.whiteTexture);
		
		// Show control fields
		
		Rect rect = baseRect;
		rect.height = EditorGUIUtility.singleLineHeight;
		rect.width = EditorGUIUtility.labelWidth - 4;
		
		EditorGUI.LabelField(rect, action.controlData.name);
		
		// Show binding fields

		ControlBinding binding = m_ActionMapEditCopy.controlSchemes[selectedScheme].bindings[actionIndex];
		if (binding != null)
		{
			rect = baseRect;
			rect.height = EditorGUIUtility.singleLineHeight;
			rect.xMin += EditorGUIUtility.labelWidth;
			
			if (binding.primaryIsButtonAxis)
			{
				DrawButtonAxisSources(ref rect, binding);
				DrawSources(ref rect, binding);
			}
			else
			{
				DrawSources(ref rect, binding);
				DrawButtonAxisSources(ref rect, binding);
			}
		}
		
		if (Event.current.type == EventType.MouseDown && totalRect.Contains(Event.current.mousePosition))
		{
			EditorGUIUtility.keyboardControl = 0;
			selectedAction = action;
			Event.current.Use();
		}
	}
	
	void DrawSources(ref Rect rect, ControlBinding binding)
	{
		for (int i = 0; i < binding.sources.Count; i++)
		{
			DrawSourceSummary(rect, binding.sources[i]);
			rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
		}
	}
	
	void DrawButtonAxisSources(ref Rect rect, ControlBinding binding)
	{
		for (int i = 0; i < binding.buttonAxisSources.Count; i++)
		{
			DrawButtonAxisSourceSummary(rect, binding.buttonAxisSources[i]);
			rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
		}
	}
	
	void DrawSourceSummary(Rect rect, InputControlDescriptor source)
	{
		EditorGUI.LabelField(rect, GetSourceString(source));
	}
	
	void DrawButtonAxisSourceSummary(Rect rect, ButtonAxisSource source)
	{
		ControlScheme scheme = m_ActionMapEditCopy.controlSchemes[selectedScheme];
		var negativeDeviceSlot = scheme.GetDeviceSlot(source.negative.deviceKey);
		var positiveDeviceSlot = scheme.GetDeviceSlot(source.positive.deviceKey);
		if ((Type)(negativeDeviceSlot.type) == (Type)(positiveDeviceSlot.type))
			EditorGUI.LabelField(rect,
				string.Format("{0} {1} {2} & {3}",
					InputDeviceUtility.GetDeviceNameWithTag(negativeDeviceSlot),
					GetDeviceInstanceString(scheme, negativeDeviceSlot),
					InputDeviceUtility.GetDeviceControlName(negativeDeviceSlot, source.negative),
					InputDeviceUtility.GetDeviceControlName(positiveDeviceSlot, source.positive)
				)
			);
		else
			EditorGUI.LabelField(rect, string.Format("{0} & {1}", GetSourceString(source.negative), GetSourceString(source.positive)));
	}

	string GetDeviceInstanceString(ControlScheme scheme, DeviceSlot deviceSlot)
	{
		int instance = 0;
		int totalInstances = 0;
		if (deviceSlot != null)
		{			
			var deviceSlots = scheme.deviceSlots;		
			for (int i = 0; i < deviceSlots.Count; i++)
			{
				var ds = deviceSlots[i];
				if ((Type)deviceSlot.type == (Type)ds.type && deviceSlot.tagIndex == ds.tagIndex)
				{
					totalInstances++;
					if (deviceSlot == ds)
						instance = totalInstances;
				}
			}
		}

		if (totalInstances > 1)
			return string.Format("#{0}", instance);
		else
			return string.Empty;
	}
	
	string GetSourceString(InputControlDescriptor source)
	{
		ControlScheme scheme = m_ActionMapEditCopy.controlSchemes[selectedScheme];
		var deviceSlot = scheme.GetDeviceSlot(source.deviceKey);
		return string.Format("{0} {1} {2}", InputDeviceUtility.GetDeviceNameWithTag(deviceSlot), GetDeviceInstanceString(scheme, deviceSlot), InputDeviceUtility.GetDeviceControlName(deviceSlot, source));
	}
	
	void UpdateActionMapScript () {
		ActionMap original = (ActionMap)serializedObject.targetObject;
		string className = GetCamelCaseString(original.name, true);
		StringBuilder str = new StringBuilder();

		string actionMapNamespace = m_ActionMapEditCopy.customNamespace;
		if (string.IsNullOrEmpty(actionMapNamespace))
			actionMapNamespace = ActionMap.kDefaultNamespace;

		str.AppendFormat(@"using UnityEngine;
using UnityEngine.InputNew;

// GENERATED FILE - DO NOT EDIT MANUALLY
namespace {0}
{{
	public class {1} : ActionMapInput {{
		public {1} (ActionMap actionMap) : base (actionMap) {{ }}
		
", actionMapNamespace, className);
		
		for (int i = 0; i < m_ActionMapEditCopy.actions.Count; i++)
		{
			Type controlType = m_ActionMapEditCopy.actions[i].controlData.controlType;
			string typeStr = controlType.Name;
			str.AppendFormat("		public {2} @{0} {{ get {{ return ({2})this[{1}]; }} }}\n", GetCamelCaseString(m_ActionMapEditCopy.actions[i].name, false), i, typeStr);
		}
		
		str.AppendLine(@"	}
}");
		
		string path = AssetDatabase.GetAssetPath(original);
		path = path.Substring(0, path.Length - Path.GetExtension(path).Length) + ".cs";
		File.WriteAllText(path, str.ToString());
		AssetDatabase.ImportAsset(path);

		original.SetMapTypeName(className+", "+"Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
	}
	
	string GetCamelCaseString(string input, bool capitalFirstLetter)
	{
		string output = string.Empty;
		bool capitalize = capitalFirstLetter;
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			if (c == ' ')
			{
				capitalize = true;
				continue;
			}
			if (char.IsLetter(c))
			{
				if (capitalize)
					output += char.ToUpper(c);
				else if (output.Length == 0)
					output += char.ToLower(c);
				else
					output += c;
				capitalize = false;
				continue;
			}
			if (char.IsDigit(c))
			{
				if (output.Length > 0)
				{
					output += c;
					capitalize = false;
				}
				continue;
			}
			if (c == '_')
			{
				output += c;
				capitalize = true;
				continue;
			}
		}
		return output;
	}
	
	string[] GetSourceControlNames(Type controlType)
	{
		string[] names = null;
		if (controlType != null && !s_ControlTypeSourceNameArrays.TryGetValue(controlType, out names))
		{
			InputControl control = (InputControl)Activator.CreateInstance(controlType, 0, null);
			names = control.sourceControlNames;
			s_ControlTypeSourceNameArrays[controlType] = names;
		}
		return names;
	}

	void DrawActionGUI()
	{
		EditorGUI.BeginChangeCheck();
		string name = EditorGUILayout.TextField("Name", selectedAction.controlData.name);
		if (EditorGUI.EndChangeCheck())
		{
			InputControlData data = selectedAction.controlData;
			data.name = name;
			selectedAction.controlData = data;
			selectedAction.name = name;
			RefreshPropertyNames();
		}

		EditorGUI.BeginChangeCheck();
		Rect rect = EditorGUILayout.GetControlRect();
		Type type = TypeGUI.TypeField(rect, new GUIContent("Control Type"), typeof(InputControl), selectedAction.controlData.controlType);
		if (EditorGUI.EndChangeCheck())
		{
			InputControlData data = selectedAction.controlData;
			data.controlType = type;
			selectedAction.controlData = data;
		}
		
		EditorGUILayout.Space();

		string[] sourceControlNames = GetSourceControlNames(selectedAction.controlData.controlType);
		if (sourceControlNames != null)
		{
			DrawCompositeControl(selectedAction, sourceControlNames);
		}
		else
		{
			if (selectedScheme >= 0 && selectedScheme < m_ActionMapEditCopy.controlSchemes.Count)
			{
				int actionIndex = m_ActionMapEditCopy.actions.IndexOf(selectedAction);
				DrawBinding(m_ActionMapEditCopy.controlSchemes[selectedScheme].bindings[actionIndex]);
			}
		}
	}

	void DrawCompositeControl(InputAction action, string[] subLabels)
	{
		if (action.controlData.componentControlIndices == null ||
			action.controlData.componentControlIndices.Length != subLabels.Length)
		{
			var data = action.controlData;
			data.componentControlIndices = new int[subLabels.Length];
			action.controlData = data;
		}
		for (int i = 0; i < subLabels.Length; i++)
		{
			DrawCompositeSource(string.Format("Source ({0})", subLabels[i]), action, i);
		}
	}

	void DrawCompositeSource(string label, InputAction action, int index)
	{
		EditorGUI.BeginChangeCheck();
		string[] actionStrings = m_ActionMapEditCopy.actions.Select(e => e.name).ToArray();
		int controlIndex = EditorGUILayout.Popup(label, action.controlData.componentControlIndices[index], actionStrings);
		if (EditorGUI.EndChangeCheck())
		{
			action.controlData.componentControlIndices[index] = controlIndex;
		}
	}
	
	void DrawBinding(ControlBinding binding)
	{
		if (binding.primaryIsButtonAxis)
		{
			DrawButtonAxisSources(binding);
			DrawSources(binding);
		}
		else
		{
			DrawSources(binding);
			DrawButtonAxisSources(binding);
		}
		
		// Remove and add buttons
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(15 * EditorGUI.indentLevel);
		if (GUILayout.Button(Styles.iconToolbarMinus, GUIStyle.none))
		{
			if (m_SelectedSource != null)
				binding.sources.Remove(m_SelectedSource);
			if (m_SelectedButtonAxisSource != null)
				binding.buttonAxisSources.Remove(m_SelectedButtonAxisSource);
		}
		Rect r = GUILayoutUtility.GetRect(Styles.iconToolbarPlusMore, GUIStyle.none);
		if (GUI.Button(r, Styles.iconToolbarPlusMore, GUIStyle.none))
		{
			ShowAddOptions(r, binding);
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
	}
	
	void ShowAddOptions(Rect rect, ControlBinding binding)
	{
		GenericMenu menu = new GenericMenu();
		menu.AddItem(new GUIContent("Regular Source"), false, AddSource, binding);
		menu.AddItem(new GUIContent("Button Axis Source"), false, AddButtonAxisSource, binding);
		menu.DropDown(rect);
	}
	
	void AddSource(object data)
	{
		ControlBinding binding = (ControlBinding)data;
		var source = new InputControlDescriptor();
		binding.sources.Add(source);
		
		m_SelectedButtonAxisSource = null;
		m_SelectedSource = source;
	}
	
	void AddButtonAxisSource(object data)
	{
		ControlBinding binding = (ControlBinding)data;
		var source = new ButtonAxisSource(new InputControlDescriptor(), new InputControlDescriptor());
		binding.buttonAxisSources.Add(source);
		
		m_SelectedSource = null;
		m_SelectedButtonAxisSource = source;
	}
	
	void DrawSources(ControlBinding binding)
	{
		for (int i = 0; i < binding.sources.Count; i++)
		{
			DrawSourceSummary(binding.sources[i]);
		}
	}
	
	void DrawButtonAxisSources(ControlBinding binding)
	{
		for (int i = 0; i < binding.buttonAxisSources.Count; i++)
		{
			DrawButtonAxisSourceSummary(binding.buttonAxisSources[i]);
		}
	}
	
	void DrawSourceSummary(InputControlDescriptor source)
	{
		Rect rect = EditorGUILayout.GetControlRect();
		
		if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
		{
			m_SelectedButtonAxisSource = null;
			m_SelectedSource = source;
			Repaint();
		}
		if (m_SelectedSource == source)
			GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
		
		DrawSourceSummary(rect, "Source", source);
		
		EditorGUILayout.Space();
	}
	
	void DrawButtonAxisSourceSummary(ButtonAxisSource source)
	{
		Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing);
		
		if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
		{
			m_SelectedSource = null;
			m_SelectedButtonAxisSource = source;
			Repaint();
		}
		if (m_SelectedButtonAxisSource == source)
			GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
		
		rect.height = EditorGUIUtility.singleLineHeight;
		DrawSourceSummary(rect, "Source (negative)", source.negative);
		rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
		DrawSourceSummary(rect, "Source (positive)", source.positive);
		
		EditorGUILayout.Space();
	}
	
	void DrawSourceSummary(Rect rect, string label, InputControlDescriptor source)
	{
		if (m_SelectedSource == source)
			GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
		
		rect = EditorGUI.PrefixLabel(rect, new GUIContent(label));
		rect.width = (rect.width - 4) * 0.5f;
		
		int indentLevel = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		var scheme = m_ActionMapEditCopy.controlSchemes[selectedScheme];
		var deviceSlots = scheme.deviceSlots;
		string[] deviceNames = deviceSlots.Select(slot =>
		{
			if (slot.type == null)
                return string.Empty;

			return string.Format("{0} {1}", InputDeviceUtility.GetDeviceNameWithTag(slot), GetDeviceInstanceString(scheme, slot));
		}).ToArray();

		EditorGUI.BeginChangeCheck();
		int deviceIndex = EditorGUI.Popup(rect, deviceSlots.FindIndex(slot =>
		{
			return source != null && source.deviceKey != DeviceSlot.kInvalidKey && slot.key == source.deviceKey;
		}), deviceNames);
		if (EditorGUI.EndChangeCheck())
			source.deviceKey = deviceSlots[deviceIndex].key;
		
		rect.x += rect.width + 4;

		var deviceSlot = scheme.GetDeviceSlot(source.deviceKey);
		string[] controlNames = InputDeviceUtility.GetDeviceControlNames(deviceSlot != null ? deviceSlot.type : null);
		EditorGUI.BeginChangeCheck();
		int controlIndex = EditorGUI.Popup(rect, source.controlIndex, controlNames);
		if (EditorGUI.EndChangeCheck())
			source.controlIndex = controlIndex;
		
		EditorGUI.indentLevel = indentLevel;
	}

	public class TypeGUI {
		static Dictionary<Type, Type[]> s_AllBasesDeviceTypes = new Dictionary<Type, Type[]>();
		static Dictionary<Type, string[]> s_AllBasesDeviceNames = new Dictionary<Type, string[]>();
		static Dictionary<Type, Dictionary<Type, int>> s_AllBasesIndicesOfDevices = new Dictionary<Type, Dictionary<Type, int>>();

		public static Type TypeField(Rect position, GUIContent label, Type baseType, Type value)
		{
			Type[] deviceTypes = null;
			string[] deviceNames = null;
			Dictionary<Type, int> indicesOfDevices = null;
			InitDevices(baseType, ref deviceTypes, ref deviceNames, ref indicesOfDevices);

			EditorGUI.BeginChangeCheck();
			int deviceIndex = EditorGUI.Popup(
				position,
				label.text,
				GetDeviceIndex(value, indicesOfDevices),
				deviceNames);
			if (EditorGUI.EndChangeCheck())
				return deviceTypes[deviceIndex];
			return value;
		}
		
		static void InitDevices(Type baseType, ref Type[] deviceTypes, ref string[] deviceNames, ref Dictionary<Type, int> indicesOfDevices)
		{
			if (deviceTypes != null)
				return;

			if (!s_AllBasesDeviceTypes.ContainsKey(baseType)) {
				deviceTypes = (
					from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
					from assemblyType in domainAssembly.GetExportedTypes()
					where assemblyType.IsSubclassOf(baseType)
					select assemblyType
				).OrderBy(e => GetInheritancePath(e, baseType)).ToArray();
				
				deviceNames = deviceTypes.Select(e => string.Empty.PadLeft(GetInheritanceDepth(e, baseType) * 3) + e.Name).ToArray();
				
				indicesOfDevices = new Dictionary<Type, int>();
				for (int i = 0; i < deviceTypes.Length; i++)
					indicesOfDevices[deviceTypes[i]] = i;

				s_AllBasesDeviceTypes[baseType] = deviceTypes;
				s_AllBasesDeviceNames[baseType] = deviceNames;
				s_AllBasesIndicesOfDevices[baseType] = indicesOfDevices;
			}
			else
			{
				deviceTypes = s_AllBasesDeviceTypes[baseType];
				deviceNames = s_AllBasesDeviceNames[baseType];
				indicesOfDevices = s_AllBasesIndicesOfDevices[baseType];
			}
		}
		
		static int GetDeviceIndex(Type type, Dictionary<Type, int> indicesOfDevices)
		{
			return (type == null ? -1 : indicesOfDevices[type]);
		}
		
		static string GetInheritancePath(Type type, Type baseType)
		{
			if (type.BaseType == baseType)
				return type.Name;
			return GetInheritancePath(type.BaseType, baseType) + "/" + type.Name;
		}
		
		static int GetInheritanceDepth(Type type, Type baseType)
		{
			if (type.BaseType == baseType)
				return 0;
			return GetInheritanceDepth(type.BaseType, baseType) + 1;
		}
	}
}
