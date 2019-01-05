using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	public abstract class ExposedValue<T> : ExposedValue {
		[Output(ShowBackingValue.Always)] public T value;

		public override object GetValue(NodePort port) {
			return value;
		}

		public abstract void SetValue(T value);
	}

	public abstract class ExposedValue : Node {
		public enum AnchorPoint { TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight }
		public bool automaticGUI = false;
		[NodeEnum] public AnchorPoint anchorPoint = AnchorPoint.Center;
		public Vector2 offset;
		public Vector2 size = new Vector2(200, 100);
		public bool draggable = false;

		public void DrawGUI() {
			if (!automaticGUI) return;

			Rect position = new Rect();
			Vector2 anchor = GetAnchorPoint();
			position.x = anchor.x - (size.x * 0.5f);
			position.y = anchor.y - (size.y * 0.5f);
			position.size = size;
			position.position += offset;
			GUILayout.BeginArea(position);
			DrawGUI(position);
			GUILayout.EndArea();
		}

		protected abstract void DrawGUI(Rect position);

		private Vector2 GetAnchorPoint() {
			switch (anchorPoint) {
				case AnchorPoint.TopLeft:
					return new Vector2(0f, 0f);
				case AnchorPoint.Top:
					return new Vector2(Screen.width * 0.5f, 0f);
				case AnchorPoint.TopRight:
					return new Vector2(Screen.width, 0f);
				case AnchorPoint.Left:
					return new Vector2(0f, Screen.height * 0.5f);
				case AnchorPoint.Center:
					return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
				case AnchorPoint.Right:
					return new Vector2(Screen.width, Screen.height * 0.5f);
				case AnchorPoint.BottomLeft:
					return new Vector2(0f, Screen.height);
				case AnchorPoint.Bottom:
					return new Vector2(Screen.width * 0.5f, Screen.height);
				case AnchorPoint.BottomRight:
					return new Vector2(Screen.width, Screen.height);
				default:
					return Vector2.zero;
			}
		}
	}
}