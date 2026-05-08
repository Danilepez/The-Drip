using System.Collections;
using UnityEngine;


public class BackpackUnlocker : MonoBehaviour
{
    [Header("Minimapa")]
    public GameObject minimapObject;

    private IEnumerator Start()
    {
        yield return null;

        InventoryManager.Instance?.SetBlocked(true);

        if (minimapObject != null)
            minimapObject.SetActive(false);
    }


    public void Unlock()
    {
        InventoryManager.Instance?.SetBlocked(false);

        if (minimapObject != null)
            minimapObject.SetActive(true);

        Debug.Log("[BackpackUnlocker] Mochila recogida — inventario y minimapa desbloqueados.");
    }
}
