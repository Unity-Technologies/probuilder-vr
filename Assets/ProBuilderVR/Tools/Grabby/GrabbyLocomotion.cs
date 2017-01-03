using UnityEngine;
using UnityEngine.InputNew;

// GENERATED FILE - DO NOT EDIT MANUALLY
namespace UnityEngine.InputNew
{
	public class GrabbyLocomotion : ActionMapInput {
		public GrabbyLocomotion (ActionMap actionMap) : base (actionMap) { }
		
		public AxisInputControl @right { get { return (AxisInputControl)this[0]; } }
		public AxisInputControl @up { get { return (AxisInputControl)this[1]; } }
		public ButtonInputControl @trigger2 { get { return (ButtonInputControl)this[2]; } }
	}
}
