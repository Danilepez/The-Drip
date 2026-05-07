using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }

    protected void TryAttack()
    {
        if (AttackTimer > 0f) return;

        AttackTimer = attackCooldown;
        Anim?.SetTrigger("attack");

        PlayerLook.IsFrozen = true;
        PlayerMovement.IsFrozen = true;

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
