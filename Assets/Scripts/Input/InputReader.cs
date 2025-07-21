using Ebonflow.Input;
using System;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class InputReader
{
    private PlayerInput _playerInput;

    // QTE, Parry, Dodge 버튼 입력 시 호출할 이벤트
    public event Action OnSelectStarted;
    public event Action OnSelectPerformed;
    public event Action OnSelectCanceled;

    public Vector2 MousePosition { get; private set; }

    public InputReader()
    {
        _playerInput = new PlayerInput();
    }

    public void EnableEvents()
    {
        _playerInput.Enable();
        _playerInput.Player.Select.started += (ctx) => OnSelectStarted();
        _playerInput.Player.Select.performed += (ctx) => OnSelectPerformed();
        _playerInput.Player.Select.canceled += (ctx) => OnSelectCanceled();
        _playerInput.Player.MousePosition.performed += (ctx) => GetMousePosition(ctx);
    }

    public void GetMousePosition(CallbackContext callbackContext)
    {
        MousePosition = callbackContext.ReadValue<Vector2>();
    }

    public void DisableEvents()
    {
        _playerInput.Disable();
        _playerInput.Player.Select.started -= (ctx) => OnSelectStarted();
        _playerInput.Player.Select.performed -= (ctx) => OnSelectPerformed();
        _playerInput.Player.Select.canceled -= (ctx) => OnSelectCanceled();
        _playerInput.Player.MousePosition.performed -= (ctx) => GetMousePosition(ctx);
    }
}
