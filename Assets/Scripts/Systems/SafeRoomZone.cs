using UnityEngine;

public class SafeRoomZone : MonoBehaviour
{
    public bool requirePlayerTag = true;
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        Debug.Log("[SafeRoomZone] Jugador entró — llamando EnterSafeRoom");
        GameFlowController.Instance?.EnterSafeRoom();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        Debug.Log("[SafeRoomZone] Jugador salió — llamando ExitSafeRoom");
        GameFlowController.Instance?.ExitSafeRoom();
    }

    private bool IsPlayer(Collider other)
    {
        if (other.CompareTag(playerTag)) return true;

        if (other.GetComponentInParent<PlayerMovement>() != null) return true;

        return false;
    }
}
