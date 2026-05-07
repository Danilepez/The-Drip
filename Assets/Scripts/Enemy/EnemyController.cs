using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public static EnemyController Instance { get; private set; }

    public Transform player;
    public float chaseDistance = 20f;
    public float attackDistance = 2f;
    public float runSpeed = 4f;
    public float attackCooldown = 2f;
    [SerializeField] private float _animSpeedMultiplier = 1.8f;

    private NavMeshAgent _agent;
    private Animator _anim;
    private float _attackTimer;
    private bool _isActive;

    private void Awake()
    {
        Instance = this;
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponentInChildren<Animator>() ?? GetComponent<Animator>();
    }

    public void Activate()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;
        _isActive = true;
        if (_agent != null) _agent.isStopped = false;
    }

    public void Deactivate()
    {
        _isActive = false;
        if (_agent != null)
        {
            _agent.ResetPath();
            _agent.isStopped = true;
        }

        if (_anim != null)
        {
            _anim.SetBool("isRunning", false);
            _anim.SetFloat("speed", 0f);
            _anim.speed = 1f;
        }

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }

    private void Start()
    {
        if (player == null)
        {
            var pm = FindAnyObjectByType<PlayerMovement>();
            if (pm != null) player = pm.transform;
        }
        _agent.speed = runSpeed;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }

    private void Update()
    {
        if (!_isActive || player == null) return;

        _attackTimer -= Time.deltaTime;
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > chaseDistance)
        {
            _agent.ResetPath();
            _anim?.SetBool("isRunning", false);
            _anim?.SetFloat("speed", 0f);
            if (_anim != null) _anim.speed = 1f;
        }
        else if (dist <= attackDistance)
        {
            _agent.ResetPath();
            _anim?.SetBool("isRunning", false);
            _anim?.SetFloat("speed", 0f);
            if (_anim != null) _anim.speed = 1f;
            TryAttack();
        }
        else
        {
            _agent.speed = runSpeed;
            _agent.destination = player.position;
            _anim?.SetBool("isRunning", true);

            float velocityRatio = _agent.velocity.magnitude / runSpeed;
            _anim?.SetFloat("speed", velocityRatio > 0.1f ? runSpeed : 0f);
            if (_anim != null) _anim.speed = velocityRatio > 0.1f ? velocityRatio * _animSpeedMultiplier : 1f;
        }
    }

    private void TryAttack()
    {
        if (_attackTimer > 0f) return;
        _attackTimer = attackCooldown;
        _anim?.SetTrigger("attack");

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
