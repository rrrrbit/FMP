using TMPro;
using UnityEngine;

public class VisualNodeIdea : VisualNode
{
    public TextMeshPro text;

    void Update()
    {
        text.text = id.ToString();

		transform.localScale = 2 * r * Vector3.one;
	}

	private void FixedUpdate()
	{
		if (graphView.showIdeas)
		{
			sr.enabled = true;
		}
		else
		{
			sr.enabled = false;
			return;
		}
	}
}
