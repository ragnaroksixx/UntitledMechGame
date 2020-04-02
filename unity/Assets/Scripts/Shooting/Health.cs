using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Shootable
{
    public int hp = 1;
    public override void OnShot(Projectile p)
    {
        base.OnShot(p);
        hp--;
        if (hp <= 1)
            Die();
    }
    public virtual void Die()
    {
        Destroy(this.gameObject);
    }
}
