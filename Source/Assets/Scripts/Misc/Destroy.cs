using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    public float Delay;

    float timer;
    // Start is called before the first frame update
    void OnEnable()
    {
        timer = Delay;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            LifeTimeEnd();
        }
    }
    public virtual void LifeTimeEnd()
    {
        Destroy(gameObject);
    }
}
