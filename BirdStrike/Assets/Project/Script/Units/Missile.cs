using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : Element
{
    public Transform target;

    private bool running = false;

    public GameObject fxExpold;
    public override void OnUpdate()
    {
        if (!running)
            { return; }
        if (target != null)
        {
            Vector3 dir = (target.position - transform.position);
            if(dir.magnitude < 0.1)
            {
                Explod();
            }

            transform.rotation = Quaternion.FromToRotation(Vector3.left, dir);            
            transform.position += speed * Time.deltaTime * dir.normalized;

        }

    }

    public void Launch()
    {
        running = true;
    }

    public void Explod()
    {
        Instantiate(fxExpold, transform.position, Quaternion.identity);
        Destroy(gameObject);

        if(target != null)
        {
            Player p = target.GetComponent<Player>();
            p.Damage(power);
        }
    }
}
