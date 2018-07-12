using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DelftToolkit {
	public class SignalLogWindow : EditorWindow {

		private List<DingSignal> sensors = new List<DingSignal>();
		private float scrollPos;

		[MenuItem("Delft Toolkit/Signal Log")]
		public static void Init() {
			SignalLogWindow window = EditorWindow.GetWindow(typeof(SignalLogWindow)) as SignalLogWindow;
			window.Show();
		}

		private void HandleNumEvent(AiGlobals.Devices device, string adrs, float val0, float val1, float val2) {
			sensors.Add(new DingSignal(device, adrs, new Vector3(val0, val1, val2)));
			if (sensors.Count > 1000) sensors.RemoveAt(0);
			else scrollPos += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			Repaint();
		}

		private void OnGUI() {
			DingControlPhysical.DingNumPhysicalEvent -= HandleNumEvent;
			DingControlPhysical.DingNumPhysicalEvent += HandleNumEvent;
			DingControlVirtual.DingNumVirtualEvent -= HandleNumEvent;
			DingControlVirtual.DingNumVirtualEvent += HandleNumEvent;

			scrollPos = EditorGUILayout.BeginScrollView(new Vector2(0, scrollPos)).y;
			for (int i = 0; i < sensors.Count; i++) {
				EditorGUILayout.LabelField(sensors[i].ToString());
			}
			EditorGUILayout.EndScrollView();
		}
	}
}