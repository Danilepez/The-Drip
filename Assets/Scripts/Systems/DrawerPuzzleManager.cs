using UnityEngine;

/// <summary>
/// Puzzle de cajones: ganas cuando TODOS los cajones correctos están abiertos
/// Y NINGÚNo de los incorrectos está abierto.
///
/// Setup en Inspector:
///  - correctDrawers[]   → los cajones que deben estar abiertos (los que indica la pizarra)
///  - incorrectDrawers[] → todos los demás cajones de la sala (los que NO deben abrirse)
///  - keyObject          → GameObject de la llave, empieza desactivado
///  - audioSource + completionClip → sonido al completar (opcional)
/// </summary>
public class DrawerPuzzleManager : MonoBehaviour
{
    [Header("Cajones correctos (los que indica la pizarra)")]
    public DrawerController[] correctDrawers;

    [Header("Cajones incorrectos (los que NO deben abrirse)")]
    public DrawerController[] incorrectDrawers;

    [Header("Llave")]
    [Tooltip("GameObject de la llave (CollectibleItem). Empieza desactivado en la escena.")]
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
        // Todos los cajones correctos deben estar abiertos
        if (correctDrawers != null)
        {
            foreach (DrawerController d in correctDrawers)
            {
                if (d == null) continue;
                if (!d.isOpen) return false;
            }
        }

        // Ningún cajón incorrecto puede estar abierto
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
