using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BloodInteractable : MonoBehaviour
{
    public InputActionReference interactAction;
    public float lookRange = 2f;

    [Header("Drip")]
    public AudioSource dripSource;
    public AudioClip dripClip;
    public bool playDripOnStart = true;
    public bool loopDrip = true;
    public bool stopDripOnPhoto = true;

    [Header("State")]
    public bool startActive = false;

    [Header("Visuals")]
    public bool hideOnDeactivate = true;
    public GameObject visualRoot;
    public Renderer[] visualRenderers;

    public static int BloodPhotoCount { get; private set; }
    public static int RequiredBloodPhotos = 2;
    public static bool HasAllBloodPhotos => BloodPhotoCount >= RequiredBloodPhotos;
    private Camera _cam;
    private bool _isLookedAt;
    private bool _hasInteracted;
    private bool _isActive;
    private Renderer[] _cachedRenderers;

    public static void ResetState()
    {
        BloodPhotoCount = 0;
    }

    private void Awake()
    {
        CacheVisuals();
    }

    private void Start()
    {
        PlayerLook pl = FindAnyObjectByType<PlayerLook>();
        if (pl != null) _cam = pl.GetComponentInChildren<Camera>();
        if (_cam == null) _cam = Camera.main;

        ConfigureDrip();
        if (startActive && GameFlowController.Instance == null) Activate();
    }

    private void Update()
    {
        if (!_isActive || _hasInteracted) return;
        if (_cam == null) return;

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        bool hit = Physics.Raycast(ray, out RaycastHit rh, lookRange)
                   && rh.collider.gameObject == gameObject;

        if (hit != _isLookedAt)
        {
            _isLookedAt = hit;
            InputHintsUI.Instance?.SetBloodHint(this, _isLookedAt);
        }

        if (_isLookedAt && interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            InputHintsUI.Instance?.SetBloodHint(this, false);
            _hasInteracted = true;
            BloodPhotoCount += 1;
            if (stopDripOnPhoto) StopDrip();
            if (CameraFlash.Instance != null) CameraFlash.Instance.TriggerFlash();
            PhotoTaken?.Invoke(this);
        }
    }

    public event Action<BloodInteractable> PhotoTaken;

    public void Activate()
    {
        _isActive = true;
        _hasInteracted = false;
        SetVisible(true);
        if (playDripOnStart) StartDrip();
    }

    public void Deactivate()
    {
        _isActive = false;
        _isLookedAt = false;
        InputHintsUI.Instance?.SetBloodHint(this, false);
        SetVisible(false);
        StopDrip();
    }

    private void ConfigureDrip()
    {
        if (dripSource == null) dripSource = GetComponent<AudioSource>();
        if (dripSource == null) return;

        if (dripClip != null) dripSource.clip = dripClip;
        dripSource.loop = loopDrip;
        dripSource.playOnAwake = false;
    }

    private void StartDrip()
    {
        if (dripSource != null && dripSource.clip != null)
        {
            dripSource.Play();
        }
    }

    private void StopDrip()
    {
        if (dripSource != null && dripSource.isPlaying)
        {
            dripSource.Stop();
        }
    }

    private void CacheVisuals()
    {
        if (visualRenderers != null && visualRenderers.Length > 0)
        {
            _cachedRenderers = visualRenderers;
        }
        else
        {
            _cachedRenderers = GetComponentsInChildren<Renderer>(true);
        }
    }

    private void SetVisible(bool visible)
    {
        if (!hideOnDeactivate) return;

        if (visualRoot != null)
        {
            visualRoot.SetActive(visible);
            return;
        }

        if (_cachedRenderers == null || _cachedRenderers.Length == 0) return;
        foreach (var r in _cachedRenderers)
        {
            if (r != null) r.enabled = visible;
        }
    }
}
