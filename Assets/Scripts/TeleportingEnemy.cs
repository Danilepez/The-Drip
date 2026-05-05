using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TeleportingEnemy : BaseEnemy
{
    public static TeleportingEnemy Instance { get; private set; }

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Teleport Settings")]
    public Transform[] teleportPoints;
    public float teleportTriggerDistance = 25f;
    public float minDistanceFromPlayer = 7f;
    public float teleportCooldown = 5f;

    private bool _isTeleporting;

    protected override void Awake()
    {
        Instance = this;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        Agent.speed = moveSpeed;
    }

    private void Update()
    {
        if (!IsActive || player == null) return;

        AttackTimer -= Time.deltaTime;
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackDistance)
        {
            Agent.ResetPath();
            Anim?.SetBool("isRunning", false);
            Anim?.SetFloat("speed", 0f);
            TryAttack();
        }
        else
        {
            Agent.destination = player.position;
            float velocity = Agent.velocity.magnitude;
            Anim?.SetBool("isRunning", velocity > 0.1f);
            Anim?.SetFloat("speed", velocity);
        }

        if (dist > teleportTriggerDistance && !_isTeleporting)
            StartCoroutine(Teleport());
    }

    private IEnumerator Teleport()
    {
        _isTeleporting = true;

        // Pick the closest teleport point that is still far enough from the player.
        Transform bestPoint = null;
        float minDist = float.MaxValue;

        foreach (var point in teleportPoints)
        {
            float d = Vector3.Distance(point.position, player.position);
            if (d > minDistanceFromPlayer && d < minDist)
            {
                minDist = d;
                bestPoint = point;
            }
        }

        if (bestPoint != null)
            Agent.Warp(bestPoint.position);

        yield return new WaitForSeconds(teleportCooldown);
        _isTeleporting = false;
    }
}
