using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaperHUDManager : MonoBehaviour
{
    public static PaperHUDManager Instance { get; private set; }

    public TMP_Text counterText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        if (PaperInventory.Instance != null)
            PaperInventory.Instance.OnChanged += UpdateCounter;
    }

    private void OnDisable()
    {
        if (PaperInventory.Instance != null)
            PaperInventory.Instance.OnChanged -= UpdateCounter;
    }

    private void UpdateCounter()
    {
        if (counterText != null)
            counterText.text = $"Documentos: {PaperInventory.Instance.Count} / {PaperInventory.TotalPapers}";
    }

    public void ShowPaper(PaperData data) { }
}
