using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.InputNew
{
	public class DeviceAssignmentsWindow : EditorWindow
	{
		static int s_MaxAssignedDevices;
		static int s_MaxMapDevices;
		static int s_MaxMaps;
		const int kDeviceElementWidth = 150;
		static int s_PlayerElementWidth = kDeviceElementWidth * 2 + 4;

		bool showMaps = true;
		Vector2 scrollPos;
		Dictionary<InputDevice, Rect> devicePositionTargets = new Dictionary<InputDevice, Rect>();
		Dictionary<InputDevice, Rect> devicePositions = new Dictionary<InputDevice, Rect>();

		static class Styles {
			public static GUIStyle deviceStyle;
			public static GUIStyle playerStyle;
			public static GUIStyle mapStyle;
			public static GUIStyle nodeLabel;
			static Styles()
			{
				deviceStyle = new GUIStyle("flow node 3");
				deviceStyle.margin = new RectOffset(4,4,4,4);
				deviceStyle.padding = new RectOffset(4,4,4,4);
				deviceStyle.contentOffset = Vector2.zero;
				deviceStyle.alignment = TextAnchor.MiddleCenter;
				deviceStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

				playerStyle = new GUIStyle("flow node 2");
				playerStyle.margin = new RectOffset(4,4,4,4);
				playerStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

				mapStyle = new GUIStyle("flow node 4");
				mapStyle.margin = new RectOffset(4,4,4,4);
				mapStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

				nodeLabel = new GUIStyle(EditorStyles.label);
				nodeLabel.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
			}
		}

		[MenuItem ("Window/Players")]
		static void Init()
		{
			// Get existing open window or if none, make a new one:
			DeviceAssignmentsWindow window = (DeviceAssignmentsWindow)EditorWindow.GetWindow (typeof (DeviceAssignmentsWindow));
			window.Show();
			window.titleContent = new GUIContent("Players");
		}

		void OnEnable()
		{
			PlayerHandle.onChange += Repaint;
			ActionMapInput.onStatusChange += Repaint;
			EditorApplication.playmodeStateChanged += Repaint;
		}

		void OnDisable()
		{
			PlayerHandle.onChange -= Repaint;
			ActionMapInput.onStatusChange -= Repaint;
			EditorApplication.playmodeStateChanged -= Repaint;
		}
		
		void OnGUI()
		{
			EditorGUILayout.BeginHorizontal("toolbar");
			showMaps = GUILayout.Toggle(showMaps, "Show Maps", "toolbarbutton");
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			if (showMaps)
				s_PlayerElementWidth = kDeviceElementWidth * 2 + 4;
			else
				s_PlayerElementWidth = kDeviceElementWidth;

			var devices = InputSystem.devices;
			var players = PlayerHandleManager.players;

			s_MaxAssignedDevices = 1;
			foreach (var player in players)
				s_MaxAssignedDevices = Mathf.Max(s_MaxAssignedDevices, player.assignments.Count);

			s_MaxMaps = 1;
			foreach (var player in players)
				s_MaxMaps = Mathf.Max(s_MaxMaps, player.maps.Count);

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			{
				ShowUnassignedDevices(devices);

				EditorGUILayout.Space();

				ShowGlobalPlayerHandles(devices, players);

				EditorGUILayout.Space();

				ShowPlayerHandles(devices, players);
			}
			DrawDevices(devices);
			EditorGUILayout.EndScrollView();
		}

		void DrawDevices(List<InputDevice> devices)
		{
			bool repaint = false;
			foreach (var device in devices)
			{
				Rect rect = new Rect();
				if (Event.current.type == EventType.Repaint)
				{
					Rect target = devicePositionTargets[device];
					if (devicePositions.TryGetValue(device, out rect))
					{
						devicePositions[device] = rect = new Rect(
							Vector2.Lerp(rect.position, target.position, 0.1f),
							Vector2.Lerp(rect.size, target.size, 0.1f)
						);
					}
					else
					{
						devicePositions[device] = rect = target;
					}
					if (rect != target)
						repaint = true;
				}
				GUI.Label(rect, device.ToString(), Styles.deviceStyle);
			}
			if (repaint)
				Repaint();
		}

		void ShowUnassignedDevices(IEnumerable<InputDevice> devices)
		{
			GUILayout.Label("Unassigned Devices", EditorStyles.boldLabel);
			GUILayout.Label("(In the prototype the available devices are hard-coded so for now presence in this list doesn't mean the devices physically exist and are connected.)");

			EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
			foreach (var device in devices)
			{
				if (device.assignment != null)
					continue;
				Rect rect = GUILayoutUtility.GetRect(new GUIContent(device.ToString()), Styles.deviceStyle, GUILayout.Width(kDeviceElementWidth));
				if (Event.current.type == EventType.Repaint)
					devicePositionTargets[device] = rect;
			}
			EditorGUILayout.EndHorizontal();
		}

		void ShowGlobalPlayerHandles(IEnumerable<InputDevice> devices, IEnumerable<PlayerHandle> players)
		{
			GUILayout.Label("Global Player Handles", EditorStyles.boldLabel);
			GUILayout.Label("Listen to all unassigned devices.");

			EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
			foreach (var player in players)
			{
				if (!player.global)
					continue;
				DrawPlayerHandle(player);
			}
			EditorGUILayout.EndHorizontal();
		}

		void ShowPlayerHandles(IEnumerable<InputDevice> devices, IEnumerable<PlayerHandle> players)
		{
			GUILayout.Label("Player Handles", EditorStyles.boldLabel);
			GUILayout.Label("Listen to devices they have assigned.");

			EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
			foreach (var player in players)
			{
				if (player.global)
					continue;
				DrawPlayerHandle(player);
			}
			EditorGUILayout.EndHorizontal();
		}

		void DrawPlayerHandle(PlayerHandle player)
		{
			EditorGUIUtility.labelWidth = 160;

			GUIContent playerContent = new GUIContent("Player " + player.index);
			GUILayout.BeginVertical(playerContent, Styles.playerStyle, GUILayout.Width(s_PlayerElementWidth));
			{
				GUILayout.Label("Assigned Devices", Styles.nodeLabel);
				for (int i = 0; i < s_MaxAssignedDevices; i++)
				{
					Rect deviceRect = GUILayoutUtility.GetRect(GUIContent.none, Styles.deviceStyle, GUILayout.Width(kDeviceElementWidth));
					if (i >= player.assignments.Count)
						continue;
					if (Event.current.type == EventType.Repaint)
						devicePositionTargets[player.assignments[i].device] = deviceRect;
				}

				if (showMaps)
				{
					GUILayout.Label("Action Map Inputs", Styles.nodeLabel);
					for (int i = 0; i < player.maps.Count; i++)
						DrawActionMapInput(player.maps[i]);
				}
			}
			EditorGUILayout.EndVertical();
			if (player.global)
			{
				Rect rect = GUILayoutUtility.GetLastRect();
				GUI.Label(rect, "(Global)");
			}
		}

		void DrawActionMapInput(ActionMapInput map)
		{
			EditorGUI.BeginDisabledGroup(!map.active);
			GUIContent mapContent = new GUIContent(map.actionMap.name);
			GUILayout.BeginVertical(mapContent, Styles.mapStyle);
			{
				LabelField("Block Subsequent", map.blockSubsequent.ToString());

				string schemeString = "-";
				if (map.active && map.controlScheme != null)
					schemeString = map.controlScheme.name;
				LabelField("Current Control Scheme", schemeString);

				string devicesString = "";
				if (map.active)
					devicesString = string.Join(", ", map.GetCurrentlyUsedDevices().Select(e => e.ToString()).ToArray());
				if (string.IsNullOrEmpty(devicesString))
					devicesString = "-";
				LabelField("Currently Used Devices", devicesString);
			}
			EditorGUILayout.EndVertical();
			EditorGUI.EndDisabledGroup();
		}

		void LabelField(string label1, string label2)
		{
			Rect rect = EditorGUILayout.GetControlRect();
			Rect rect1 = rect;
			Rect rect2 = rect;
			rect2.xMin += EditorGUIUtility.labelWidth;

			rect1.xMax = rect2.xMin - 2;
			GUI.Label(rect1, label1, Styles.nodeLabel);
			GUI.Label(rect2, label2, Styles.nodeLabel);
		}
	}
}