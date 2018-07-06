using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace DelftToolkit {
	[CustomNodeGraphEditor(typeof(StateGraph))]
	public class StateGraphEditor : NodeGraphEditor {

		/// <summary> 
		/// Overriding GetNodePath lets you control if and how nodes are categorized.
		/// In this example we are sorting out all node types that are not in the XNode.Examples namespace.
		/// </summary>
		public override string GetNodeMenuName(System.Type type) {
			if (type.Namespace == "DelftToolkit") {
				return base.GetNodeMenuName(type).Replace("Delft Toolkit/", "Create/");
			} else return null;
		}

        [InitializeOnLoadMethod]
        private static void initEvents() {
            Actions.DingEvent -= SetAction;
            Actions.DingEvent += SetAction;
			DingControlPhysical.DingNumPhysicalEvent -= HandleNumEvent;
			DingControlPhysical.DingNumPhysicalEvent += HandleNumEvent;
			DingControlVirtual.DingNumVirtualEvent -= HandleNumEvent;
			DingControlVirtual.DingNumVirtualEvent += HandleNumEvent;
        }

        public static void SetAction(AiGlobals.Devices aDevice, Action anAction) {
            NodeEditorWindow.current.Repaint();
        }

		public static void HandleNumEvent(AiGlobals.Devices device, string adrs, float val0, float val1, float val2) {
			NodeEditorWindow.current.Repaint();
		}
	}
}