using UnityEngine;

public enum FiringTypes
{
    Straight,
    Arc
}

public enum ProjectileTypes {}

[CreateAssetMenu(fileName = "RangedMonsterData", menuName = "Scriptable Objects/RangedMonsterData")]
public class RangedMonsterData : MonsterData
{
    public ProjectileTypes ProjectileType;
    public FiringTypes FiringType;
}
