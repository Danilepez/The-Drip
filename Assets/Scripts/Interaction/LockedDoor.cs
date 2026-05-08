using System;
using System.Collections;
using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    public event Action MinigameStarted;
    public event Action MinigameCompleted;

    [Header("Door")]
    public BaseDoorController doorController;

    [Header("Entrance Door (se desbloquea al recoger la nota)")]
    public BaseDoorController entranceDoor;

    [Header("Minigame Items")]
    public CollectibleItem[] itemsToCollect;

    [Header("Trigger")]
    public string playerTag = "Player";

    [Header("Audio (optional)")]
    public AudioClip lockSound;
    public AudioClip unlockSound;

    [Header("Timing")]
    public float closeDelay = 2f;

    private bool _minigameStarted;
    private bool _minigameComplete;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        if (doorController != null)
            doorController.enabled = true;

        if (entranceDoor != null)
            entranceDoor.isLocked = true;
    }

    private void Update(){}

    public void UnlockEntrance()
    {
        if (entranceDoor != null)
        {
            entranceDoor.isLocked = false;
            Debug.Log("[LockedDoor] Entrada a la safe room desbloqueada.");
        }
    }

    public void BeginMinigame()
    {
        if (_minigameStarted) return;
        StartMinigame();
    }

    private void StartMinigame()
    {
        _minigameStarted = true;

        MinigameStarted?.Invoke();

        if (doorController != null)
        {
            StartCoroutine(CloseAfterDelay());
        }

        PlaySound(lockSound);

        DollController.Instance?.Activate();

        Debug.Log("[LockedDoor] Minigame started — puerta cerrada + bloqueada, Muñeca activada.");
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        yield return StartCoroutine(doorController.ForceCloseRoutine());
        doorController.isLocked = true;
    }

    private void CheckCompletion()
    {
        if (itemsToCollect == null || itemsToCollect.Length == 0) return;

        foreach (CollectibleItem item in itemsToCollect)
        {
            if (item != null) return;
        }

        CompleteMinigame();
    }

    public void CompleteMinigame()
    {
        _minigameComplete = true;

        MinigameCompleted?.Invoke();

        if (doorController != null)
            doorController.isLocked = false;

        PlaySound(unlockSound);

        if (DollController.Instance != null)
        {
            DollController.Instance.Deactivate();
        }

        Debug.Log("[LockedDoor] Minigame complete — door unlocked, Doll deactivated.");
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        AudioSource src = GetComponent<AudioSource>();
        if (src != null)
        {
            src.PlayOneShot(clip);
        }
        else
        {
            var go = new GameObject("_LockedDoorOneShot");
            var tempSrc = go.AddComponent<AudioSource>();
            tempSrc.clip = clip;
            tempSrc.spatialBlend = 0f;
            tempSrc.Play();
            Destroy(go, clip.length + 0.1f);
        }
    }
}
