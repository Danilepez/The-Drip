using System.Collections;
using UnityEngine;

public abstract class BaseDoorController : BaseInteractable
{
    public float openSpeed = 2f;

    protected override IEnumerator Interact()
    {
        return Toggle();
    }

    protected abstract IEnumerator Toggle();
}
