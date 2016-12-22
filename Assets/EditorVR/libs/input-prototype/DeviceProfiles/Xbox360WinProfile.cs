using UnityEngine.InputNew;
using System.Collections;
using Assets.Utilities;

namespace UnityEngine.InputNew
{
	public class Xbox360WinProfile : JoystickProfile
	{
		public Xbox360WinProfile()
		{
			var gamepad = new Gamepad();
			
			AddDeviceName("Gamepad");
			AddSupportedPlatform("Windows");
			SetMappingsCount(gamepad.controlDataList.Count, gamepad.controlDataList.Count);

			SetMapping(00, gamepad.leftStickX.index, "Left Stick X");
			SetMapping(01, gamepad.leftStickY.index, "Left Stick Y", defaultDeadZones, Range.fullInverse, Range.full);
			SetMapping(18, gamepad.leftStickButton.index, "Left Stick Button");
			
			SetMapping(03, gamepad.rightStickX.index, "Right Stick X");
			SetMapping(04, gamepad.rightStickY.index, "Right Stick Y", defaultDeadZones, Range.fullInverse, Range.full);
			SetMapping(19, gamepad.rightStickButton.index, "Right Stick Button");
			
			SetMapping(06, gamepad.dPadUp.index, "DPad Up");
			SetMapping(06, gamepad.dPadDown.index, "DPad Down");
			SetMapping(05, gamepad.dPadLeft.index, "DPad Left");
			SetMapping(05, gamepad.dPadRight.index, "DPad Right");
			
			SetMapping(10, gamepad.action1.index, "A");
			SetMapping(11, gamepad.action2.index, "B");
			SetMapping(12, gamepad.action3.index, "X");
			SetMapping(13, gamepad.action4.index, "Y");
			
			SetMapping(08, gamepad.leftTrigger.index, "Left Trigger", defaultDeadZones, Range.full, Range.positive);
			SetMapping(09, gamepad.rightTrigger.index, "Right Trigger", defaultDeadZones, Range.full, Range.positive);
			SetMapping(14, gamepad.leftBumper.index, "Left Bumper");
			SetMapping(15, gamepad.rightBumper.index, "Right Bumper");
			
			SetMapping(17, gamepad.start.index, "Start");
			SetMapping(16, gamepad.back.index, "Back");
		}
	}
}
