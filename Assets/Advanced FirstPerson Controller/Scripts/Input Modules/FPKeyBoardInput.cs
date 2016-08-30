using UnityEngine;
using System.Collections;
using System;

namespace AdvancedFPController
{
    public class FPKeyBoardInput : MonoBehaviour, FPInput
    {
        public KeyCode walkF = KeyCode.W;
        public KeyCode walkB = KeyCode.S;
        public KeyCode strafeR = KeyCode.D;
        public KeyCode strafeL = KeyCode.A;

        public KeyCode lookUp = KeyCode.UpArrow;
        public KeyCode lookDown = KeyCode.DownArrow;
        public KeyCode lookRight = KeyCode.RightArrow;
        public KeyCode lookLeft = KeyCode.LeftArrow;

        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode sprintKey = KeyCode.LeftShift;
        public KeyCode crouchKey = KeyCode.C;
        public KeyCode proneKey = KeyCode.LeftControl;
        public KeyCode leanRightKey = KeyCode.E;
        public KeyCode leanLeftKey = KeyCode.Q;
        
        [SerializeField]
        Vector2 move;
        [SerializeField]
        Vector2 look;
        [SerializeField]
        Vector2 altLook;

        [SerializeField]
        float lookDelta = 5;

        [SerializeField]
        bool jump;
        [SerializeField]
        bool sprint;
        [SerializeField]
        bool crouch;
        [SerializeField]
        bool prone;

        [SerializeField]
        bool leanRight;
        [SerializeField]
        bool leanLeft;

        Vector2 FPInput.move
        {
            get
            {
                return move;
            }
        }

        Vector2 FPInput.look
        {
            get
            {
                return look;
            }
        }

        Vector2 FPInput.altLook
        {
            get
            {
                return altLook;
            }
        }

        bool FPInput.crouchToggle
        {
            get
            {
                return crouch;
            }
        }

        bool FPInput.ProneToggle
        {
            get
            {
                return prone;
            }
        }

        bool FPInput.jump
        {
            get
            {
                return jump;
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
                return leanRight;
            }
        }

        bool FPInput.leanLeft
        {
            get
            {
                return leanLeft;
            }
        }

        void FPInput.UpdateInput()
        {
            GetMove();
            GetLook();
            GetAltLook();

            GetJump();
            GetSprint();
            GetCrouch();
            GetProne();

            GetLeanRight();
            GetLeanLeft();
        }

        public void GetMove()
        {
            move.y = CharacterTools.GetAxisRaw(walkF, walkB, move.y);
            move.x = CharacterTools.GetAxisRaw(strafeR, strafeL, move.x);
        }

        public void GetLook()
        {
            look.x = Input.GetAxis("Mouse X");
            look.y = Input.GetAxis("Mouse Y");
        }

        public void GetAltLook()
        {
            altLook.x = CharacterTools.GetAxis(lookRight, lookLeft, altLook.x, lookDelta * Time.deltaTime);
            altLook.y = CharacterTools.GetAxis(lookUp, lookDown, altLook.y, lookDelta * Time.deltaTime);
        }

        void GetJump()
        {
            jump = Input.GetKeyDown(jumpKey);
        }

        void GetSprint()
        {
            sprint = Input.GetKey(sprintKey);
        }

        void GetCrouch()
        {
            crouch = Input.GetKeyDown(crouchKey);
        }

        void GetProne()
        {
            prone = Input.GetKeyDown(proneKey);
        }

        void GetLeanRight()
        {
            leanRight = Input.GetKey(leanRightKey);
        }

        void GetLeanLeft()
        {
            leanLeft = Input.GetKey(leanLeftKey);
        }
    }

    public static class CharacterTools
    {
        public static float GetAxis(KeyCode positive, KeyCode negative, float current, float axisDelta, bool snap = true)
        {
            if (Input.GetKey(positive))
            {
                if (snap)
                    return Mathf.Clamp(Mathf.MoveTowards(current, 1, axisDelta), 0, 1);
                else
                    return Mathf.MoveTowards(current, 1, axisDelta);
            }

            if (Input.GetKey(negative))
            {
                if (snap)
                    return Mathf.Clamp(Mathf.MoveTowards(current, -1, axisDelta), -1, 0);
                else
                    return Mathf.MoveTowards(current, -1, axisDelta);
            }

            return Mathf.MoveTowards(current, 0, axisDelta);
        }

        public static float GetAxisRaw(KeyCode positive, KeyCode negative, float current)
        {
            if (Input.GetKey(positive))
            {
                return 1;
            }

            if (Input.GetKey(negative))
            {
                return -1;
            }

            return 0;
        }
    }
}