using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public GameObject prefab;
    public int maxHealth = 50;
    public int attackDamage = 10;
    public float moveSpeed = 3.5f;
    public bool isBoss;
}
