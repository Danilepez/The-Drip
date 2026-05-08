using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class DollController : BaseEnemy
{
    public static DollController Instance { get; private set; }

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 8f;

    [Header("FinalChase Respawn")]
    [Tooltip("Posición donde spawnear la muñeca al inicio del FinalChase. Si es null usa su posición actual.")]
    public Transform spawnPoint;

    private Renderer[] _renderers;
    private Camera _cam;

    protected override void Awake()
    {
        Instance = this;
        base.Awake();
    }

    public override void Activate()
    {
        // Asegurarse de que el GameObject esté activo (puede haber sido desactivado al salir de SafeRoom)
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        base.Activate();
    }

    /// <summary>Teletransporta la muñeca al spawnPoint asignado (llamado desde GameFlowController).</summary>
    public void WarpToSpawnPoint()
    {
        if (spawnPoint == null) return;
        Agent.Warp(spawnPoint.position);
        transform.position = spawnPoint.position;
    }

    protected override void Start()
    {
        base.Start();
        Agent.speed = moveSpeed;

        PlayerLook pl = FindAnyObjectByType<PlayerLook>();
        if (pl != null) _cam = pl.GetComponentInChildren<Camera>();
        if (_cam == null)  _cam = Camera.main;

        _renderers = GetComponentsInChildren<Renderer>(true);
    }

    private void Update()
    {
        if (!IsActive || player == null) return;

        AttackTimer -= Time.deltaTime;

        FacePlayer();

        if (IsInCameraFrustum())
        {
            Agent.isStopped = true;
            Agent.velocity  = Vector3.zero;
            // Anim?.SetBool("isRunning", false);
            // Anim?.SetFloat("speed", 0f);
            return;
        }

        Agent.isStopped = false;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackDistance)
        {
            Agent.ResetPath();
            // Anim?.SetBool("isRunning", false);
            // Anim?.SetFloat("speed", 0f);
            TryAttack();
        }
        else
        {
            Agent.destination = player.position;
            float velocity = Agent.velocity.magnitude;
            // Anim?.SetBool("isRunning", false);
            // Anim?.SetFloat("speed", velocity);
        }
    }

    private void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * rotationSpeed);
    }

    private bool IsInCameraFrustum()
    {
        if (_cam == null || _renderers == null || _renderers.Length == 0)
            return false;

        Bounds combined = _renderers[0].bounds;
        for (int i = 1; i < _renderers.Length; i++)
            combined.Encapsulate(_renderers[i].bounds);

        Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(_cam);
        return GeometryUtility.TestPlanesAABB(frustum, combined);
    }
}