using UnityEngine;


public class DrawerPuzzleManager : MonoBehaviour
{
    [Header("Cajones correctos (los que indica la pizarra)")]
    public DrawerController[] correctDrawers;

    [Header("Cajones incorrectos (los que NO deben abrirse)")]
    public DrawerController[] incorrectDrawers;

    [Header("Llave")]
    public GameObject keyObject;

    [Header("Audio (opcional)")]
    public AudioSource audioSource;
    public AudioClip completionClip;

    private bool _completed;

    private void Start()
    {
        if (keyObject != null)
            keyObject.SetActive(false);

        if (correctDrawers == null || correctDrawers.Length == 0)
            Debug.LogWarning("[DrawerPuzzle] No hay cajones correctos asignados.");
    }

    private void Update()
    {
        if (_completed) return;

        if (IsSolved())
            Complete();
    }

    private bool IsSolved()
    {
        if (correctDrawers != null)
        {
            foreach (DrawerController d in correctDrawers)
            {
                if (d == null) continue;
                if (!d.isOpen) return false;
            }
        }

        if (incorrectDrawers != null)
        {
            foreach (DrawerController d in incorrectDrawers)
            {
                if (d == null) continue;
                if (d.isOpen) return false;
            }
        }

        return true;
    }

    private void Complete()
    {
        _completed = true;

        if (keyObject != null)
            keyObject.SetActive(true);

        if (audioSource != null && completionClip != null)
            audioSource.PlayOneShot(completionClip);

        Debug.Log("[DrawerPuzzle] ¡Combinación correcta! Llave disponible.");
    }
}
