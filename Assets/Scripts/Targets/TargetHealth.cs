using UnityEngine;

public class TargetHealth : MonoBehaviour
{
    public Health targetHealth => _targetHealth;

    [SerializeField]
    private Health _targetHealth = null;
}
