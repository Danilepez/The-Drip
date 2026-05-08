using System;
using System.Collections;
using UnityEngine;

public class RecorderInteractable : BaseInteractable
{
    public AudioSource audioSource;
    public AudioClip clip;
    public bool playOnce = true;

    [Header("Hint UI")]
    [Tooltip("Asigna aquí el GameObject del hint 'E — Reproducir' de tu Canvas.")]
    public GameObject recorderHintUI;

    private bool _played;
    private bool _showingHint;

    public event Action PlaybackFinished;

    protected override void Init()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null) audioSource.playOnAwake = false;
    }

    protected override void ShowInteractHint(bool isLookedAt, bool isOpen)
    {
        if (_showingHint == isLookedAt) return;
        _showingHint = isLookedAt;

        // Si hay hint asignado directamente, usarlo
        if (recorderHintUI != null)
        {
            recorderHintUI.SetActive(isLookedAt);
            return;
        }

        // Fallback: usar InputHintsUI
        InputHintsUI.Instance?.SetRecorderHint(this, isLookedAt);
    }

    protected override IEnumerator Interact()
    {
        if (playOnce && _played) yield break;

        _played = true;

        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
            yield return new WaitForSecondsRealtime(clip.length);
        }

        PlaybackFinished?.Invoke();
    }
}