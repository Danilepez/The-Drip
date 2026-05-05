using UnityEngine;

public class ExamineTarget : MonoBehaviour
{
    public Transform cameraAnchor;
    public BoxCollider movementBounds;
    public Highlightable highlight;

    private void Awake()
    {
        if (movementBounds != null) movementBounds.isTrigger = true;
        if (highlight == null) highlight = GetComponentInParent<Highlightable>();
        if (highlight == null) highlight = GetComponentInChildren<Highlightable>(true);
    }

    public void SetHighlighted(bool highlighted)
    {
        highlight?.SetHighlighted(highlighted);
    }
}
