using System.Collections;
using UnityEngine;

public class DrawerController : BaseInteractable
{
    public float openSpeed = 2f;
    public Vector3 openOffset = new Vector3(0f, 0f, -0.4f);
    public bool isOpen = false;

    [Header("Sonidos")]
    public AudioClip openClip;
    public AudioClip closeClip;

    private Vector3 _closedLocalPosition;
    private Vector3 _openLocalPosition;

    protected override void Init()
    {
        _closedLocalPosition = transform.localPosition;
        _openLocalPosition = _closedLocalPosition + openOffset;
    }

    protected override IEnumerator Interact()
    {
        Vector3 targetPosition = isOpen ? _closedLocalPosition : _openLocalPosition;
        isOpen = !isOpen;
        RefreshHint();

        AudioClip clip = isOpen ? openClip : closeClip;
        if (clip != null)
        {
            var go = new GameObject("_DrawerSound");
            go.transform.position = transform.position;
            var src = go.AddComponent<AudioSource>();
            src.spatialBlend = 1f;
            src.PlayOneShot(clip);
            Destroy(go, clip.length + 0.1f);
        }

        while (Vector3.Distance(transform.localPosition, targetPosition) > 0.001f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * openSpeed);
            yield return null;
        }

        transform.localPosition = targetPosition;
    }

    protected override bool TryGetOpenState(out bool open)
    {
        open = isOpen;
        return true;
    }
}
