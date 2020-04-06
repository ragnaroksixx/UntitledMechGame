using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public static List<Enemy> enemies = new List<Enemy>();
    NavMeshAgent agent;
    Health h;
    private void Awake()
    {
        enemies.Add(this);
    }
    // Use this for initialization
    void Start()
    {
        h = GetComponent<Health>();
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.transform.parent.GetComponentInChildren<MechController>().Hit();
            KillAll(false);
        }
    }

    public static void KillAll(bool byPlayer)
    {
        foreach (Enemy item in enemies)
        {
            item.h.Die(byPlayer);
        }
    }
}
