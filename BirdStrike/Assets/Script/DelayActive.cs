using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayActive : MonoBehaviour
{
    public float dealy = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        StartCoroutine(Delay());
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(dealy);
        this.gameObject.SetActive(false);
    }
}
