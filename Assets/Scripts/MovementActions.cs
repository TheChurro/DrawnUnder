// GENERATED AUTOMATICALLY FROM 'Assets/MovementActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @MovementActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @MovementActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""MovementActions"",
    ""maps"": [
        {
            ""name"": ""Gameplay"",
            ""id"": ""1267a9a5-d3a8-4d45-9ac5-94c955df34af"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""0fb1b329-9b8f-4d57-a194-0773355a25a7"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""5a45382a-6251-4539-b3bc-de4defe67803"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""ce117f4b-b518-4a57-a7ef-57ec352fab0d"",
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
                    ""id"": ""bffd23f2-2632-4738-9c47-25c1a3c5e57f"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""3a7945d0-25d5-4a3c-b135-38afbf200ae4"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""7c6631e0-73f9-4f63-851f-2654a845d0d5"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""58f6367a-9b5f-4ce6-a37f-d1c0d18e1254"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""0946cf98-3126-4191-aa15-784424a057aa"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Recording"",
            ""id"": ""065a7f4f-3118-408e-bc60-125bd8204d33"",
            ""actions"": [
                {
                    ""name"": ""Record"",
                    ""type"": ""Button"",
                    ""id"": ""c6ff84fd-5b3f-4409-a64c-5d191955b14e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Play"",
                    ""type"": ""Button"",
                    ""id"": ""846161f0-8d0e-42b2-886d-3581a980717d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Advance"",
                    ""type"": ""Button"",
                    ""id"": ""61fd8268-a533-45a6-8549-860a8e6cfab1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Reverse"",
                    ""type"": ""Button"",
                    ""id"": ""0b0a76a1-0f22-418a-b55a-94383326b934"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""b2dbff1f-4a53-4ffc-a08b-e58292d58898"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Record"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""37d40a51-1a47-4e15-a062-0a00b16a5d94"",
                    ""path"": ""<Keyboard>/p"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Play"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1f82aeb2-e8ae-45f9-be6e-7ca0a6730496"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Advance"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d08b0b4c-4f58-4d81-8e64-dbddc4cfad51"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Reverse"",
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
        m_Gameplay_Jump = m_Gameplay.FindAction("Jump", throwIfNotFound: true);
        // Recording
        m_Recording = asset.FindActionMap("Recording", throwIfNotFound: true);
        m_Recording_Record = m_Recording.FindAction("Record", throwIfNotFound: true);
        m_Recording_Play = m_Recording.FindAction("Play", throwIfNotFound: true);
        m_Recording_Advance = m_Recording.FindAction("Advance", throwIfNotFound: true);
        m_Recording_Reverse = m_Recording.FindAction("Reverse", throwIfNotFound: true);
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
    private readonly InputAction m_Gameplay_Jump;
    public struct GameplayActions
    {
        private @MovementActions m_Wrapper;
        public GameplayActions(@MovementActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Gameplay_Move;
        public InputAction @Jump => m_Wrapper.m_Gameplay_Jump;
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
                @Jump.started -= m_Wrapper.m_GameplayActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_GameplayActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_GameplayActionsCallbackInterface.OnJump;
            }
            m_Wrapper.m_GameplayActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
            }
        }
    }
    public GameplayActions @Gameplay => new GameplayActions(this);

    // Recording
    private readonly InputActionMap m_Recording;
    private IRecordingActions m_RecordingActionsCallbackInterface;
    private readonly InputAction m_Recording_Record;
    private readonly InputAction m_Recording_Play;
    private readonly InputAction m_Recording_Advance;
    private readonly InputAction m_Recording_Reverse;
    public struct RecordingActions
    {
        private @MovementActions m_Wrapper;
        public RecordingActions(@MovementActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Record => m_Wrapper.m_Recording_Record;
        public InputAction @Play => m_Wrapper.m_Recording_Play;
        public InputAction @Advance => m_Wrapper.m_Recording_Advance;
        public InputAction @Reverse => m_Wrapper.m_Recording_Reverse;
        public InputActionMap Get() { return m_Wrapper.m_Recording; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(RecordingActions set) { return set.Get(); }
        public void SetCallbacks(IRecordingActions instance)
        {
            if (m_Wrapper.m_RecordingActionsCallbackInterface != null)
            {
                @Record.started -= m_Wrapper.m_RecordingActionsCallbackInterface.OnRecord;
                @Record.performed -= m_Wrapper.m_RecordingActionsCallbackInterface.OnRecord;
                @Record.canceled -= m_Wrapper.m_RecordingActionsCallbackInterface.OnRecord;
                @Play.started -= m_Wrapper.m_RecordingActionsCallbackInterface.OnPlay;
                @Play.performed -= m_Wrapper.m_RecordingActionsCallbackInterface.OnPlay;
                @Play.canceled -= m_Wrapper.m_RecordingActionsCallbackInterface.OnPlay;
                @Advance.started -= m_Wrapper.m_RecordingActionsCallbackInterface.OnAdvance;
                @Advance.performed -= m_Wrapper.m_RecordingActionsCallbackInterface.OnAdvance;
                @Advance.canceled -= m_Wrapper.m_RecordingActionsCallbackInterface.OnAdvance;
                @Reverse.started -= m_Wrapper.m_RecordingActionsCallbackInterface.OnReverse;
                @Reverse.performed -= m_Wrapper.m_RecordingActionsCallbackInterface.OnReverse;
                @Reverse.canceled -= m_Wrapper.m_RecordingActionsCallbackInterface.OnReverse;
            }
            m_Wrapper.m_RecordingActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Record.started += instance.OnRecord;
                @Record.performed += instance.OnRecord;
                @Record.canceled += instance.OnRecord;
                @Play.started += instance.OnPlay;
                @Play.performed += instance.OnPlay;
                @Play.canceled += instance.OnPlay;
                @Advance.started += instance.OnAdvance;
                @Advance.performed += instance.OnAdvance;
                @Advance.canceled += instance.OnAdvance;
                @Reverse.started += instance.OnReverse;
                @Reverse.performed += instance.OnReverse;
                @Reverse.canceled += instance.OnReverse;
            }
        }
    }
    public RecordingActions @Recording => new RecordingActions(this);
    public interface IGameplayActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
    }
    public interface IRecordingActions
    {
        void OnRecord(InputAction.CallbackContext context);
        void OnPlay(InputAction.CallbackContext context);
        void OnAdvance(InputAction.CallbackContext context);
        void OnReverse(InputAction.CallbackContext context);
    }
}
