using UnityEngine;
using BasePlatformer.Monsters;

/// <summary>
/// Ground monster health wrapper.
/// Sets the base maxHealth from GroundMonsterData before initializing currentHealth.
/// </summary>
public class GroundMonsterHealth : MonsterHealth
{
    [Header("Monster Data")]
    public GroundMonsterData monsterData;

    protected override void Awake()
    {
        if (monsterData != null)
        {
            maxHealth = monsterData.Health;
        }
        else
        {
            Debug.LogWarning($"[GroundMonsterHealth] {gameObject.name} has no monsterData assigned. Using default maxHealth.");
        }

        base.Awake();
    }
}
