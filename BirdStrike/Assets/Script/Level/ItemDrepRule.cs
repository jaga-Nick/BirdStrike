using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class ItemDrepRule : MonoBehaviour
{
    public Item item;
    public float dropRatio;

    public void Execute(Vector3 pos)
    {
        if(Random.Range(0f,100f) < dropRatio)
        {
            Item rule = Instantiate<Item>(item);
            rule.transform.position = pos;
        }
    }
}
