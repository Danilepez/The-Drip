using TMPro;
using UnityEngine;


public class ObjectiveDisplay : MonoBehaviour
{
    public static ObjectiveDisplay Instance { get; private set; }

    [Header("UI")]
    public TMP_Text objectiveText;
    public GameObject root;

    [Header("Objetivos (en orden)")]
    [TextArea(2, 4)]
    public string[] objectives = new string[]
    {
        "Reproduce la grabadora",
        "Ve al escritorio y busca la nota en una de las mesas",
        "Ve al sótano y busca la nota en la morgue",
        "Ve al segundo piso y busca la nota en el consultorio médico",
        "Ve a la sala del proyector en el 1er piso, ahí encontrarás la llave para salir",
        "Resuelve el acertijo sin que la muñeca te atrape",
        "Escapa por la puerta principal"
    };

    private int _currentIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.StateChanged += OnStateChanged;

        ShowObjectiveAtIndex(_currentIndex);
    }

    private void OnDestroy()
    {
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.StateChanged -= OnStateChanged;
    }

    private void Update(){}

    private void OnStateChanged(GameFlowState state)
    {
        switch (state)
        {
            case GameFlowState.SafeRoom:
            case GameFlowState.SafeRoomMinigame:
                ForceText("Resuelve el acertijo sin que la muñeca te atrape");
                break;
            case GameFlowState.FinalChase:
                ForceText("Escapa por la puerta principal");
                break;
        }
    }

    public void SetText(string text)
    {
        if (objectiveText != null)
            objectiveText.text = text;
    }

    public void NextObjective()
    {
        _currentIndex = Mathf.Min(_currentIndex + 1, objectives.Length - 1);
        ShowObjectiveAtIndex(_currentIndex);
    }

    public void SetObjective(int index)
    {
        _currentIndex = Mathf.Clamp(index, 0, objectives.Length - 1);
        ShowObjectiveAtIndex(_currentIndex);
    }

    private void ShowObjectiveAtIndex(int index)
    {
        if (objectives == null || objectives.Length == 0) return;
        int i = Mathf.Clamp(index, 0, objectives.Length - 1);
        if (objectiveText != null)
            objectiveText.text = objectives[i];
    }

    private void ForceText(string text)
    {
        if (objectiveText != null)
            objectiveText.text = text;
    }

    private void SetVisible(bool visible)
    {
        if (root != null)
            root.SetActive(visible);
        else if (objectiveText != null)
            objectiveText.gameObject.SetActive(visible);
    }
}
