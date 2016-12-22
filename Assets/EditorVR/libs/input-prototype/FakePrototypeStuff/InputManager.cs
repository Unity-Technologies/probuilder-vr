using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputNew;

public class InputManager : MonoBehaviour
{
	public void Update()
	{
		InputSystem.BeginFrame();
	}
}
