using System;
using System.Collections.Generic;
using UnityEngine;

////REVIEW: should there be PointerMoveEvents for touches?

// So the concept of the button simulation goes like this:
// - If you put down one finger and quickly release it without moving much, it's a left clickl

namespace UnityEngine.InputNew
{
	public class Touchscreen
		: Pointer
	{
		#region Constructors

		public Touchscreen()
			: this("Touchscreen", null) { }

		protected Touchscreen(string deviceName, List<InputControlData> additionalControls)
			: base(deviceName, null)
		{
			var controls = this.controlDataList;

			for (var i = 0; i < MaxConcurrentTouches; ++i)
				AddControlsForTouch(i, controls);

			if (additionalControls != null)
				controls.AddRange(additionalControls);

			SetControls(controls);
		}

		#endregion

		#region Public Methods
		
		public new static Touchscreen current { get { return InputSystem.GetMostRecentlyUsedDevice<Touchscreen>(); } }

		public void SendSimulatedPointerEvents(TouchEvent touchEvent, bool cursorLocked)
		{
			if (cursorLocked)
				SimulateLockedMouseInput(touchEvent);
			else
				SimulateUnlockedMouseInput(touchEvent);
		}

		public override bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
		{
			var consumed = false;

			var touchEvent = inputEvent as TouchEvent;
			if (touchEvent != null)
			{
				consumed |= intoState.SetCurrentValue((int)GetControlForFinger(touchEvent.touch.fingerId, TouchControl.Touch0PositionX), touchEvent.touch.position.x);
				consumed |= intoState.SetCurrentValue((int)GetControlForFinger(touchEvent.touch.fingerId, TouchControl.Touch0PositionY), touchEvent.touch.position.y);
				consumed |= intoState.SetCurrentValue((int)GetControlForFinger(touchEvent.touch.fingerId, TouchControl.Touch0DeltaX), touchEvent.touch.delta.x);
				consumed |= intoState.SetCurrentValue((int)GetControlForFinger(touchEvent.touch.fingerId, TouchControl.Touch0DeltaY), touchEvent.touch.delta.y);

				// Store complete touch state for finger.
				var touchIndex = GetTouchIndexForFinger(touchEvent.touch.fingerId);
				m_Touches[touchIndex] = touchEvent.touch;
			}

			if (consumed)
				return true;

			return base.ProcessEventIntoState(inputEvent, intoState);
		}

		#endregion

		#region Non-Public Methods

		private static void AddControlsForTouch(int index, List<InputControlData> controls)
		{
			var prefix = "Touch" + index;

			var positionIndex = controls.Count;
			controls.Add(new InputControlData
			{
				  name = prefix + "Position"
				, componentControlIndices = new[] { positionIndex + 1, positionIndex + 2 }
			});
			controls.Add(new InputControlData
			{
				  name = prefix + "PositionX"
				, controlType = typeof(AxisInputControl)
			});
			controls.Add(new InputControlData
			{
				  name = prefix + "PositionY"
				, controlType = typeof(AxisInputControl)
			});

			var deltaIndex = controls.Count;
			controls.Add(new InputControlData
			{
				  name = prefix + "Delta"
				, componentControlIndices = new[] { deltaIndex + 1, deltaIndex + 2 }
			});
			controls.Add(new InputControlData
			{
				  name = prefix + "DeltaX"
				, controlType = typeof(AxisInputControl)
			});
			controls.Add(new InputControlData
			{
				  name = prefix + "DeltaY"
				, controlType = typeof(AxisInputControl)
			});
		}

		private static TouchControl GetControlForFinger(int fingerId, TouchControl control)
		{
			return (int)control - (int)TouchControl.Touch0Position
			       + (fingerId * TouchControlConstants.ControlsPerTouch)
			       + TouchControl.Touch0Position;
		}

		private int GetTouchIndexForFinger(int fingerId)
		{
			for (var i = 0; i < m_Touches.Count; ++i)
				if (m_Touches[i].fingerId == fingerId)
					return i;

			m_Touches.Add(new Touch());
			return m_Touches.Count - 1;
		}

		////TODO: move the entire simulation logic outside of Touchscreen entirely

		private void SimulateUnlockedMouseInput(TouchEvent touchEvent)
		{
			////TODO: perform simulation like in SimulateInputEvents.cpp
		}

		private void SimulateLockedMouseInput(TouchEvent touchEvent)
		{
			// If we currently don't have a pointer finger and this is a finger-down event,
			// make the finger the current pointer.
			if (!m_CurrentPointerTouch.isValid && touchEvent.touch.phase == TouchPhase.Began)
				m_CurrentPointerTouch = touchEvent.touch;

			////TODO: set pointer button state from touches

			// If the finger is the current pointer, update the pointer state.
			if (m_CurrentPointerTouch.fingerId == touchEvent.touch.fingerId)
			{
				SendPointerMoveEvent(touchEvent.touch.position, touchEvent.touch.delta, touchEvent.time);

				// If a pointer touch ends, update our state and detect clicks.
				if (touchEvent.touch.phase == TouchPhase.Ended)
				{
					// If touch hasn't moved significantly while down, it's a click.
					// If slow-click, it's a right-click; if fast-click, it's a left-click.
					var deltaToFirstTouchPoint = m_CurrentPointerTouch.position - touchEvent.touch.position;
					if (Mathf.Abs(deltaToFirstTouchPoint.x) < 2 && ////TODO: this should probably be configurable
						Mathf.Abs(deltaToFirstTouchPoint.y) < 2)
					{
						var clickDuration = touchEvent.time - m_CurrentPointerTouch.time;
						if (clickDuration <= 0.9f) ////TODO: should be configurable
						{
							SendPointerClickEvent(PointerControl.LeftButton, true, m_CurrentPointerTouch.time);
							SendPointerClickEvent(PointerControl.LeftButton, false, touchEvent.time);
						}
					}

					m_CurrentPointerTouch = new Touch();
				}
			}

			////TODO: right button keyed to third finger

			// If pointing with one finger and second finger goes down or up, handle
			// left-click.
			if (m_CurrentPointerTouch.isValid && !m_FirstClickTouch.isValid)
			{
				if (touchEvent.touch.phase == TouchPhase.Began)
				{
					m_FirstClickTouch = touchEvent.touch;	
					SendPointerClickEvent(PointerControl.LeftButton, true, touchEvent.time);
				}
				else if (touchEvent.touch.phase == TouchPhase.Ended)
				{
					SendPointerClickEvent(PointerControl.LeftButton, false, touchEvent.time);
					m_FirstClickTouch = new Touch();
				}
			}
		}

		private void SendPointerMoveEvent(Vector3 position, Vector3 delta, float time)
		{
			var inputEvent = InputSystem.CreateEvent<PointerMoveEvent>();
			inputEvent.time = time;
			inputEvent.deviceType = typeof(Touchscreen);
			inputEvent.deviceIndex = 0; ////FIXME: must be right index for us
			inputEvent.position = position;
			inputEvent.delta = delta;
			InputSystem.QueueEvent(inputEvent);
		}

		private void SendPointerClickEvent(PointerControl controlIndex, bool clicked, float time)
		{
			var inputEvent = InputSystem.CreateEvent<GenericControlEvent>();
			inputEvent.time = time;
			inputEvent.deviceType = typeof(Touchscreen);
			inputEvent.deviceIndex = 0; ////FIXME: must be right index for us
			inputEvent.controlIndex = (int)controlIndex;
			inputEvent.value = clicked ? 1.0f : 0.0f;
			InputSystem.QueueEvent(inputEvent);
		}

		#endregion

		#region Public Properties

		////REVIEW: this needs to be readonly, really
		public List<Touch> touches
		{
			get { return m_Touches; }
		}

		////REVIEW: this needs to be dynamic in the real thing
		public const int MaxConcurrentTouches = 5;

		#endregion

		#region Fields

		private List<Touch> m_Touches = new List<Touch>(MaxConcurrentTouches);
		private Touch m_CurrentPointerTouch;
		private Touch m_FirstClickTouch;
		private Touch m_SecondClickTouch;

		#endregion
	}
}
