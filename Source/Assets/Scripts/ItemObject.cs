using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public Player owner;
    public bool ownerIsInnume { get; protected set; }
    [SerializeField]
    float innumeDelay = 0.25f;
    float innumeTimer = 0.25f;

    public event System.Action onHitTarget;

    private void OnEnable()
    {
        InnumeReset();
    }

    private void Update()
    {
        if(ownerIsInnume)
            InnumeTick();
    }
    public void InnumeReset()
    {
        innumeTimer = innumeDelay;
        ownerIsInnume = true;
    }
    public void InnumeTick()
    {
        innumeTimer -= Time.deltaTime;
        if (innumeTimer < 0)
        {
            ownerIsInnume = false;
        }
    }

    public void OnHitTarget()
    {
        onHitTarget?.Invoke();
    }

    private void OnDestroy()
    {
        onHitTarget = null;
    }
}
