using UnityEngine;
using UnityEditor;
 
namespace DelftToolkit {
	public class MenuItems{
		[MenuItem("Delft AI Toolkit/Show Settings")]
		private static void NewMenuOption()
		{
			Selection.activeObject = Resources.Load<Settings>("DelftAIToolkitSettings");
		}
	}
}
