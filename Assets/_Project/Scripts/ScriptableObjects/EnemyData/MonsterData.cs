using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Objects/MonsterData")]
public class MonsterData : ScriptableObject
{
    public int Health;
    public float MoveSpeed;
    public int Damage;
    public float CoolTime;
}
