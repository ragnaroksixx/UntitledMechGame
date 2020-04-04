using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public static List<Enemy> enemies = new List<Enemy>();
    NavMeshAgent agent;
    // Use this for initialization
    void Start()
    {
        enemies.Add(this);
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(MechController.player.transform.position);
    }
    private void OnDestroy()
    {
        enemies.Remove(this);
    }
}
