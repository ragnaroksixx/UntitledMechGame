﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnSequencer : MonoBehaviour
{
    public List<SpawnSequence> sequences;
    public float spawnerSafeDistance = 1;
    private void Start()
    {

    }
    public IEnumerator PlaySequences()
    {
        foreach (SpawnSequence item in sequences)
        {
            yield return PlaySequence(item);
        }
        while (true)
            yield return PlaySequence(sequences[sequences.Count - 1]);
    }
    public IEnumerator PlaySequence(SpawnSequence s)
    {
        List<Spawner> spawners = new List<Spawner>();
        spawners = GetSpawners();
        foreach (SpawnData item in s.datum)
        {
            for (int i = 0; i < item.numEnemies; i++)
            {
                int index = item.spawnwerIndex;
                int pass = spawners.Count;
                while (GetDistance(spawners[index]) < spawnerSafeDistance)
                {
                    index++;
                    if (index >= spawners.Count)
                        index = 0;
                    if (pass-- < 0)
                    {
                        index = spawners.Count - 1;
                        break;
                    }
                }
                spawners[index].Spawn(item.enemyPrefab);
                yield return new WaitForSeconds(1f);
            }
            yield return new WaitForSeconds(item.delay);
        }
    }

    public List<Spawner> GetSpawners()
    {
        List<Spawner> hits = new List<Spawner>(Spawner.spawners);
        hits.Sort(delegate (Spawner a, Spawner b)
        {
            return GetDistance(a)
            .CompareTo(GetDistance(b));
        });
        return hits;
    }

    float GetDistance(Spawner s)
    {
        Vector3 a = s.transform.position;
        a.y = 0;
        Vector3 b = MechController.player.transform.position;
        b.y = 0;
        return Vector3.Distance(a, b);
    }

}
