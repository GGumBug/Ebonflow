using static UnityEngine.InputSystem.InputAction;
using UnityEngine;
using System;

namespace StageEditor.Input
{
    public class StageEditorInputReader : MonoBehaviour
    {
        StageEditorInput _input;

        public event Action OnRightMouseStarted;

        public Vector2 MousePosition { get; private set; }

        private void Awake()
        {
            _input = new StageEditorInput();
        }

        private void OnEnable()
        {
            _input.StageEditor.Enable();
            _input.StageEditor.MousePosition.performed += (ctx) => GetMousePosition(ctx);
            _input.StageEditor.RightMouse.started += (ctx) => OnRightMouseStarted();
        }

        public void GetMousePosition(CallbackContext callbackContext)
        {
            MousePosition = callbackContext.ReadValue<Vector2>();
        }

        private void OnDisable()
        {
            _input.StageEditor.MousePosition.performed -= (ctx) => GetMousePosition(ctx);
            _input.StageEditor.RightMouse.started -= (ctx) => OnRightMouseStarted();
            _input.StageEditor.Disable();
        }
    }
}