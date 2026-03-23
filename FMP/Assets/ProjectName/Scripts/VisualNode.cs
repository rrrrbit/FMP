using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
public class VisualNode : MonoBehaviour
{
	public Vector3 a;
	public Vector3 v;
	public Node node;
	public bool onScreen = true;

	public static implicit operator Node(VisualNode v) => v.node;
}
