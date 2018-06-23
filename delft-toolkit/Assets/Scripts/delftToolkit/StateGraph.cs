using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace DelftToolkit {
	[CreateAssetMenu(fileName = "New State Graph", menuName = "delft Toolkit")]
	public class StateGraph : NodeGraph {

		// The current "active" node
		public StateNodeBase current;
        //public StateNodeEditor NodeEditorWindow;

		public void Continue() {
			current.MoveNext();
		}

        //public void UpdateWindow() {
        //    current.Repaint();
        //}


	}
}