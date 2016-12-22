using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputNew;

public class ExecuteAllEvents
	: MonoBehaviour
{
	public void Update()
	{
		InputSystem.ExecuteEvents();
	}

	// We can't execute events in both Update and FixedUpdate until we have a solution
	// for how to make checks for wasJustPressed and wasJustReleased work in both.
	/*public void FixedUpdate()
	{
		InputSystem.ExecuteEvents();
	}*/
}
