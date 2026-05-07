using System;
using System.Collections;
using UnityEngine;

public class RecorderInteractable : BaseInteractable
{
    public AudioSource audioSource;
    public AudioClip clip;
    public bool playOnce = true;

    private bool _played;

    public event Action PlaybackFinished;

    protected override void Init()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null) audioSource.playOnAwake = false;
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