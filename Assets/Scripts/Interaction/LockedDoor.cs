using System.Collections;
using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [Header("Door")]
    public BaseDoorController doorController;

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
    }

    private void Update()
    {
        if (!_minigameStarted || _minigameComplete) return;
        CheckCompletion();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_minigameStarted) return;
        if (!other.CompareTag(playerTag)) return;
        StartMinigame();
    }

    private void StartMinigame()
    {
        _minigameStarted = true;

        if (doorController != null)
        {
            StartCoroutine(CloseAfterDelay());
        }

        PlaySound(lockSound);

        DollController.Instance?.Activate();

        Debug.Log("[LockedDoor] Minigame started — door closed + locked, Doll activated.");
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        yield return StartCoroutine(doorController.ForceCloseRoutine());
        doorController.enabled = false;
    }

    private void CheckCompletion()
    {
        foreach (CollectibleItem item in itemsToCollect)
        {
            if (item != null) return;
        }

        CompleteMinigame();
    }

    private void CompleteMinigame()
    {
        _minigameComplete = true;

        if (doorController != null)
            doorController.enabled = true;

        PlaySound(unlockSound);

        if (DollController.Instance != null)
            DollController.Instance.gameObject.SetActive(false);

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
