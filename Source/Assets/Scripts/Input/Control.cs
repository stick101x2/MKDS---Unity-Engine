// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/Input/Control.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Control : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Control()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Control"",
    ""maps"": [
        {
            ""name"": ""Gameplay"",
            ""id"": ""96984c7e-215f-4bed-88b6-d98fe48ed442"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""757abf5d-4e65-4233-8ece-db857e1d4646"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Foward"",
                    ""type"": ""Button"",
                    ""id"": ""7b446247-9ff8-4cd5-902c-8cc1de23d7f3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Back"",
                    ""type"": ""Button"",
                    ""id"": ""2df59f3e-569a-420a-a8cd-cde3469064cb"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Item"",
                    ""type"": ""Button"",
                    ""id"": ""a9010e85-e96b-43e0-b2e6-b14fec568e43"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Drift"",
                    ""type"": ""Button"",
                    ""id"": ""a645f381-25a3-4cba-9b28-61a19fb252e6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Start"",
                    ""type"": ""Button"",
                    ""id"": ""a299f7eb-24e7-4647-8dca-68b7698601ba"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""ec55252a-3335-4d82-b334-3202fd7361f3"",
                    ""path"": ""<Keyboard>/p"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Foward"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4f3c7429-385b-4657-9805-661ebb1b224d"",
                    ""path"": ""<Keyboard>/o"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Back"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6d8b9c33-f525-448e-b298-ea2f3e24fb6e"",
                    ""path"": ""<Keyboard>/i"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Item"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""46ca8f35-d310-4e14-884f-ed12eb5d4e86"",
                    ""path"": ""<Keyboard>/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drift"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3588e7a6-a433-4589-a027-cd1978505a71"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drift"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5082ac5b-f584-4a94-ab3d-a7e1e6eac947"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Start"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Move"",
                    ""id"": ""d5c6f5cd-1b61-4e32-80f1-c52f40517eeb"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""db858ffb-5d33-4ba4-8a2c-d407952b36ec"",
                    ""path"": ""<Keyboard>/t"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""ea8671be-4f2c-4c1a-a6d3-604cd5145e29"",
                    ""path"": ""<Keyboard>/g"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""e21afa4e-45e4-4bf7-ad65-f4d8a390507c"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""f3346dca-c2de-4f3d-8c4a-f512bed397b8"",
                    ""path"": ""<Keyboard>/h"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        },
        {
            ""name"": ""Ui"",
            ""id"": ""e33bf8f6-baa1-44da-9a74-b8c9a70c26ab"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""0aa3064e-fc4f-47ed-a14e-303f9c0759c3"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Select"",
                    ""type"": ""Button"",
                    ""id"": ""d9cafec2-dad6-4047-bb8b-3d2fead83143"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Return"",
                    ""type"": ""Button"",
                    ""id"": ""4165972e-a121-4dd2-90af-548b7bfd783e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Start"",
                    ""type"": ""Button"",
                    ""id"": ""6ffbdcb0-8a68-4ab3-a6be-08a3ff44f145"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Move"",
                    ""id"": ""e1bbbf2b-d137-48e1-8c1a-6dd0dc16cdcf"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""e9f79a72-de0f-4cf1-9849-e92a6da0d353"",
                    ""path"": ""<Keyboard>/t"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""678ba0a3-5e62-4e86-afd3-083519fbd8fd"",
                    ""path"": ""<Keyboard>/g"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""e59c5463-68dc-4964-9007-4b66925c92ee"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""63a9e058-4e42-4a46-a9b7-88924c09b853"",
                    ""path"": ""<Keyboard>/h"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""822cd3f0-d0ff-4d67-8d93-99ee393fec66"",
                    ""path"": ""<Keyboard>/p"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""151b5526-1e46-4b07-801f-48953e95ede2"",
                    ""path"": ""<Keyboard>/o"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Return"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""fb9fe24c-fb27-4284-a881-1863f8c4937c"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Start"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Gameplay
        m_Gameplay = asset.FindActionMap("Gameplay", throwIfNotFound: true);
        m_Gameplay_Move = m_Gameplay.FindAction("Move", throwIfNotFound: true);
        m_Gameplay_Foward = m_Gameplay.FindAction("Foward", throwIfNotFound: true);
        m_Gameplay_Back = m_Gameplay.FindAction("Back", throwIfNotFound: true);
        m_Gameplay_Item = m_Gameplay.FindAction("Item", throwIfNotFound: true);
        m_Gameplay_Drift = m_Gameplay.FindAction("Drift", throwIfNotFound: true);
        m_Gameplay_Start = m_Gameplay.FindAction("Start", throwIfNotFound: true);
        // Ui
        m_Ui = asset.FindActionMap("Ui", throwIfNotFound: true);
        m_Ui_Move = m_Ui.FindAction("Move", throwIfNotFound: true);
        m_Ui_Select = m_Ui.FindAction("Select", throwIfNotFound: true);
        m_Ui_Return = m_Ui.FindAction("Return", throwIfNotFound: true);
        m_Ui_Start = m_Ui.FindAction("Start", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Gameplay
    private readonly InputActionMap m_Gameplay;
    private IGameplayActions m_GameplayActionsCallbackInterface;
    private readonly InputAction m_Gameplay_Move;
    private readonly InputAction m_Gameplay_Foward;
    private readonly InputAction m_Gameplay_Back;
    private readonly InputAction m_Gameplay_Item;
    private readonly InputAction m_Gameplay_Drift;
    private readonly InputAction m_Gameplay_Start;
    public struct GameplayActions
    {
        private @Control m_Wrapper;
        public GameplayActions(@Control wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Gameplay_Move;
        public InputAction @Foward => m_Wrapper.m_Gameplay_Foward;
        public InputAction @Back => m_Wrapper.m_Gameplay_Back;
        public InputAction @Item => m_Wrapper.m_Gameplay_Item;
        public InputAction @Drift => m_Wrapper.m_Gameplay_Drift;
        public InputAction @Start => m_Wrapper.m_Gameplay_Start;
        public InputActionMap Get() { return m_Wrapper.m_Gameplay; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GameplayActions set) { return set.Get(); }
        public void SetCallbacks(IGameplayActions instance)
        {
            if (m_Wrapper.m_GameplayActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnMove;
                @Foward.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnFoward;
                @Foward.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnFoward;
                @Foward.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnFoward;
                @Back.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnBack;
                @Back.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnBack;
                @Back.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnBack;
                @Item.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnItem;
                @Item.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnItem;
                @Item.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnItem;
                @Drift.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnDrift;
                @Drift.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnDrift;
                @Drift.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnDrift;
                @Start.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnStart;
                @Start.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnStart;
                @Start.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnStart;
            }
            m_Wrapper.m_GameplayActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Foward.started += instance.OnFoward;
                @Foward.performed += instance.OnFoward;
                @Foward.canceled += instance.OnFoward;
                @Back.started += instance.OnBack;
                @Back.performed += instance.OnBack;
                @Back.canceled += instance.OnBack;
                @Item.started += instance.OnItem;
                @Item.performed += instance.OnItem;
                @Item.canceled += instance.OnItem;
                @Drift.started += instance.OnDrift;
                @Drift.performed += instance.OnDrift;
                @Drift.canceled += instance.OnDrift;
                @Start.started += instance.OnStart;
                @Start.performed += instance.OnStart;
                @Start.canceled += instance.OnStart;
            }
        }
    }
    public GameplayActions @Gameplay => new GameplayActions(this);

    // Ui
    private readonly InputActionMap m_Ui;
    private IUiActions m_UiActionsCallbackInterface;
    private readonly InputAction m_Ui_Move;
    private readonly InputAction m_Ui_Select;
    private readonly InputAction m_Ui_Return;
    private readonly InputAction m_Ui_Start;
    public struct UiActions
    {
        private @Control m_Wrapper;
        public UiActions(@Control wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Ui_Move;
        public InputAction @Select => m_Wrapper.m_Ui_Select;
        public InputAction @Return => m_Wrapper.m_Ui_Return;
        public InputAction @Start => m_Wrapper.m_Ui_Start;
        public InputActionMap Get() { return m_Wrapper.m_Ui; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UiActions set) { return set.Get(); }
        public void SetCallbacks(IUiActions instance)
        {
            if (m_Wrapper.m_UiActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_UiActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_UiActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_UiActionsCallbackInterface.OnMove;
                @Select.started -= m_Wrapper.m_UiActionsCallbackInterface.OnSelect;
                @Select.performed -= m_Wrapper.m_UiActionsCallbackInterface.OnSelect;
                @Select.canceled -= m_Wrapper.m_UiActionsCallbackInterface.OnSelect;
                @Return.started -= m_Wrapper.m_UiActionsCallbackInterface.OnReturn;
                @Return.performed -= m_Wrapper.m_UiActionsCallbackInterface.OnReturn;
                @Return.canceled -= m_Wrapper.m_UiActionsCallbackInterface.OnReturn;
                @Start.started -= m_Wrapper.m_UiActionsCallbackInterface.OnStart;
                @Start.performed -= m_Wrapper.m_UiActionsCallbackInterface.OnStart;
                @Start.canceled -= m_Wrapper.m_UiActionsCallbackInterface.OnStart;
            }
            m_Wrapper.m_UiActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Select.started += instance.OnSelect;
                @Select.performed += instance.OnSelect;
                @Select.canceled += instance.OnSelect;
                @Return.started += instance.OnReturn;
                @Return.performed += instance.OnReturn;
                @Return.canceled += instance.OnReturn;
                @Start.started += instance.OnStart;
                @Start.performed += instance.OnStart;
                @Start.canceled += instance.OnStart;
            }
        }
    }
    public UiActions @Ui => new UiActions(this);
    public interface IGameplayActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnFoward(InputAction.CallbackContext context);
        void OnBack(InputAction.CallbackContext context);
        void OnItem(InputAction.CallbackContext context);
        void OnDrift(InputAction.CallbackContext context);
        void OnStart(InputAction.CallbackContext context);
    }
    public interface IUiActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnSelect(InputAction.CallbackContext context);
        void OnReturn(InputAction.CallbackContext context);
        void OnStart(InputAction.CallbackContext context);
    }
}
