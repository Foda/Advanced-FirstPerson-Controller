using UnityEngine;
using UnityEngine.EventSystems;

using AdvancedFPController;
using UnityEngine.UI;
using System;

public class FPTouchInput : MonoBehaviour, FPInput
{
    [SerializeField]
    VirtualJoystick walkJoystick;

    [SerializeField]
    VirtualJoystick lookJoystick;

    [SerializeField]
    Button jumpB;

    [SerializeField]
    Button sprintB;

    [SerializeField]
    Button croucB;

    [SerializeField]
    Button proneB;

    [SerializeField]
    Button leanRB;
    [SerializeField]
    Button leanLB;

    bool jump;

    bool sprintBegin;

    bool sprint;

    bool sprintEnd;

    bool crouch;
    bool prone;

    bool leanR;
    bool leanL;

    Vector2 FPInput.move
    {
        get
        {
            return walkJoystick.value;
        }
    }

    Vector2 FPInput.look
    {
        get
        {
            return lookJoystick.value;
        }
    }

    Vector2 FPInput.altLook
    {
        get
        {
            return Vector2.zero;
        }
    }

    bool FPInput.crouchToggle
    {
        get
        {
            if (crouch)
            {
                crouch = false;
                return true;
            }
            else
                return false;
        }
    }

    bool FPInput.ProneToggle
    {
        get
        {
            if (prone)
            {
                prone = false;
                return true;
            }
            else
                return false;
        }
    }

    bool FPInput.jump
    {
        get
        {
            if (jump)
            {
                jump = false;
                return true;
            }
            else
                return false;
        }
    }

    bool FPInput.sprint
    {
        get
        {
            return sprint;
        }
    }

    bool FPInput.leanRight
    {
        get
        {
            return leanR;
        }
    }

    bool FPInput.leanLeft
    {
        get
        {
            return leanL;
        }
    }

    void Start()
    {
        jumpB.onClick.AddListener(Jump);
        sprintB.onClick.AddListener(ToggleSprint);
        croucB.onClick.AddListener(ToggleCrouch);
        proneB.onClick.AddListener(ToggleProne);

        leanRB.onClick.AddListener(()=> ToggleLean(1));
        leanLB.onClick.AddListener(()=> ToggleLean(-1));
    }

    void Jump()
    {
        jump = true;
    }

    void ToggleSprint()
    {
        sprint = !sprint;
    }

    void ToggleCrouch()
    {
        crouch = !crouch;
    }

    void ToggleProne()
    {
        prone = !prone;
    }

    void ToggleLean(int direction)
    {
        leanR = direction > 0 && !leanR;
        leanL = direction < 0 && !leanL;
    }

    public void UpdateInput()
    {
        
    }
}