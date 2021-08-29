using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    public enum State
    {
        Defualt,
        Damage
    }
    public State state;
    public PlayerUi ui;
    public List<IPlayer> p_setup = new List<IPlayer>();
    public UserInput input { get; protected set; }
    public PlayerLap lap { get; protected set; }
    public PlayerVariables v { get; protected set; }
    public PlayerSteer steer { get; protected set; }
    public PlayerMove move { get; protected set; }
    public EntityCollision col { get; protected set; }
    public PlayerGround gnormal { get; protected set; }
    public EntityGravity gravity { get; protected set; }
    public PlayerAnim anim { get; protected set; }
    public PlayerDrift drift { get; protected set; }
    public PlayerAudio audo { get; protected set; }
    public PlayerAi ai { get; protected set; }
    public PlayerDamage dam { get; protected set; }
    public PlayerStats stats { get; protected set; }
    public PlayerItem item { get; protected set; }
    public PlayerBoost boost { get; protected set; }
    public Driver driver;
    public Kart kart;
    // Start is called before the first frame update
    public void Awake()
    {
        base.OnAwake();

        input = GetComponent<UserInput>();
        v = GetComponent<PlayerVariables>();
        steer = GetComponent<PlayerSteer>();
        move = GetComponent<PlayerMove>();
        col = GetComponent<EntityCollision>();
        gravity = GetComponent<EntityGravity>();
        gnormal = GetComponent<PlayerGround>();
        anim = GetComponentInChildren<PlayerAnim>();
        drift = GetComponent<PlayerDrift>();
        ai = GetComponent<PlayerAi>();
        audo = GetComponent<PlayerAudio>();
        lap = GetComponent<PlayerLap>();
        dam = GetComponent<PlayerDamage>();
        item = GetComponent<PlayerItem>();
        stats = GetComponent<PlayerStats>();
        boost = GetComponent<PlayerBoost>();

        stats.Setup(this);
        anim.OnSetup(this);

        p_setup.AddRange(GetComponents<IPlayer>());
        for (int i = 0; i < p_setup.Count; i++) { p_setup[i].Setup(this); }      
    }
    private void Start()
    {
        if(v.isAi)
        {
            v.wasAi = true;
        }

        if(v.debugAi)
        {
            drift.driftTilt *= 2f;
            input.accelHeld = true;
            v.isAi = true;
            input.isAi = true;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(!v.isAi)
        {
            if(input.itemDown)
            {
                input.itemDown = false;
                boost.Boost(1f,150f);
            }
        }

        gravity.Gravity();

        gnormal.OnFixedUpdate();

        switch (state)
        {
            case State.Defualt:
                Main();
                break;
            case State.Damage:
                Damaged();
                break;
            default:
                break;
        }

        Visual();
    }
    public void Damaged()
    {
        dam.OnFixedUpdate();
    }
    public void Main()
    {
        

        if (v.isAi)
        {
            CPU();
        }
        else
        {
            Human();
        }
    }
    public void Human()
    {       
        steer.OnFixedUpdate();

        move.OnFixedUpdate();

        drift.OnFixedUpdate();
    }

    public void CPU()
    {
        ai.OnFixedUpdate();

        move.OnFixedUpdate();

        drift.OnFixedUpdate();
    }

    public void SetAi()
    {
        v.isAi = true;
        input.isAi = true;
        input.EnableInput(false);
    }
    void Visual()
    {
        float ix = v.isAi ? ai.fakeInputX : input.x;
       // anim.SetDriverAnimState(anim.driveAnimState);
        anim.DriverTurn(ix);
        anim.WheelsSteer(ix);

        if (input.accelHeld && input.dccelHeld)
        {
            /*
            if (!anim.c_kart.dustInPlace.IsPlaying())
                anim.c_kart.dustInPlace.Play();*/

            anim.WheelsMove(maxSpeed / 7.5f, true);
        }
        else
        {
            /*
            if (anim.c_kart.dustInPlace.IsPlaying())
                anim.c_kart.dustInPlace.Stop();*/

            anim.WheelsMove(v.realSpeed);
        }

        kart.particles.Smoke(ref input.accelDown, input.accelHeld,input.dccelHeld);
    }

    public void SetState(State state)
    {
        this.state = state;
    }
}

public interface IPlayer
{
    void Setup(Player p);
}

