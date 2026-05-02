using UnityEngine;
using UnityEngine.InputSystem;

public class BloodInteractable : MonoBehaviour
{
    public InputActionReference interactAction;
    public AudioClip screamClip;
    public float lookRange = 2f;

    private static bool _enemySpawned;
    private Camera _cam;
    private bool _isLookedAt;

    private void Start()
    {
        PlayerLook pl = FindAnyObjectByType<PlayerLook>();
        if (pl != null) _cam = pl.GetComponentInChildren<Camera>();
        if (_cam == null) _cam = Camera.main;
    }

    private void Update()
    {
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        bool hit = Physics.Raycast(ray, out RaycastHit rh, lookRange)
                   && rh.collider.gameObject == gameObject;

        if (hit != _isLookedAt)
        {
            _isLookedAt = hit;
            if (_isLookedAt) HintTextUI.Instance?.Show(this, "F  Interactuar");
            else HintTextUI.Instance?.Hide(this);
        }

        if (_isLookedAt && interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            HintTextUI.Instance?.Hide(this);
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
