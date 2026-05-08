using UnityEngine;

public class SafeRoomZone : MonoBehaviour
{
    public bool requirePlayerTag = true;
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        GameFlowController.Instance?.EnterSafeRoom();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        GameFlowController.Instance?.ExitSafeRoom();
    }

    private bool IsPlayer(Collider other)
    {
        if (requirePlayerTag && !other.CompareTag(playerTag)) return false;
        if (!requirePlayerTag)
        {
            if (other.GetComponentInParent<PlayerMovement>() == null) return false;
        }
        return true;
    }
}
