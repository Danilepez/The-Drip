using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraFlash : MonoBehaviour
{
    public static CameraFlash Instance { get; private set; }

    public Image flashImage;
    public float flashDuration = 0.35f;

    [Header("Sonido de foto")]
    public AudioSource audioSource;
    public AudioClip photoSound;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (flashImage != null)
            flashImage.color = Color.clear;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void TriggerFlash()
    {
        StopAllCoroutines();
        StartCoroutine(Flash());

        if (audioSource != null && photoSound != null)
            audioSource.PlayOneShot(photoSound);
    }

    private IEnumerator Flash()
    {
        flashImage.color = Color.white;
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / flashDuration);
            flashImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        flashImage.color = Color.clear;
    }
}
