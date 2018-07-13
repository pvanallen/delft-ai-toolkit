using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DelftToolkit {
	public class SignalLogWindow : EditorWindow {

		private List<DingSignal> signals = new List<DingSignal>();
		private float scrollPos;
		private float lastMaxScroll;
		private int dingFilter = -1;
		private string messageFilter = "";

		[MenuItem("Delft Toolkit/Signal Log")]
		public static void Init() {
			SignalLogWindow window = EditorWindow.GetWindow(typeof(SignalLogWindow)) as SignalLogWindow;
			window.Show();
		}

		private void HandleNumEvent(AiGlobals.Devices device, string adrs, float val0, float val1, float val2) {
			signals.Add(new DingSignal(device, adrs, new Vector3(val0, val1, val2)));
			if (signals.Count > 1000) signals.RemoveAt(0);
			Repaint();
		}

		private void OnGUI() {
			DingControlPhysical.DingNumPhysicalEvent -= HandleNumEvent;
			DingControlPhysical.DingNumPhysicalEvent += HandleNumEvent;
			DingControlVirtual.DingNumVirtualEvent -= HandleNumEvent;
			DingControlVirtual.DingNumVirtualEvent += HandleNumEvent;

			GUILayout.BeginHorizontal();
			string[] devices = new string[] { "All" }.Concat(Enum.GetNames(typeof(AiGlobals.Devices))).ToArray();
			dingFilter = EditorGUILayout.Popup("Device filter", dingFilter, devices);
			messageFilter = EditorGUILayout.TextField("Message filter", messageFilter);
			GUILayout.EndHorizontal();

			scrollPos = EditorGUILayout.BeginScrollView(new Vector2(0, scrollPos)).y;

			int count = 0;
			for (int i = 0; i < signals.Count; i++) {
				if (dingFilter != 0 && signals[i].device != (AiGlobals.Devices) dingFilter - 1) continue;
				if (!string.IsNullOrEmpty(messageFilter) && !signals[i].oscMessage.Filter(messageFilter)) continue;
				EditorGUILayout.LabelField(signals[i].ToString());
				count++;
			}

			EditorGUILayout.EndScrollView();
			float maxScroll = GetLastScrollViewMaxScroll(count);
			if (scrollPos >= lastMaxScroll) scrollPos = maxScroll + EditorGUIUtility.standardVerticalSpacing;
			lastMaxScroll = maxScroll;
		}

		private float GetLastScrollViewMaxScroll(int items) {
			float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			Rect rect = GUILayoutUtility.GetLastRect();
			return (lineHeight * items) - rect.height;
		}
	}
}