using System;
using UnityEngine;

public class EnemyDirector : MonoBehaviour
{
    [Header("Enemy")]
    public EnemyController enemyController;

    [Header("Visibility")]
    public Transform player;
    public float viewDistance = 20f;
    public float viewAngle = 120f;
    public float eyeHeight = 1.6f;
    public LayerMask visibilityMask = ~0;
    public float hiddenDurationSeconds = 20f;

    [Header("Audio")]
    public AudioSource screamSource;
    public AudioClip screamClip;
    public AudioSource chaseLoopSource;
    public AudioClip chaseLoopClip;
    public bool loopChaseAudio = true;

    public event Action<bool> VisibilityChanged;
    public event Action HiddenTimerCompleted;

    private bool _isHunting;
    private bool _lastVisible = true;
    private float _hiddenTimer;
    private bool _hiddenCompleted;

    public void StartHunt()
    {
        if (enemyController != null) enemyController.Activate();
        PlayScream();
        StartChaseLoop();
        _isHunting = true;
        _hiddenTimer = 0f;
        _hiddenCompleted = false;
        _lastVisible = true;
    }

    public void StopHunt()
    {
        StopChaseLoop();
        if (enemyController != null) enemyController.Deactivate();
        _isHunting = false;
        _hiddenTimer = 0f;
        _hiddenCompleted = false;
    }

    private void Update()
    {
        if (!_isHunting) return;

        bool visible = CanSeePlayer();
        if (visible != _lastVisible)
        {
            _lastVisible = visible;
            VisibilityChanged?.Invoke(visible);
            if (visible)
            {
                _hiddenTimer = 0f;
                _hiddenCompleted = false;
            }
        }

        if (!visible)
        {
            _hiddenTimer += Time.deltaTime;
            if (!_hiddenCompleted && _hiddenTimer >= hiddenDurationSeconds)
            {
                _hiddenCompleted = true;
                HiddenTimerCompleted?.Invoke();
            }
        }
    }

    private void PlayScream()
    {
        if (screamSource == null)
        {
            screamSource = GetComponent<AudioSource>();
        }

        if (screamSource == null || screamClip == null) return;
        screamSource.PlayOneShot(screamClip);
    }

    private void StartChaseLoop()
    {
        if (chaseLoopSource == null) return;
        if (chaseLoopClip != null) chaseLoopSource.clip = chaseLoopClip;
        chaseLoopSource.loop = loopChaseAudio;
        chaseLoopSource.playOnAwake = false;
        chaseLoopSource.Play();
    }

    private void StopChaseLoop()
    {
        if (chaseLoopSource == null) return;
        if (chaseLoopSource.isPlaying) chaseLoopSource.Stop();
    }

    private bool CanSeePlayer()
    {
        Transform target = player;
        if (target == null && enemyController != null) target = enemyController.player;
        if (target == null)
        {
            PlayerMovement pm = FindAnyObjectByType<PlayerMovement>();
            if (pm != null) target = pm.transform;
        }

        if (target == null) return true;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 toTarget = target.position - origin;
        float distance = toTarget.magnitude;
        if (distance > viewDistance) return false;

        Vector3 forward = transform.forward;
        Vector3 dir = toTarget.normalized;
        float angle = Vector3.Angle(forward, dir);
        if (angle > viewAngle * 0.5f) return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, distance, visibilityMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform != target && hit.transform.root != target) return false;
        }

        return true;
    }
}
