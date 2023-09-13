using UnityEngine;

namespace Helpers.TouchHelper
{
	internal class TouchCreator
	{
		private Touch _touch;

		public float DeltaTime
		{
			get => _touch.deltaTime;
			set => _touch.deltaTime = value;
		}

		public int TapCount
		{
			get => _touch.tapCount;
			set => _touch.tapCount = value;
		}

		public TouchPhase Phase
		{
			get => _touch.phase;
			set => _touch.phase = value;
		}

		public Vector2 DeltaPosition
		{
			get => _touch.deltaPosition;
			set => _touch.deltaPosition = value;
		}

		public int FingerId
		{
			get => _touch.fingerId;
			set => _touch.fingerId = value;
		}

		public Vector2 Position
		{
			get => _touch.position;
			set => _touch.position = value;
		}

		public Vector2 RawPosition
		{
			get => _touch.rawPosition;
			set => _touch.rawPosition = value;
		}

		public Touch Create()
		{
			return _touch;
		}

		public TouchCreator()
		{
			_touch = new Touch();
		}
	}
}