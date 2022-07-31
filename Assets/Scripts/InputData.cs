using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace P90brush
{
    public class InputData
    {
        public float ForwardMove = 0f;
        public float SideMove = 0f;
        public float MouseX = 0f;
        public float MouseY = 0f;
        public bool JumpPressed = false;
        public bool JumpPressedLastUpdate = false;
        public bool ResetPressed = false;
        public bool HookPressed = false;
        public bool HookPressedLastUpdate = false;
        public bool HookRetractPressed = false;
        public bool SlowMotionPressed = false;

        public void Reset() {
            this.ForwardMove = 0f;
            this.SideMove = 0f;
            this.MouseX = 0f;
            this.MouseY = 0f;
            this.JumpPressed = false;
            this.JumpPressedLastUpdate = false;
            this.ResetPressed = false;
            this.HookPressed = false;
            this.HookPressedLastUpdate = false;
            this.HookRetractPressed = false;
            this.SlowMotionPressed = false;
        }

        public void Update(MovementConfig MoveConfig) {
            #region Catch Movement
            var moveLeft = Input.GetKey(MoveConfig.MoveLeft);
            var moveRight = Input.GetKey(MoveConfig.MoveRight);
            var moveFwd = Input.GetKey(MoveConfig.MoveForward);
            var moveBack = Input.GetKey(MoveConfig.MoveBack);

            // Lateral Movements
            if (moveLeft) {
                SideMove = -MoveConfig.Accel;
            } else if (moveRight) {
                SideMove = MoveConfig.Accel;
            } else {
                SideMove = 0;
            }

            // Frontal Movements
            if (moveFwd) {
                ForwardMove = MoveConfig.Accel;
            } else if (moveBack) {
                ForwardMove = -MoveConfig.Accel;
            } else {
                ForwardMove = 0;
            }
            #endregion


            #region Catch Action
            ResetPressed = Input.GetKey(MoveConfig.ResetButton);

            JumpPressedLastUpdate = JumpPressed;
            JumpPressed = Input.GetKey(MoveConfig.JumpButton);

            HookPressedLastUpdate = HookPressed;
            HookPressed = Input.GetButton(MoveConfig.HookButton);
            HookRetractPressed = Input.GetKey(MoveConfig.HookRetractButton);

            SlowMotionPressed = Input.GetKey(MoveConfig.SlowMotion);
            #endregion


            #region Catch Mouse mouvement
            this.MouseX = (Input.GetAxis("Mouse X") * MoveConfig.XSens * .02f);
            this.MouseY = Input.GetAxis("Mouse Y") * MoveConfig.YSens * .02f;
            #endregion
        }
    }
}