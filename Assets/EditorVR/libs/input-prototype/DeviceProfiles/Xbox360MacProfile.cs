using UnityEngine.InputNew;
using System.Collections;
using Assets.Utilities;

namespace UnityEngine.InputNew
{
	public class Xbox360MacProfile : JoystickProfile
	{
		public Xbox360MacProfile()
		{
			var gamepad = new Gamepad();
			
			AddDeviceName("Gamepad");
			AddSupportedPlatform("OS X");
			SetMappingsCount(gamepad.controlCount, gamepad.controlCount);
			
			SetMapping(00, gamepad.leftStickX.index, "Left Stick X");
			SetMapping(01, gamepad.leftStickY.index, "Left Stick Y", defaultDeadZones, Range.fullInverse, Range.full);
			SetMapping(21, gamepad.leftStickButton.index, "Left Stick Button");
			
			SetMapping(02, gamepad.rightStickX.index, "Right Stick X");
			SetMapping(03, gamepad.rightStickY.index, "Right Stick Y", defaultDeadZones, Range.fullInverse, Range.full);
			SetMapping(22, gamepad.rightStickButton.index, "Right Stick Button");
			
			SetMapping(15, gamepad.dPadUp.index, "DPad Up");
			SetMapping(16, gamepad.dPadDown.index, "DPad Down");
			SetMapping(17, gamepad.dPadLeft.index, "DPad Left");
			SetMapping(18, gamepad.dPadRight.index, "DPad Right");
			
			SetMapping(26, gamepad.action1.index, "A");
			SetMapping(27, gamepad.action2.index, "B");
			SetMapping(28, gamepad.action3.index, "X");
			SetMapping(29, gamepad.action4.index, "Y");
			
			SetMapping(04, gamepad.leftTrigger.index, "Left Trigger", defaultDeadZones, Range.full, Range.positive);
			SetMapping(05, gamepad.rightTrigger.index, "Right Trigger", defaultDeadZones, Range.full, Range.positive);
			SetMapping(23, gamepad.leftBumper.index, "Left Bumper");
			SetMapping(24, gamepad.rightBumper.index, "Right Bumper");
			
			SetMapping(19, gamepad.start.index, "Start");
			SetMapping(20, gamepad.back.index, "Back");
			SetMapping(25, gamepad.system.index, "System");
		}
	}
}
