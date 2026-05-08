using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseDoorController : BaseInteractable
{
    public float openSpeed = 2f;

    [Header("NavMesh")]
    public NavMeshObstacle[] navObstacles;
    public bool disableObstaclesWhenOpen = true;

    protected override IEnumerator Interact()
    {
        return Toggle();
    }

    protected void InitNavObstacles(bool isOpen)
    {
        if (navObstacles == null || navObstacles.Length == 0)
        {
            navObstacles = GetComponentsInChildren<NavMeshObstacle>();
        }

        ApplyNavObstacleState(isOpen);
    }

    protected void ApplyNavObstacleState(bool isOpen)
    {
        if (navObstacles == null || navObstacles.Length == 0) return;

        bool enabledState = !(disableObstaclesWhenOpen && isOpen);
        foreach (var obstacle in navObstacles)
        {
            if (obstacle == null) continue;
            obstacle.carving = true;
            obstacle.enabled = enabledState;
        }
    }

    protected abstract IEnumerator Toggle();
    public IEnumerator ForceCloseRoutine()
    {
        StopAllCoroutines();

        if (TryGetOpenState(out bool isOpen) && isOpen)
            yield return Toggle();
    }
}
