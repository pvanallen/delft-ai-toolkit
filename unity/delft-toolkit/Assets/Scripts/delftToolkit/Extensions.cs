using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions {
	public class CoroutineRunner : MonoBehaviour { }
	private static CoroutineRunner coroutineRunner;
	public static Coroutine RunCoroutine(this IEnumerator ienum) {
		if (coroutineRunner == null) {
			coroutineRunner = new GameObject("[CoroutineRunner]").AddComponent<CoroutineRunner>();
			coroutineRunner.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable | HideFlags.DontSaveInBuild;
			coroutineRunner.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable | HideFlags.DontSaveInBuild;
		}
		return coroutineRunner.StartCoroutine(ienum);
	}

	/// <summary> Converts color to format "byte, byte, byte" </summary>
	public static string ToCSV(this Color32 color) {
		return color.r + "," + color.g + "," + color.b;
	}
}