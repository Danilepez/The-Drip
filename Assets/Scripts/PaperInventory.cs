using System;
using UnityEngine;

public class PaperInventory : MonoBehaviour
{
    public static PaperInventory Instance { get; private set; }

    public event Action OnChanged;

    public const int TotalPapers = 2;
    public int Count { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddOne()
    {
        Count++;
        Debug.Log($"[PaperInventory] Count={Count}/{TotalPapers}, GameManager={GameManager.Instance}");
        OnChanged?.Invoke();
        if (Count >= TotalPapers)
            GameManager.Instance?.WinGame();
    }
}
