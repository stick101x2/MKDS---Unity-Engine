using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeItemBoxEntity : Entity
{
    public DamagePlayer damage { get; protected set; }
    public ItemObject item { get; protected set; }
    public EntityGravity grav { get; protected set; }
    public EntityCollision col { get; protected set; }
    public FakeItemBox main { get; protected set; }
    // Start is called before the first frame update
    void Awake()
    {
        OnAwake();

        main = GetComponent<FakeItemBox>();
        main.e = this;

        damage = GetComponent<DamagePlayer>();
        item = GetComponent<ItemObject>();
        grav = GetComponent<EntityGravity>();
        col = GetComponent<EntityCollision>();

        main.OnAwake();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
