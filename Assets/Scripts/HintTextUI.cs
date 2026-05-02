using TMPro;
using UnityEngine;

public class HintTextUI : MonoBehaviour
{
    public static HintTextUI Instance { get; private set; }

    private TMP_Text _text;
    private object _owner;

    private void Awake()
    {
        Instance = this;
        _text = GetComponent<TMP_Text>();
    }

    public void Show(object owner, string msg)
    {
        _owner = owner;
        if (_text != null) _text.text = msg;
    }

    public void Hide(object owner)
    {
        if (_owner != owner) return;
        _owner = null;
        if (_text != null) _text.text = "";
    }
}
