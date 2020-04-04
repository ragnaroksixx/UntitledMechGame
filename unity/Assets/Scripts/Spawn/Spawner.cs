using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    public static List<Spawner> spawners = new List<Spawner>();
    List<Transform> spawnLocations;
    int i = 0;
    private void Awake()
    {
        spawners.Add(this);
        spawnLocations = new List<Transform>(transform.GetComponentsInChildren<Transform>());
    }
    public void Spawn(Enemy enemy)
    {
        Transform t = spawnLocations[i];
        i++;
        if (i >= spawnLocations.Count)
            i = 0;
        GameObject.Instantiate(enemy, t.position, Quaternion.identity);
    }
    private void OnDestroy()
    {
        spawners.Remove(this);
    }
}
