using NUnit.Framework.Constraints;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Spring Settings Token", menuName = "Design Tokens/Spring Settings")]
public class TOKEN_SpringSettings : DesignToken<SpringSettings>
{

}

[Serializable]
public struct SpringSettings
{
    public float frequency;
    public float damping;
    public float response;
}