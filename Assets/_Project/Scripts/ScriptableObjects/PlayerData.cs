using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public float JumpPower;
    public float MoveSpeed;
    public int Health;
    public float timeToApex = 0.35f;
    public float jumpCutMultiplier = 0.5f;
    public float coyoteTime = 0.12f;
    public float minUpwardNormalY = 0.9f;
    public float killPlaneY = -3.5f;
    public float respawnCooldown = 0.5f;
    public float invincibilityDuration = 0.5f;
    public float blinkInterval = 0.15f;
}
