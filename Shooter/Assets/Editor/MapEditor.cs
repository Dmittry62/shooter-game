using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		if (DrawDefaultInspector())
		{
			MapGenerator map = target as MapGenerator;
			map.GenerateMap ();
		}
	}
}
