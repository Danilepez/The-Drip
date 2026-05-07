using UnityEngine;

public class ExamineTarget : MonoBehaviour
{
    public Transform cameraAnchor;
    public BoxCollider movementBounds;
    public Highlightable highlight;

    [Header("Look Limits")]
    public bool useCustomYawLimits = false;
    public float minYaw = -90f;
    public float maxYaw = 90f;
    public bool useCustomPitchLimits = false;
    public float minPitch = -30f;
    public float maxPitch = 30f;
    public bool clampPitchToBounds = true;

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
