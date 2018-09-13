using UnityEngine;

public class MovementBase : PlayerBase
{
        public bool IsJumping, IsRushing, IsSideMoving,IsBackMoving;
        public int CrouchState;

        private CharacterController Player;
        private Transform PlayerTransform, Eyes;

        private Vector3 MoveDir, JumpDir;

        private float Sensitivity, RotationX, RotationY;
        private float FallVelocity;
        private float BobVir, BobFrame, StepSize;
        private float CurretnHight, StandHight, CrouchHight;
        private float CrouchFrame, CrouchTime;

        private AnimationCurve BobCurve, CrouchCurve;

        public enum State
        {
            stand_idle,
            crouch_idle,
            down,
            up
        }
        
        public void SetPlayer(GameObject Object)
        {
            Player = Object.GetComponent<CharacterController>();
            PlayerTransform = Object.GetComponent<Transform>();
            Eyes = Object.GetComponentInChildren<Camera>().gameObject.transform;
            Object.GetComponentInChildren<Camera>().enabled = true;
            StandHight = Player.height;
            CrouchHight = StandHight * 0.55f;

            Sensitivity = 10.0f;

            BobVir = 1.0f;
            StepSize = Player.stepOffset * 6.0f;
            BobCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(StepSize * 0.5f, 1.0f), new Keyframe(StepSize, 0.0f));
        }

        public void UpdateView()
        {
            if (Player.isGrounded)
            {
                BobFrame += Player.velocity.magnitude * Time.deltaTime;
                if (BobFrame > StepSize)
                {
                    BobVir *= -1.0f;
                    BobFrame = 0.0f;
                    FootStep();
                }
            }

            RotationY = PlayerTransform.eulerAngles.y + Input.GetAxis("Mouse X") * Sensitivity;
            PlayerTransform.eulerAngles = new Vector3(0.0f, RotationY, 0.0f);
            RotationX = Mathf.Clamp(RotationX + Input.GetAxis("Mouse Y") * Sensitivity, -60.0f, 60.0f);
            Eyes.eulerAngles = new Vector3(-RotationX + BobCurve.Evaluate(BobFrame) * (IsRushing ? 2.0f : 1.0f), RotationY + BobVir * BobCurve.Evaluate(BobFrame) * (IsRushing ? 1.5f : 0.5f), 0.0f);
        }


        public void UpdateInput()
        {
            JumpDir = Vector3.zero;
            IsSideMoving = false;
            IsBackMoving = false;

            if (!Player.isGrounded)
            {
                if (Input.GetKey(KeyCode.W)) JumpDir += PlayerTransform.forward;
                else if (Input.GetKey(KeyCode.S)) JumpDir -= PlayerTransform.forward;
                if (Input.GetKey(KeyCode.A)) JumpDir -= PlayerTransform.right;
                else if (Input.GetKey(KeyCode.D)) JumpDir += PlayerTransform.right;

                return;
            }

            if (Input.GetKey(KeyCode.Space) && !IsJumping && !IsRushing && CrouchState == (int)State.stand_idle)
            {
                Jump();
                return;
            }

            MoveDir = Vector3.zero;

            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (CrouchState == (int)State.stand_idle || CrouchState == (int)State.up)
                    Crouch();
            }
            else if ((CrouchState == (int)State.crouch_idle || CrouchState == (int)State.down) && CanStand()) Stand();

            if (Input.GetKey(KeyCode.W))
            {
                MoveDir += PlayerTransform.forward;
                if (Input.GetKey(KeyCode.LeftShift) && CrouchState == (int)State.stand_idle)
                {
                    IsRushing = true;
                    return;
                }
            }
            else
                if (Input.GetKey(KeyCode.S))
                {
                    MoveDir -= PlayerTransform.forward;
                    IsBackMoving = true;
                }

            if (Input.GetKey(KeyCode.A))
            {
                MoveDir -= PlayerTransform.right;
                IsSideMoving = true;
            }
            else
                if (Input.GetKey(KeyCode.D))
                {
                    MoveDir += PlayerTransform.right;
                    IsSideMoving = true;
                }

            IsRushing = false;
        }

        public void UpdateMove()
        {
            if (!Player.isGrounded && FallVelocity > 0.0f) MoveDir *= 0.96f;
            else if (IsJumping) Land();

            float CurrentSpeed = IsRushing ? RushSpeed : WalkSpeed;
            if (CrouchState != (int)State.stand_idle) CurrentSpeed = CrouchSpeed;
            else
            {
                if (IsBackMoving) CurrentSpeed *= 0.8f;
                if (IsSideMoving) CurrentSpeed *= 0.7f;
            }

            CurrentSpeed *= BaseWeight / (BaseWeight + AdditionWeight);

            Player.Move((new Vector3(MoveDir.x * CurrentSpeed, FallVelocity += Physics.gravity.y * Time.fixedDeltaTime, MoveDir.z * CurrentSpeed) + JumpDir) * Time.fixedDeltaTime);

            if (CrouchState == (int)State.down)
            {
                CrouchTime = CurretnHight / StandHight * 0.2f;
                CrouchCurve = new AnimationCurve(new Keyframe(0.0f, CurretnHight), new Keyframe(CrouchTime, CrouchHight));

                CrouchFrame += Time.deltaTime;

                if (CrouchFrame > CrouchTime)
                {
                    CrouchState = (int)State.crouch_idle;
                    Player.height = CrouchHight;
                    return;
                }

                PlayerTransform.position -= new Vector3(0.0f, (Player.height - CrouchCurve.Evaluate(CrouchFrame)) * 0.5f, 0.0f);
                Player.height = CrouchCurve.Evaluate(CrouchFrame);

                return;
            }

            if (CrouchState != (int)State.up)
                return;

            CrouchTime = (StandHight - CurretnHight) / (StandHight - CrouchHight) * 0.2f;
            CrouchCurve = new AnimationCurve(new Keyframe(0.0f, CurretnHight), new Keyframe(CrouchTime, StandHight));
            CrouchFrame += Time.deltaTime;

            if (CrouchFrame > CrouchTime)
            {
                CrouchState = (int)State.stand_idle;
                Player.height = StandHight;
                return;
            }

            PlayerTransform.position += new Vector3(0.0f, (CrouchCurve.Evaluate(CrouchFrame) - Player.height) * 0.5f, 0.0f);
            Player.height = CrouchCurve.Evaluate(CrouchFrame);
        }

        virtual public void Jump()
        {
            IsJumping = true;
            FallVelocity = JumpSpeed;
        }

        virtual public void Land()
        {
            IsJumping = false;
        }

        virtual public void Crouch()
        {
            CrouchFrame = 0.0f;
            CurretnHight = Player.height;
            CrouchState = (int)State.down;
        }

        virtual public void Stand()
        {
            CrouchFrame = 0.0f;
            CurretnHight = Player.height;
            CrouchState = (int)State.up;
        }

        virtual public void FootStep()
        {
        }

        private bool CanStand()
        {
            RaycastHit hit;
            if(Physics.CapsuleCast(PlayerTransform.position, PlayerTransform.position + Vector3.up * Player.height * 0.5f, Player.radius * 0.9f, PlayerTransform.up, out hit, StandHight - CrouchHight))
            return false;

            return true;
        }
}