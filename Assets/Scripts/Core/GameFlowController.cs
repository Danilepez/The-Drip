using System;
using System.Collections;
using UnityEngine;

public enum GameFlowState
{
    Intro,
    Exploration,
    BloodPhotoWindow,
    Hunt,
    HideCountdown,
    Recovery,
    SafeRoom,
    SafeRoomMinigame,
    FinalChase
}

public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }

    [Header("Blood")]
    public BloodInteractable firstBlood;
    public BloodInteractable[] bloodSpawnPoints;
    public float photoWindowSeconds = 60f;
    public bool startWithFirstBlood = true;
    public bool enableBloodSpawnCycle = true;
    public float minSpawnInterval = 50f;
    public float maxSpawnInterval = 80f;
    public bool avoidRepeatSpawn = true;

    [Header("Enemy")]
    public EnemyDirector enemyDirector;

    [Header("Safe Room")]
    public LockedDoor safeRoomDoor;

    [Header("Final Chase")]
    public DollController dollController;

    [Header("Intro")]
    public RecorderInteractable recorder;
    public bool waitForRecorder = true;
    public float recorderCooldownSeconds = 10f;

    [Header("Recovery")]
    public float recoverySeconds = 0f;

    [Header("UI")]
    public BloodTimerUI bloodTimerUI;

    public GameFlowState CurrentState { get; private set; }
    public event Action<GameFlowState> StateChanged;

    private BloodInteractable _activeBlood;
    private Coroutine _spawnRoutine;
    private int _lastSpawnIndex = -1;
    private Coroutine _recoveryRoutine;
    private Coroutine _introRoutine;
    private bool _minigameWasCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        if (bloodTimerUI == null) bloodTimerUI = BloodTimerUI.Instance;
        InitializeBloodObjects();

        if (waitForRecorder && recorder != null)
        {
            SetState(GameFlowState.Intro);
            return;
        }

        if (startWithFirstBlood && firstBlood != null)
        {
            StartBloodWindow(firstBlood);
        }
        else
        {
            SetState(GameFlowState.Exploration);
            StartSpawnCycle();
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        SubscribeBlood(firstBlood);
        if (bloodSpawnPoints != null)
        {
            foreach (var blood in bloodSpawnPoints)
            {
                SubscribeBlood(blood);
            }
        }

        if (recorder != null)
        {
            recorder.PlaybackFinished += HandleRecorderFinished;
        }

        if (enemyDirector != null)
        {
            enemyDirector.VisibilityChanged += HandleEnemyVisibility;
            enemyDirector.HiddenTimerCompleted += HandleHiddenTimerCompleted;
        }

        if (safeRoomDoor != null)
        {
            safeRoomDoor.MinigameStarted += HandleMinigameStarted;
            safeRoomDoor.MinigameCompleted += HandleMinigameCompleted;
        }

        if (bloodTimerUI != null)
        {
            bloodTimerUI.TimerElapsed += HandleTimerElapsed;
        }
        else if (BloodTimerUI.Instance != null)
        {
            BloodTimerUI.Instance.TimerElapsed += HandleTimerElapsed;
        }
    }

    private void Unsubscribe()
    {
        UnsubscribeBlood(firstBlood);
        if (bloodSpawnPoints != null)
        {
            foreach (var blood in bloodSpawnPoints)
            {
                UnsubscribeBlood(blood);
            }
        }

        if (recorder != null)
        {
            recorder.PlaybackFinished -= HandleRecorderFinished;
        }

        if (enemyDirector != null)
        {
            enemyDirector.VisibilityChanged -= HandleEnemyVisibility;
            enemyDirector.HiddenTimerCompleted -= HandleHiddenTimerCompleted;
        }

        if (safeRoomDoor != null)
        {
            safeRoomDoor.MinigameStarted -= HandleMinigameStarted;
            safeRoomDoor.MinigameCompleted -= HandleMinigameCompleted;
        }

        if (bloodTimerUI != null)
        {
            bloodTimerUI.TimerElapsed -= HandleTimerElapsed;
        }
        else if (BloodTimerUI.Instance != null)
        {
            BloodTimerUI.Instance.TimerElapsed -= HandleTimerElapsed;
        }
    }

    public void StartBloodWindow(BloodInteractable blood)
    {
        if (blood == null) return;

        StopSpawnCycle();
        _activeBlood = blood;
        _activeBlood.Activate();
        SetState(GameFlowState.BloodPhotoWindow);
        StartTimer(photoWindowSeconds);
    }

    private void HandlePhotoTaken(BloodInteractable blood)
    {
        if (CurrentState != GameFlowState.BloodPhotoWindow) return;
        if (blood != _activeBlood) return;

        StopTimer();
        _activeBlood.Deactivate();
        _activeBlood = null;
        SetState(GameFlowState.Exploration);
        StartSpawnCycle();
    }

    private void HandleTimerElapsed()
    {
        if (CurrentState != GameFlowState.BloodPhotoWindow) return;

        StopTimer();
        _activeBlood?.Deactivate();
        _activeBlood = null;
        SetState(GameFlowState.Hunt);
        enemyDirector?.StartHunt();
    }

    public void EndHunt()
    {
        if (CurrentState != GameFlowState.Hunt) return;
        BeginRecovery();
    }

    public void EnterSafeRoom()
    {
        if (CurrentState == GameFlowState.SafeRoom || CurrentState == GameFlowState.SafeRoomMinigame) return;

        StopTimer();
        StopSpawnCycle();

        if (_activeBlood != null)
        {
            _activeBlood.Deactivate();
            _activeBlood = null;
        }

        enemyDirector?.StopHunt();
        SetState(GameFlowState.SafeRoom);
    }

    public void ExitSafeRoom()
    {
        // Blocked while minigame is in progress (door is physically locked too)
        if (CurrentState == GameFlowState.SafeRoomMinigame) return;
        if (CurrentState != GameFlowState.SafeRoom) return;

        if (_minigameWasCompleted)
        {
            StartFinalChase();
        }
        else
        {
            // Fallback: minigame not done, just return to exploration
            SetState(GameFlowState.Exploration);
            StartSpawnCycle();
        }
    }

    public void StartFinalChase()
    {
        StopSpawnCycle();
        StopTimer();
        SetState(GameFlowState.FinalChase);

        // Activate main enemy
        enemyDirector?.StartHunt();

        // Activate doll
        DollController doll = dollController != null ? dollController : DollController.Instance;
        doll?.Activate();

        Debug.Log("[GameFlow] FinalChase — ambos enemigos activos. ¡Corre!");
    }

    private void HandleRecorderFinished()
    {
        if (!waitForRecorder) return;
        if (CurrentState != GameFlowState.Intro) return;

        if (_introRoutine != null)
        {
            StopCoroutine(_introRoutine);
            _introRoutine = null;
        }

        _introRoutine = StartCoroutine(IntroCooldownRoutine());
    }

    private IEnumerator IntroCooldownRoutine()
    {
        if (recorderCooldownSeconds > 0f)
        {
            yield return new WaitForSeconds(recorderCooldownSeconds);
        }

        _introRoutine = null;

        if (startWithFirstBlood && firstBlood != null)
        {
            StartBloodWindow(firstBlood);
        }
        else
        {
            SetState(GameFlowState.Exploration);
            StartSpawnCycle();
        }
    }

    private void StartTimer(float seconds)
    {
        if (bloodTimerUI != null)
        {
            bloodTimerUI.StartTimer(seconds);
        }
        else
        {
            BloodTimerUI.Instance?.StartTimer(seconds);
        }
    }

    private void StopTimer()
    {
        if (bloodTimerUI != null)
        {
            bloodTimerUI.StopTimer();
        }
        else
        {
            BloodTimerUI.Instance?.StopTimer();
        }
    }

    private void SetState(GameFlowState state)
    {
        if (CurrentState == state) return;
        CurrentState = state;
        StateChanged?.Invoke(state);
    }

    private void HandleEnemyVisibility(bool isVisible)
    {
        if (CurrentState != GameFlowState.Hunt && CurrentState != GameFlowState.HideCountdown) return;
        if (CurrentState == GameFlowState.SafeRoom) return;

        SetState(isVisible ? GameFlowState.Hunt : GameFlowState.HideCountdown);
    }

    private void HandleHiddenTimerCompleted()
    {
        if (CurrentState != GameFlowState.Hunt && CurrentState != GameFlowState.HideCountdown) return;
        BeginRecovery();
    }

    private void HandleMinigameStarted()
    {
        EnterSafeRoom();
        SetState(GameFlowState.SafeRoomMinigame);
    }

    private void HandleMinigameCompleted()
    {
        if (CurrentState == GameFlowState.SafeRoomMinigame)
        {
            _minigameWasCompleted = true;
            SetState(GameFlowState.SafeRoom);
        }
    }

    private void BeginRecovery()
    {
        enemyDirector?.StopHunt();
        SetState(GameFlowState.Recovery);

        if (_recoveryRoutine != null)
        {
            StopCoroutine(_recoveryRoutine);
            _recoveryRoutine = null;
        }

        if (recoverySeconds <= 0f)
        {
            SetState(GameFlowState.Exploration);
            StartSpawnCycle();
            return;
        }

        _recoveryRoutine = StartCoroutine(RecoveryRoutine());
    }

    private IEnumerator RecoveryRoutine()
    {
        yield return new WaitForSeconds(recoverySeconds);
        _recoveryRoutine = null;
        SetState(GameFlowState.Exploration);
        StartSpawnCycle();
    }

    private void InitializeBloodObjects()
    {
        if (firstBlood != null) firstBlood.Deactivate();
        if (bloodSpawnPoints == null) return;

        foreach (var blood in bloodSpawnPoints)
        {
            if (blood != null) blood.Deactivate();
        }
    }

    private void StartSpawnCycle()
    {
        if (!enableBloodSpawnCycle) return;
        if (_spawnRoutine != null) return;
        if (bloodSpawnPoints == null || bloodSpawnPoints.Length == 0) return;

        _spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private void StopSpawnCycle()
    {
        if (_spawnRoutine == null) return;
        StopCoroutine(_spawnRoutine);
        _spawnRoutine = null;
    }

    private IEnumerator SpawnRoutine()
    {
        float delay = UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval);
        yield return new WaitForSeconds(delay);

        if (CurrentState != GameFlowState.Exploration)
        {
            _spawnRoutine = null;
            yield break;
        }

        BloodInteractable next = PickRandomBlood();
        _spawnRoutine = null;

        if (next != null)
        {
            StartBloodWindow(next);
        }
    }

    private BloodInteractable PickRandomBlood()
    {
        if (bloodSpawnPoints == null || bloodSpawnPoints.Length == 0) return null;

        int startIndex = UnityEngine.Random.Range(0, bloodSpawnPoints.Length);

        for (int i = 0; i < bloodSpawnPoints.Length; i++)
        {
            int index = (startIndex + i) % bloodSpawnPoints.Length;
            BloodInteractable candidate = bloodSpawnPoints[index];
            if (candidate == null) continue;
            if (candidate == _activeBlood) continue;
            if (avoidRepeatSpawn && bloodSpawnPoints.Length > 1 && index == _lastSpawnIndex) continue;

            _lastSpawnIndex = index;
            return candidate;
        }

        return null;
    }

    private void SubscribeBlood(BloodInteractable blood)
    {
        if (blood == null) return;
        blood.PhotoTaken += HandlePhotoTaken;
    }

    private void UnsubscribeBlood(BloodInteractable blood)
    {
        if (blood == null) return;
        blood.PhotoTaken -= HandlePhotoTaken;
    }
}
