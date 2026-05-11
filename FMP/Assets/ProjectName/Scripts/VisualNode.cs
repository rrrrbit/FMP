using UnityEngine;

public class VisualNode : MonoBehaviour
{
	public int id;
	public bool onScreen = true;
	public float r;
	public MGR_graphView graphView;
	public MGR_gameMaths gameMaths;
	public Rigidbody2D rb;
	public SpriteRenderer sr;

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
