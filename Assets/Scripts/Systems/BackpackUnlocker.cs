using System.Collections;
using UnityEngine;

/// <summary>
/// Pon este script en un GameObject vacío (ej. "GameSystems").
/// Bloquea el inventario y desactiva el minimapa al inicio.
/// Al recoger la mochila, llama Unlock() desde onCollected del CollectibleItem de la mochila.
/// </summary>
public class BackpackUnlocker : MonoBehaviour
{
    [Header("Minimapa")]
    [Tooltip("GameObject del minimapa. Se desactiva al inicio y se activa al recoger la mochila.")]
    public GameObject minimapObject;

    private IEnumerator Start()
    {
        // Esperar un frame para que InventoryManager.Instance esté listo
        yield return null;

        // Bloquear inventario hasta que se recoja la mochila
        InventoryManager.Instance?.SetBlocked(true);

        // Desactivar minimapa
        if (minimapObject != null)
            minimapObject.SetActive(false);
    }

    /// <summary>
    /// Llamar desde onCollected del CollectibleItem de la mochila.
    /// </summary>
    public void Unlock()
    {
        InventoryManager.Instance?.SetBlocked(false);

        if (minimapObject != null)
            minimapObject.SetActive(true);

        Debug.Log("[BackpackUnlocker] Mochila recogida — inventario y minimapa desbloqueados.");
    }
}
