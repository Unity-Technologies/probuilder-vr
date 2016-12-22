using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputNew;

public class InputManagerEndFrame
	: MonoBehaviour
{
	public void Update()
	{
		InputSystem.EndFrame();
	}
}
