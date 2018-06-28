using System.Collections.Generic;
using Rotorz.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace DelftToolkit {
	public class DelftActionListAdaptor : GenericListAdaptor<Action>, IReorderableListDropTarget {

		private const float MouseDragThresholdInPixels = 0.6f;

		// Static reference to the list adaptor of the selected item.
		private static DelftActionListAdaptor s_SelectedList;
		// Static reference limits selection to one item in one list.
		private static Action s_SelectedItem;
		// Position in GUI where mouse button was anchored before dragging occurred.
		private static Vector2 s_MouseDownPosition;

		// Holds data representing the item that is being dragged.
		private class DraggedItem {

			public static readonly string TypeName = typeof(DraggedItem).FullName;

			public readonly DelftActionListAdaptor SourceListAdaptor;
			public readonly int Index;
			public readonly Action ShoppingItem;

			public DraggedItem(DelftActionListAdaptor sourceList, int index, Action shoppingItem) {
				SourceListAdaptor = sourceList;
				Index = index;
				ShoppingItem = shoppingItem;
			}

		}

		public DelftActionListAdaptor(IList<Action> list) : base(list, null, 16f) { }

		public override void DrawItemBackground(Rect position, int index) {
			if (this == s_SelectedList && List[index] == s_SelectedItem) {
				Color restoreColor = GUI.color;
				GUI.color = ReorderableListStyles.SelectionBackgroundColor;
				GUI.DrawTexture(position, EditorGUIUtility.whiteTexture);
				GUI.color = restoreColor;
			}
		}

		public override void DrawItem(Rect position, int index) {
			Action delftAction = List[index];

			int controlID = GUIUtility.GetControlID(FocusType.Passive);

			switch (Event.current.GetTypeForControl(controlID)) {
				case EventType.MouseDown:
					Rect totalItemPosition = ReorderableListGUI.CurrentItemTotalPosition;
					if (totalItemPosition.Contains(Event.current.mousePosition)) {
						// Select this list item.
						s_SelectedList = this;
						s_SelectedItem = delftAction;
					}

					// Calculate rectangle of draggable area of the list item.
					// This example excludes the grab handle at the left.
					Rect draggableRect = totalItemPosition;
					draggableRect.x = position.x;
					draggableRect.width = position.width;

					if (Event.current.button == 0 && draggableRect.Contains(Event.current.mousePosition)) {
						// Select this list item.
						s_SelectedList = this;
						s_SelectedItem = delftAction;

						// Lock onto this control whilst left mouse button is held so
						// that we can start a drag-and-drop operation when user drags.
						GUIUtility.hotControl = controlID;
						s_MouseDownPosition = Event.current.mousePosition;
						Event.current.Use();
					}
					break;

				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID) {
						GUIUtility.hotControl = 0;

						// Begin drag-and-drop operation when the user drags the mouse
						// pointer across the threshold. This threshold helps to avoid
						// inadvertently starting a drag-and-drop operation.
						if (Vector2.Distance(s_MouseDownPosition, Event.current.mousePosition) >= MouseDragThresholdInPixels) {
							// Prepare data that will represent the item.
							var item = new DraggedItem(this, index, delftAction);

							// Start drag-and-drop operation with the Unity API.
							DragAndDrop.PrepareStartDrag();
							// Need to reset `objectReferences` and `paths` because `PrepareStartDrag`
							// doesn't seem to reset these (at least, in Unity 4.x).
							DragAndDrop.objectReferences = new Object[0];
							DragAndDrop.paths = new string[0];

							DragAndDrop.SetGenericData(DraggedItem.TypeName, item);
							DragAndDrop.StartDrag(delftAction.actionType.ToString());
						}

						// Use this event so that the host window gets repainted with
						// each mouse movement.
						Event.current.Use();
					}
					break;

				case EventType.Repaint:
					{
						Rect pos = new Rect(position.x, position.y, 50, EditorGUIUtility.singleLineHeight);
						EditorGUI.EnumPopup(pos, delftAction.actionType);

						switch (delftAction.actionType) {
							case AiGlobals.ActionTypes.move:
								DrawNextProperty(ref pos, 70);
								delftAction.moveParams.type = (AiGlobals.ActionMoveTypes) EditorGUI.EnumPopup(pos, delftAction.moveParams.type);
								DrawNextProperty(ref pos, 30);
								delftAction.moveParams.time = EditorGUI.FloatField(pos, delftAction.moveParams.time);
								DrawNextProperty(ref pos, 30);
								delftAction.moveParams.speed = EditorGUI.FloatField(pos, delftAction.moveParams.speed);
								break;
							case AiGlobals.ActionTypes.leds:
								DrawNextProperty(ref pos, 70);
								delftAction.ledParams.type = (AiGlobals.ActionLedTypes) EditorGUI.EnumPopup(pos, delftAction.ledParams.type);
								DrawNextProperty(ref pos, 30);
								delftAction.ledParams.time = EditorGUI.FloatField(pos, delftAction.ledParams.time);
								DrawNextProperty(ref pos, 30);
								delftAction.ledParams.ledNum = EditorGUI.IntField(pos, delftAction.ledParams.ledNum);
								pos.x = position.x;
								pos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
								pos.width = position.width;
								EditorGUI.BeginChangeCheck();
								Color32 col = ParseColor(delftAction.ledParams.color);
								col = EditorGUI.ColorField(pos, col);
								if (EditorGUI.EndChangeCheck()) {
									delftAction.ledParams.color = col.r + "," + col.g + "," + col.b;
								}
								break;
							case AiGlobals.ActionTypes.delay:
								DrawNextProperty(ref pos, 30);
								delftAction.delayParams.time = EditorGUI.FloatField(pos, delftAction.delayParams.time);
								break;
							case AiGlobals.ActionTypes.analogin:
								DrawNextProperty(ref pos, 70);
								delftAction.analoginParams.type = (AiGlobals.ActionAnalogInTypes) EditorGUI.EnumPopup(pos, delftAction.analoginParams.type);
								DrawNextProperty(ref pos, 30);
								delftAction.analoginParams.interval = EditorGUI.IntField(pos, delftAction.analoginParams.interval);
								DrawNextProperty(ref pos, 30);
								delftAction.analoginParams.port = EditorGUI.IntField(pos, delftAction.analoginParams.port);
								break;
						}
					}
					break;
			}
		}

		private void DrawNextProperty(ref Rect pos, float width) {
			pos.x += pos.width + 2;
			pos.width = width;
		}

		private Color32 ParseColor(string s) {
			string[] s_rgb = s.Split(',');
			if (s_rgb.Length != 3) return Color.white;
			byte[] c_rgb = new byte[3];
			for (int i = 0; i < 3; i++) byte.TryParse(s_rgb[i], out c_rgb[i]);
			return new Color32(c_rgb[0], c_rgb[1], c_rgb[2], 255);
		}

		public bool CanDropInsert(int insertionIndex) {
			if (!ReorderableListControl.CurrentListPosition.Contains(Event.current.mousePosition))
				return false;

			// Drop insertion is possible if the current drag-and-drop operation contains
			// the supported type of custom data.
			return DragAndDrop.GetGenericData(DraggedItem.TypeName) is DraggedItem;
		}

		public void ProcessDropInsertion(int insertionIndex) {
			if (Event.current.type == EventType.DragPerform) {
				var draggedItem = DragAndDrop.GetGenericData(DraggedItem.TypeName) as DraggedItem;

				// Are we just reordering within the same list?
				if (draggedItem.SourceListAdaptor == this) {
					Move(draggedItem.Index, insertionIndex);
				} else {
					// Nope, we are moving the item!
					List.Insert(insertionIndex, draggedItem.ShoppingItem);
					draggedItem.SourceListAdaptor.Remove(draggedItem.Index);

					// Ensure that the item remains selected at its new location!
					s_SelectedList = this;
				}
			}
		}

		public override float GetItemHeight(int index) {
			return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
		}
	}
}