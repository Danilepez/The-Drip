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
    [Tooltip("Puerta fisica de entrada a la sala. Llama UnlockEntrance() desde onCollected de la nota.")]
    public BaseDoorController entranceDoor;

    [Header("Minigame Items")]
    [Tooltip("Ya no se usa — el puzzle de cajones lo maneja DrawerPuzzleManager.")]
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

        // La puerta de entrada empieza bloqueada hasta que la nota la desbloquee
        if (entranceDoor != null)
            entranceDoor.isLocked = true;
    }

    private void Update()
    {
        // La completación ahora la maneja DrawerPuzzleManager (llama CompleteMinigame() al recoger la llave)
    }

    /// <summary>
    /// Llamar desde el onCollected de la nota del escritorio.
    /// Desbloquea la puerta de entrada a la safe room.
    /// </summary>
    public void UnlockEntrance()
    {
        if (entranceDoor != null)
        {
            entranceDoor.isLocked = false;
            Debug.Log("[LockedDoor] Entrada a la safe room desbloqueada.");
        }
    }

    /// <summary>
    /// Llamado directamente por GameFlowController.EnterSafeRoom().
    /// No usa OnTriggerEnter para evitar condiciones de carrera con SafeRoomZone.
    /// </summary>
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

    /// <summary>
    /// Llamar desde onCollected de la llave (via Inspector) para desbloquear la puerta y activar FinalChase.
    /// </summary>
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
