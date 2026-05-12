using System;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEngine;

[Serializable]
public abstract class DesignToken<T> : ScriptableObject, IDesignToken
{
    public T value;
    public static implicit operator T(DesignToken<T> token)
    {
        return token.value;
    }
    public event System.Action Reload;
    public void InvokeReload()
    {
        Reload?.Invoke();
    }
}

[CustomEditor(typeof(DesignToken<>), true)]
public class EDITOR_DesignToken : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();    
        if (GUILayout.Button("Reload"))
        {
            ((IDesignToken)target).InvokeReload();
        }
    }
}

interface IDesignToken
{
    public void InvokeReload();
}
