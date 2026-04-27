using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(TokenResolvableValue<SpringSettings>), true)]
public class TokenResolvableValueEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();    
        if (GUILayout.Button("Reload"))
        {
            ((ITokenResolvableValue)target).InvokeReload();
        }
    }
}
