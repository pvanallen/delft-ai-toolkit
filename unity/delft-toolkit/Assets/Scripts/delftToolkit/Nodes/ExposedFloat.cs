using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[CreateNodeMenu("Exposed values/Float")]
	public class ExposedFloat : ExposedValue<float> {

		protected override void DrawGUI(Rect position) {
			string stringValue = value.ToString("F");
			stringValue = GUILayout.TextField(stringValue);
			if (GUI.changed) {
				float newVal = value;
				if (float.TryParse(stringValue, out newVal)) value = newVal;
			}
		}

		public override void SetValue(float value) {
			this.value = value;
		}
	}
}