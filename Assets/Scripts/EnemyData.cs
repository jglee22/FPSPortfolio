using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public GameObject prefab;
    public int maxHealth = 50;
    public int attackDamage = 10;
    public float moveSpeed = 3.5f;
    public float animatorSpeed = 1f;
    public bool isBoss;
    public float enrageHealthRatio = 0.5f;
    public float enrageMoveSpeedMultiplier = 1.5f;
    public float enrageAnimatorSpeed = 1.35f;
}
