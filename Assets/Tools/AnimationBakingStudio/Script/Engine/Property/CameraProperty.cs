using System;

namespace ABS
{
	public enum CameraMode : int
    {
		Orthographic = 0,
		Perspective = 1
    }

	[Serializable]
	public class CameraProperty : PropertyBase
	{
		public CameraMode mode = CameraMode.Orthographic;
		public float orthographicSize = 2.5f;
		public float fieldOfView = 45;
		public float distance = 6;
	}
}
