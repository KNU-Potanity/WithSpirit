using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Objects/MonsterData")]
public class MonsterData : ScriptableObject
{
    public int Health;
    public float MoveSpeed;
    public int Damage;
    public float CoolTime;
    public float AttackRangeX;
    public float AttackRangeY;
    public float DetectionRange;
    public float Knockback;
    public float AttackAnimDelay;
    public float hurtFlashDuration;
    public float deathFadeDuration;
}
