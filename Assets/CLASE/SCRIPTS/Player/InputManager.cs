using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class InputManager : MonoBehaviour
{
    private static InputManager _instance = null;
    public static InputManager Instance { get => _instance; private set => _instance = value; }

    private PlayerControls playerControls;
    private bool shootPressed;
    private bool meleePressed;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            Debug.Log("Instance created");
        }
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Player.Fire.performed += OnFire;
        playerControls.Player.Crouch.performed += OnMelee;
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Player.Fire.performed -= OnFire;
        playerControls.Player.Crouch.performed -= OnMelee;
        playerControls.Disable();
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        shootPressed = true;
    }

    private void OnMelee(InputAction.CallbackContext context)
    {
        meleePressed = true;
    }

    public Vector2 GetMoveInput()
    {
        return playerControls.Player.Move.ReadValue<Vector2>();
    }

    public bool IsMoveInputPressed()
    {
        return playerControls.Player.Move.IsPressed();
    }

    public bool WasRunInputPressed()
    {
        return playerControls.Player.Run.IsPressed();
    }

    public bool IsMovingBackwards()
    {
        return playerControls.Player.Move.ReadValue<Vector2>().y < 0;
    }

    public bool IsMovingOnXAxis()
    {
        return playerControls.Player.Move.ReadValue<Vector2>().x != 0;
    }

    public Vector2 GetMouseDelta()
    {
        return playerControls.Player.Look.ReadValue<Vector2>();
    }

    
    public bool IsShooting()
    {
        bool isShooting = shootPressed;
        shootPressed = false;
        return isShooting;
    }

    public bool IsMeleeAttacking()
    {
        bool isMeleeAttacking = meleePressed;
        meleePressed = false;
        return isMeleeAttacking;
    }
}
