using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : BaseEnemy
{
    public static EnemyController Instance { get; private set; }

    [Header("Chase Settings")]
    public float chaseDistance = 20f;
    public float runSpeed = 4f;
    [SerializeField] private float _animSpeedMultiplier = 1.8f;

    [Header("Chase Audio")]
    public AudioClip[] chaseClips;
    public float chaseAudioMinDistance = 2f;
    public float chaseAudioMaxDistance = 20f;

    private AudioSource _chaseAudio;
    private int _lastClipIndex = -1;

    protected override void Awake()
    {
        Instance = this;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        Agent.speed = runSpeed;

        _chaseAudio = gameObject.AddComponent<AudioSource>();
        _chaseAudio.spatialBlend = 1f;
        _chaseAudio.rolloffMode = AudioRolloffMode.Linear;
        _chaseAudio.minDistance = chaseAudioMinDistance;
        _chaseAudio.maxDistance = chaseAudioMaxDistance;
        _chaseAudio.loop = false;
        _chaseAudio.playOnAwake = false;
    }

    private void Update()
    {
        if (!IsActive || player == null) return;

        AttackTimer -= Time.deltaTime;
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > chaseDistance)
        {
            Agent.ResetPath();
            Anim?.SetBool("isRunning", false);
            Anim?.SetFloat("speed", 0f);
            if (Anim != null) Anim.speed = 1f;
        }
        else if (dist <= attackDistance)
        {
            Agent.ResetPath();
            Anim?.SetBool("isRunning", false);
            Anim?.SetFloat("speed", 0f);
            if (Anim != null) Anim.speed = 1f;
            TryAttack();
        }
        else
        {
            Agent.speed = runSpeed;
            Agent.destination = player.position;
            Anim?.SetBool("isRunning", true);

            float velocityRatio = Agent.velocity.magnitude / runSpeed;
            Anim?.SetFloat("speed", velocityRatio > 0.1f ? runSpeed : 0f);
            if (Anim != null) Anim.speed = velocityRatio > 0.1f ? velocityRatio * _animSpeedMultiplier : 1f;

            PlayChaseAudio();
        }
    }

    private void PlayChaseAudio()
    {
        if (chaseClips == null || chaseClips.Length == 0) return;
        if (_chaseAudio.isPlaying) return;

        // Pick a random clip, avoiding immediate repeats
        int index;
        do { index = Random.Range(0, chaseClips.Length); }
        while (chaseClips.Length > 1 && index == _lastClipIndex);

        _lastClipIndex = index;
        _chaseAudio.clip = chaseClips[index];
        _chaseAudio.Play();
    }
}
