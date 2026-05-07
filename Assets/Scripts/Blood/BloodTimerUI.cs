using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class BloodTimerUI : MonoBehaviour
{
    public static BloodTimerUI Instance { get; private set; }

    public event Action TimerElapsed;

    public TMP_Text timerText;
    public GameObject root;
    public bool useUnscaledTime = false;

    private Coroutine _timerRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SetVisible(false);
    }

    public void StartTimer(float seconds)
    {
        if (_timerRoutine != null) StopCoroutine(_timerRoutine);
        _timerRoutine = StartCoroutine(RunTimer(seconds));
    }

    public void StopTimer()
    {
        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
            _timerRoutine = null;
        }
        SetVisible(false);
    }

    private IEnumerator RunTimer(float seconds)
    {
        SetVisible(true);
        float remaining = Mathf.Max(0f, seconds);

        while (remaining > 0f)
        {
            UpdateText(remaining);
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            remaining -= dt;
            yield return null;
        }

        UpdateText(0f);
        _timerRoutine = null;
        TimerElapsed?.Invoke();
    }

    private void UpdateText(float seconds)
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.CeilToInt(seconds);
        int minutes = totalSeconds / 60;
        int secs = totalSeconds % 60;
        timerText.text = $"{minutes:00}:{secs:00}";
    }

    private void SetVisible(bool visible)
    {
        if (root != null)
        {
            root.SetActive(visible);
            return;
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(visible);
        }
    }
}
