using UnityEngine;
using System;

public class DespawnNotifier : MonoBehaviour
{
    public event Action OnDespawned;

    private void OnDestroy()
    {
        OnDespawned?.Invoke();
        OnDespawned = null;
    }
}