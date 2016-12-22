using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.InputNew
{
	public static class PlayerHandleManager
	{
		static Dictionary<int, PlayerHandle> s_Players = null;
		static int s_NextPlayerIndex = 0;

		public static IEnumerable<PlayerHandle> players { get { return s_Players.Values; } }

		static PlayerHandleManager()
		{
			Reset();

			#if UNITY_EDITOR
			EditorApplication.playmodeStateChanged += OnPlaymodeChanged;
			#endif
		}

		public static PlayerHandle GetNewPlayerHandle()
		{
			PlayerHandle handle = new PlayerHandle(s_NextPlayerIndex);
			s_Players[handle.index] = handle;
			s_NextPlayerIndex++;
			return handle;
		}

		// Gets existing handle for index if available.
		public static PlayerHandle GetPlayerHandle(int index)
		{
			PlayerHandle player = null;
			s_Players.TryGetValue(index, out player);
			return player;
		}

		internal static void RemovePlayerHandle(PlayerHandle handle)
		{
			s_Players.Remove(handle.index);
		}

		#if UNITY_EDITOR
		static void OnPlaymodeChanged()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;
			Reset();
		}
		#endif

		static void Reset()
		{
			if (s_Players != null)
			{
				List<PlayerHandle> players = new List<PlayerHandle>(s_Players.Values);
				for (int i = 0; i < players.Count; i++)
					players[i].Destroy();
			}

			s_Players = new Dictionary<int, PlayerHandle>();
			s_NextPlayerIndex = 0;
		}
	}
}
