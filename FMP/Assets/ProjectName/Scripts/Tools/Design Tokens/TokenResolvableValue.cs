using NUnit.Framework;
using System;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class TokenResolvableValue<T> : ITokenResolvableValue
{
    public bool useToken;
    public T value;
    public DesignToken<T> token;
    public event System.Action<T> Reload;

    public static implicit operator T(TokenResolvableValue<T> toResolve)
    {
        if (toResolve.useToken)
        {
            return toResolve.token.value;
        }
        else
        {
            return toResolve.value;
        }
    }

    public void InvokeReload()
    {
        Reload?.Invoke(this);
    }
}

interface ITokenResolvableValue
{
    public void InvokeReload();
}


