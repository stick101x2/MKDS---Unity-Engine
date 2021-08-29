using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public enum PlayerNUM // player id
{
    P1,
    P2,
    P3,
    P4
}
public class UserInput : MonoBehaviour, Control.IGameplayActions
{
    InputAction cinputAction; // move input action
    InputAction fowardAction;
    InputAction backwardAction;
    InputAction itemAction;
    InputAction driftAction;
    InputAction startAction;

    [SerializeField]
    private Control controls; // assgined input asset

    #region vars
    public bool isAi;
    public PlayerNUM NUM;

    public float x; 
    public float y; 

    public bool accelDown; 
    public bool accelHeld; 
    public bool accelUp;

    public bool driftDown;
    public bool driftHeld;
    public bool driftUp;

    public bool itemDown;
    public bool itemHeld; 
    public bool itemUp; 
    
    public bool dccelDown; 
    public bool dccelHeld; 
    public bool dccelUp; 

    public bool start; 
    // start pressed
    #endregion
    public void ResetInput() //resets input to default
    {
        x = 0;
        y = 0;

        accelDown = false;
        accelHeld = false;
        accelUp = false;

        driftDown = false;
        driftHeld = false;
        driftUp = false;

        itemDown = false;
        itemHeld = false;
        itemUp = false;

        dccelDown = false;
        dccelHeld = false;
        dccelUp = false;

        start = false;
    }
    void Awake()
    {
        controls = new Control();

        cinputAction = controls.Gameplay.Move;
        fowardAction = controls.Gameplay.Foward;
        backwardAction = controls.Gameplay.Back;
        itemAction = controls.Gameplay.Item;
        driftAction = controls.Gameplay.Drift;
        startAction = controls.Gameplay.Start;

        controls.Gameplay.SetCallbacks(this);

        if (isAi)
            return;
        EnableInput(true);

        ResetInput();
    }

    public void EnableInput(bool yes)
    {
        if (yes)
        {
            controls.Enable();
        }
        else
        {
            controls.Disable();
        }
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        var cinput = context.ReadValue<Vector2>();

        x = cinput.x;
        y = cinput.y;
    }
    public void OnFoward(InputAction.CallbackContext context)
    {
        if (!accelDown)
            accelDown = context.started;
        accelHeld = context.ReadValueAsButton();
        if (!accelUp)
            accelUp = context.canceled;
    }
    public void OnBack(InputAction.CallbackContext context)
    {
        if (!dccelDown)
            dccelDown = context.started;
        dccelHeld = context.ReadValueAsButton();
        if (!dccelUp)
            dccelUp = context.canceled;
    }
    public void OnItem(InputAction.CallbackContext context)
    {
        if (!itemDown)
        {
            itemDown = context.started;
        }
        itemHeld = context.ReadValueAsButton();
        if (!itemUp)
            itemUp = context.canceled;
    }
    public void OnDrift(InputAction.CallbackContext context)
    {
        if (!driftDown)
            driftDown = context.started;
        driftHeld = context.ReadValueAsButton();
        if (!driftUp)
            driftUp = context.canceled;
    }
    public void OnStart(InputAction.CallbackContext context)
    {
        if (!start)
            start = context.started;
    }
}
