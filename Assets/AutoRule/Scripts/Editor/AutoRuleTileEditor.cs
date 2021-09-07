using UnityEngine;
using UnityEditor;

// ----------------------------------------------------------------------------
// Author: Alexandre Brull
// https://brullalex.itch.io/
// ----------------------------------------------------------------------------

[CustomEditor(typeof(AutoRuleTile))]
[CanEditMultipleObjects]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AutoRuleTile myScript = (AutoRuleTile)target;
        if (GUILayout.Button("Build Rule Tile"))
        {
            myScript.OverrideRuleTile();
        }
    }
}
