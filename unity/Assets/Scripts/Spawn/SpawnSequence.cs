using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Sequence", menuName = "ScriptableObjects")]
public class SpawnSequence : ScriptableObject
{
    public int minimumScore = 0;
    public int maxScore = -1;
    public List<SpawnData> datum;
}

[System.Serializable]
public class SpawnData
{
    public Enemy enemyPrefab;
    public int spawnwerIndex;
    public float delay;
    public int numEnemies = 1;
}