namespace UnityEngine.InputNew
{
	public class PlayerDeviceAssignment
	{
		public readonly PlayerHandle player;
		public readonly InputDevice device;

		public PlayerDeviceAssignment(PlayerHandle playerHandle, InputDevice device)
		{
			this.device = device;
			this.player = playerHandle;
		}

		public void Assign()
		{
			player.assignments.Add(this);
			device.assignment = this;

			if (PlayerHandle.onChange != null)
				PlayerHandle.onChange.Invoke();
		}

		public void Unassign()
		{
			player.assignments.Remove(this);
			device.assignment = null;

			if (PlayerHandle.onChange != null)
				PlayerHandle.onChange.Invoke();
		}
	}
}
