using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
public class VisualNode : MonoBehaviour
{
	public int id;
	public bool onScreen = true;

    // will handle clicks

    private void OnBecameVisible()
    {
        onScreen = true;
    }

    private void OnBecameInvisible()
    {
        onScreen = false;
    }
}
