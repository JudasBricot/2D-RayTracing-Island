using UnityEngine;
using UnityEditor;
using TerrainGeneration;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.Awake();
                mapGen.ApplyNoiseParameters();
                mapGen.GetFalloffMap();
                mapGen.DrawMapInEditor();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapGen.Awake();
            mapGen.ApplyNoiseParameters();
            mapGen.GetFalloffMap();
            mapGen.DrawMapInEditor();
        }
    }
}