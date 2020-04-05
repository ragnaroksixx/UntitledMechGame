using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Shootable
{
    public int hp = 1;
    public int score = 10;
    public override void OnShot(Projectile p)
    {
        base.OnShot(p);
        hp--;
        if (hp <= 1)
            Die(true);
    }
    public virtual void Die(bool byPlayer)
    {
        if (byPlayer)
            ScoreSystem.instance.AddToScore(score);
        Destroy(this.gameObject);
    }
}
