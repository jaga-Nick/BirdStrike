using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int AddHP = 5;

    public GameObject bullet;

    public float lifeTime = 10f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(0, -1f * Time.deltaTime, 0);
    }

    public void Use(Unit target)
    {
        target.AddHP(this.AddHP);
        Destroy(this.gameObject);
    }
}
