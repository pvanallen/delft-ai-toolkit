using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[CreateNodeMenu("Exposed values/String")]
	public class ExposedString : ExposedValue<string> {

		protected override void DrawGUI(Rect position) {
			value = GUILayout.TextField(value);
		}

		public override void SetValue(string value) {
			this.value = value;
		}
	}
}