using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(BipedAnimation))]
public class BipedAnimtionEditor : Editor//PropertyDrawer
{
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		BipedAnimation bipedAnim = target as BipedAnimation;
		bipedAnim.foot0.end = EditorGUILayout.ObjectField (
			"Right foot",
			bipedAnim.foot0.end,
			typeof(Transform),
			true
		) as Transform;

		bipedAnim.foot1.end = EditorGUILayout.ObjectField (
			"Left foot",
			bipedAnim.foot1.end,
			typeof(Transform),
			true
		) as Transform;

		float offset = EditorGUILayout.FloatField ("Max foot offset", Mathf.Sqrt (bipedAnim.maxFootOffsetSq));
		bipedAnim.maxFootOffsetSq = offset * offset;

		bipedAnim.swingSpeed = 1f / EditorGUILayout.FloatField ("Swing time", 1f / bipedAnim.swingSpeed);
	}
}
