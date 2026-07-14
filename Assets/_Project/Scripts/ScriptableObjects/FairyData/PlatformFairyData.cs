using UnityEngine;

[CreateAssetMenu(fileName = "PlatformFairyData", menuName = "Scriptable Objects/PlatformFairyData")]
public class PlatformFairyData : ScriptableObject
{
    public float CoolTime;
    public PlatformTypes PlatformType;
    public float StatModifier;
}
