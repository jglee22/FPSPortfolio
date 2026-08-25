using UnityEngine;

[System.Serializable]
public class WaveEnemyEntry
{
    public EnemyData enemyData;
    public int count;
}

[CreateAssetMenu(fileName = "WaveData", menuName = "Enemy/WaveData")]
public class WaveData : ScriptableObject
{
    public WaveEnemyEntry[] enemies;
    public float spawnInterval = 1f;
    public float waveDelay = 5f;
    public bool isBossWave;
    public float healthMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float speedMultiplier = 1f;
}
