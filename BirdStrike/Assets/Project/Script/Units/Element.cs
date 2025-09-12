using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Element : MonoBehaviour
{
    public float speed;

    public Vector3 direction = Vector3.zero;

    public SIDE side;

    public float power = 1;

    public float lifeTime;
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        OnUpdate();
    }

    public virtual void OnUpdate()
    {
        transform.position += speed * Time.deltaTime * direction;

        if (!GameUtil.Instance.InScreen(transform.position))
        {
            Destroy(gameObject, 1f);
        }
    }
}
