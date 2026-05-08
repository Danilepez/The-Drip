using System.Collections;
using UnityEngine;

public class DoorController : BaseDoorController
{
    public float openAngle = -90f;
    public bool isOpen = false;

    [Header("Sonidos")]
    public AudioClip openClip;
    public AudioClip closeClip;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;

    protected override void Init()
    {
        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
        InitNavObstacles(isOpen);
    }

    protected override IEnumerator Toggle()
    {
        Quaternion targetRotation = isOpen ? _closedRotation : _openRotation;
        isOpen = !isOpen;
        RefreshHint();
        ApplyNavObstacleState(isOpen);

        // Reproducir sonido al inicio del movimiento
        AudioClip clip = isOpen ? openClip : closeClip;
        if (clip != null)
        {
            var go = new GameObject("_DoorSound");
            go.transform.position = transform.position;
            var src = go.AddComponent<AudioSource>();
            src.spatialBlend = 1f;
            src.PlayOneShot(clip);
            Destroy(go, clip.length + 0.1f);
        }

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }
        transform.rotation = targetRotation;
    }

    protected override bool TryGetOpenState(out bool open)
    {
        open = isOpen;
        return true;
    }
}
