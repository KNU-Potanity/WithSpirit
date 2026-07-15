using UnityEngine;

[CreateAssetMenu(fileName = "MainFairyData", menuName = "Scriptable Objects/MainFairyData")]
public class MainFairyData : ScriptableObject
{
    public int Damage;
    public float CoolTime;
    public float PlatformSize;
    public PlatformTypes PlatformType;
}
