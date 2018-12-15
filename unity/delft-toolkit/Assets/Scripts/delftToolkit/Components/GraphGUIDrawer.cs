using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DelftToolkit {
	public class GraphGUIDrawer : MonoBehaviour {

		public DelftToolkit.StateGraph graph;

		void OnGUI() {
			graph.DrawGUI();
		}
	}
}