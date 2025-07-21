using Ebonflow.Input;
using System;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class InputReader : MonoBehaviour
{
    private PlayerInput _playerInput;
    private bool _isSelecting;

    // QTE, Parry, Dodge 버튼 입력 시 호출할 이벤트
    public event Action OnSelectStarted;
    public event Action OnSelectDown;
    public event Action OnSelectCanceled;

    public Vector2 MousePosition { get; private set; }

    private void Awake()
    {
        _playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        _playerInput.Enable();
        _playerInput.Player.Select.started += (ctx) => _isSelecting = true;
        _playerInput.Player.Select.canceled += (ctx) => _isSelecting = false;
        _playerInput.Player.Select.started += (ctx) => OnSelectStarted();
        _playerInput.Player.Select.canceled += (ctx) => OnSelectCanceled();
        _playerInput.Player.MousePosition.performed += (ctx) => GetMousePosition(ctx);
    }

    private void Update()
    {
        if (_isSelecting)
        {
            OnSelectDown?.Invoke();
        }
    }

    public void GetMousePosition(CallbackContext callbackContext)
    {
        MousePosition = callbackContext.ReadValue<Vector2>();
    }

    private void OnDisable()
    {
        _playerInput.Disable();
        _playerInput.Player.Select.started -= (ctx) => _isSelecting = true;
        _playerInput.Player.Select.canceled -= (ctx) => _isSelecting = false;
        _playerInput.Player.Select.started -= (ctx) => OnSelectStarted();
        _playerInput.Player.Select.performed -= (ctx) => OnSelectDown();
        _playerInput.Player.Select.canceled -= (ctx) => OnSelectCanceled();
        _playerInput.Player.MousePosition.performed -= (ctx) => GetMousePosition(ctx);
    }
}
