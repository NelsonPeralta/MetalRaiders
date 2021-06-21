// GENERATED AUTOMATICALLY FROM 'Assets/Ressources/CubeInput.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @CubeInput : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @CubeInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""CubeInput"",
    ""maps"": [
        {
            ""name"": ""Cube"",
            ""id"": ""f29576bb-4066-40de-9864-124598b11df2"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""02388831-e0fe-4974-984c-3f071aac8a73"",
                    ""expectedControlType"": ""Stick"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Move Up"",
                    ""type"": ""Button"",
                    ""id"": ""6e22917c-dd93-44ea-8327-f9a6b9338071"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Move Down"",
                    ""type"": ""Button"",
                    ""id"": ""46a6bc8e-2210-4bdd-9951-0df96da2ca4a"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""c3044be4-b957-4f8a-b1d7-32ee2fd9101e"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""733a8e0f-de76-44da-b060-b75163fc7cc8"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move Up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3cc359ba-5909-4542-8768-bc610b127a9e"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Cube
        m_Cube = asset.FindActionMap("Cube", throwIfNotFound: true);
        m_Cube_Move = m_Cube.FindAction("Move", throwIfNotFound: true);
        m_Cube_MoveUp = m_Cube.FindAction("Move Up", throwIfNotFound: true);
        m_Cube_MoveDown = m_Cube.FindAction("Move Down", throwIfNotFound: true);
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

    // Cube
    private readonly InputActionMap m_Cube;
    private ICubeActions m_CubeActionsCallbackInterface;
    private readonly InputAction m_Cube_Move;
    private readonly InputAction m_Cube_MoveUp;
    private readonly InputAction m_Cube_MoveDown;
    public struct CubeActions
    {
        private @CubeInput m_Wrapper;
        public CubeActions(@CubeInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Cube_Move;
        public InputAction @MoveUp => m_Wrapper.m_Cube_MoveUp;
        public InputAction @MoveDown => m_Wrapper.m_Cube_MoveDown;
        public InputActionMap Get() { return m_Wrapper.m_Cube; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CubeActions set) { return set.Get(); }
        public void SetCallbacks(ICubeActions instance)
        {
            if (m_Wrapper.m_CubeActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_CubeActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_CubeActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_CubeActionsCallbackInterface.OnMove;
                @MoveUp.started -= m_Wrapper.m_CubeActionsCallbackInterface.OnMoveUp;
                @MoveUp.performed -= m_Wrapper.m_CubeActionsCallbackInterface.OnMoveUp;
                @MoveUp.canceled -= m_Wrapper.m_CubeActionsCallbackInterface.OnMoveUp;
                @MoveDown.started -= m_Wrapper.m_CubeActionsCallbackInterface.OnMoveDown;
                @MoveDown.performed -= m_Wrapper.m_CubeActionsCallbackInterface.OnMoveDown;
                @MoveDown.canceled -= m_Wrapper.m_CubeActionsCallbackInterface.OnMoveDown;
            }
            m_Wrapper.m_CubeActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @MoveUp.started += instance.OnMoveUp;
                @MoveUp.performed += instance.OnMoveUp;
                @MoveUp.canceled += instance.OnMoveUp;
                @MoveDown.started += instance.OnMoveDown;
                @MoveDown.performed += instance.OnMoveDown;
                @MoveDown.canceled += instance.OnMoveDown;
            }
        }
    }
    public CubeActions @Cube => new CubeActions(this);
    public interface ICubeActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnMoveUp(InputAction.CallbackContext context);
        void OnMoveDown(InputAction.CallbackContext context);
    }
}