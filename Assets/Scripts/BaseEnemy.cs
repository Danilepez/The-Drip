using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Abstract base class for all enemies in the game.
/// Handles: NavMeshAgent/Animator setup, player reference resolution,
/// renderer visibility toggling, and the shared attack + lose sequence.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Base Enemy Settings")]
    public Transform player;
    public float attackDistance = 2f;
    public float attackCooldown = 2f;

    protected NavMeshAgent Agent;
    protected Animator Anim;
    protected float AttackTimer;
    protected bool IsActive;

    protected virtual void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponentInChildren<Animator>() ?? GetComponent<Animator>();
    }

    /// <summary>
    /// Call this to make the enemy visible and start its AI.
    /// </summary>
    public virtual void Activate()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;
        IsActive = true;
    }

    protected virtual void Start()
    {
        if (player == null)
        {
            var pm = FindAnyObjectByType<PlayerMovement>();
            if (pm != null) player = pm.transform;
        }

        // Start hidden; call Activate() when the enemy should appear.
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }

    /// <summary>
    /// Attempts an attack. Resets cooldown, plays animation, freezes the player
    /// and triggers the lose sequence after a short delay.
    /// </summary>
    protected void TryAttack()
    {
        if (AttackTimer > 0f) return;

        AttackTimer = attackCooldown;
        Anim?.SetTrigger("attack");

        PlayerLook.IsFrozen = true;
        PlayerMovement.IsFrozen = true;

        // Face the player (yaw only)
        Vector3 dir = transform.position - player.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            player.rotation = Quaternion.LookRotation(dir);

        StartCoroutine(LoseAfterDelay(3f));
    }

    private IEnumerator LoseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance?.LoseGame();
    }
}
