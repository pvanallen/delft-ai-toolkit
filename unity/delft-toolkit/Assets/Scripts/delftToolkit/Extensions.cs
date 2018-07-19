using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

	public static void StopCoroutine(this Coroutine coroutine) {
		coroutineRunner.StopCoroutine(coroutine);
	}

	public static IEnumerator WaitAndDo(this Action action, float delay) {
		yield return new WaitForSeconds(delay);
		action();
	}

	/// <summary> Converts color to format "byte, byte, byte" </summary>
	public static string ToCSV(this Color32 color) {
		return color.r + "," + color.g + "," + color.b;
	}

	/// <summary> Filter string using * as wildcard </summary>
	public static bool Filter(this string src, string filter) {
		string regular = "^.*" + Regex.Escape(filter).Replace("\\*", ".*") + ".*$";
		return Regex.IsMatch(src, regular);
	}
}