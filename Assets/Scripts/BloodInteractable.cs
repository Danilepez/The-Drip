using UnityEngine;
using UnityEngine.InputSystem;

public class BloodInteractable : MonoBehaviour
{
    public InputActionReference interactAction;
    public AudioClip screamClip;
    public float lookRange = 2f;

    private static bool _enemySpawned;
    public static int BloodPhotoCount { get; private set; }
    public static int RequiredBloodPhotos = 2;
    public static bool HasAllBloodPhotos => BloodPhotoCount >= RequiredBloodPhotos;
    private Camera _cam;
    private bool _isLookedAt;
    private bool _hasInteracted;

    public static void ResetState()
    {
        _enemySpawned = false;
        BloodPhotoCount = 0;
    }

    private void Start()
    {
        PlayerLook pl = FindAnyObjectByType<PlayerLook>();
        if (pl != null) _cam = pl.GetComponentInChildren<Camera>();
        if (_cam == null) _cam = Camera.main;
    }

    private void Update()
    {
        if (_hasInteracted) return;

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
            if (CameraFlash.Instance != null) CameraFlash.Instance.TriggerFlash();

            if (!_enemySpawned)
            {
                _enemySpawned = true;
                if (screamClip != null)
                {
                    var src = new GameObject("_ScreamOneShot").AddComponent<AudioSource>();
                    src.clip = screamClip;
                    src.spatialBlend = 0f;
                    src.volume = 1f;
                    src.Play();
                    Destroy(src.gameObject, screamClip.length + 0.1f);
                }
                EnemyController.Instance?.Activate();
            }
        }
    }
}
