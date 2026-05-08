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

    public virtual void Deactivate()
    {
        IsActive = false;
        if (Agent.isOnNavMesh)
        {
            Agent.ResetPath();
            Agent.isStopped = true;
        }
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
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

    [Header("Attack Look")]
    public float faceHeight = 1.6f;

    [Header("Sonido de Captura")]
    public AudioClip captureSound;

    protected void TryAttack()
    {
        if (AttackTimer > 0f) return;

        AttackTimer = attackCooldown;
        Anim?.SetTrigger("attack");

        Vector3 faceTarget = transform.position + Vector3.up * faceHeight;
        PlayerLook.ForceLookAt(faceTarget);

        if (captureSound != null)
        {
            var go = new GameObject("_CaptureSound");
            var src = go.AddComponent<AudioSource>();
            src.spatialBlend = 0f;
            src.PlayOneShot(captureSound);
            Destroy(go, captureSound.length + 0.1f);
        }

        PlayerLook.IsFrozen = true;
        PlayerMovement.IsFrozen = true;

        StartCoroutine(LoseAfterDelay(3f));
    }

    private IEnumerator LoseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance?.LoseGame();
    }
}
