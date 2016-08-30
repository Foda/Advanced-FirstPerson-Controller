using UnityEngine;

namespace AdvancedFPController
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]

    public class FirstPersonController : MonoBehaviour
    {
        [Header("Restriction")]
        [SerializeField]
        internal bool canMove = true;
        [SerializeField]
        internal bool canLook = true;
        [SerializeField]
        internal bool canLean = true;
        [SerializeField]
        internal bool canJump = true;
        [SerializeField]
        internal bool canSprint = true;
        [SerializeField]
        internal bool canCrouch = true;
        [SerializeField]
        internal bool canProne = true;
        [SerializeField]
        internal bool canSlide = true;

        [SerializeField]
        SprintRestriction sprintRestriction = SprintRestriction.forwardAndSides;

        bool inAir;
        public bool InAir { get { return inAir; } }

        //status variables
        bool onGround;
        public bool OnGround { get { return onGround; } }

        bool jumping;
        public bool Jumping { get { return jumping; } }

        bool falling;
        public bool Falling { get { return falling; } }

        //movement variables

        Vector3 forwardDirection;
        Vector3 rightDirection;

        [Header("Movement")]
        [SerializeField]
        Vector2 speed;

        [SerializeField]
        bool snapSpeedY = true;
        [SerializeField]
        bool snapSpeedX = true;

        Vector2 Speed { get { return speed; } }
        Acceleration acceleration;

        [SerializeField]
        [Tooltip("The raycast length to check if we are on ground and if we hit our head")]
        float CheckDistance = 0.15f;
        [SerializeField]
        [Tooltip("Part Will Allow To Try To Change State Even If The Current Space Doesnt Alow For That, Full Will Block")]
        HeadHitDetect headHitDetect;
        [SerializeField]
        float headHitCheckDistance = 0.05f;
        [SerializeField]
        InAirDirection inAirDirection = InAirDirection.JumpDirecion;

        
        //look variables
        [Header("Look")]
        [SerializeField]
        float lookSens = 2.5f;

        
        [SerializeField]
        Vector2 cameraLimmit = new Vector2(80, 280);

        [Header("Gravity")]
        //interfal variables
        [SerializeField]
        float maxGravity = 20;

        float gravity;

        [SerializeField]
        float gravityDelta = 15;

        [Header("Jump")]
        [SerializeField]
        float maxJumpPower = 5;

        float jumpPower;

        [SerializeField]
        float jumpPowerDelta = 10;

        [Header("Sliding")]
        [SerializeField]
        float maxSlidePower = 6;

        float slidePower;

        [SerializeField]
        float slideDelta = 5;

        [SerializeField]
        Vector2 slideLookLimmit = new Vector2(80, 280);
        [SerializeField]
        SlideCameraResetMethod cameraResetMethod = SlideCameraResetMethod.InstantToCamera;
        [SerializeField]
        float slideLookResetDelta = 200;

        [Header("Leaning")]
        //leaning
        [SerializeField]
        float maxLean = 40;
        float lean = 0.5f;
        public float LeanAmmout { get { return lean; } }

        [SerializeField]
        float leanDelta = 2;

        [SerializeField]
        bool AlignCamRotation = true;

        [Header("Character States")]
        //character states
        [SerializeField]
        CharacterState normal;
        [SerializeField]
        CharacterState sprint;
        [SerializeField]
        CharacterState crouch;
        [SerializeField]
        CharacterState slide;
        [SerializeField]
        CharacterState prone;

        CharacterState desiredState;

        [SerializeField]
        bool changingState;

        public bool ChangingStates { get { return changingState; } }

        float wInterval;

        bool standing = true;
        public bool Standing { get { return standing; } }

        bool sprinting;
        public bool Sprinting { get { return sprinting; } }

        bool crouching;
        public bool Crouching { get { return crouching; } }

        bool proning;
        public bool Proning { get { return proning; } }

        bool sliding;
        public bool Sliding { get { return sliding; } }

        bool leaning;
        public bool Leaning { get { return leaning; } }

        State characterState;
        public State CharacterState { get { return characterState; } }

        //auto get
        CharacterController ch;
        FPInput fpi;
        AudioSource aud;
        Transform camPivot;
        Transform cam;

        Vector3 charRotation;
        Vector3 camRotation;

        Vector3 camPosition;

        Vector3 fallBegin;

        bool aligningView;

        bool beganMovement;

        Vector3 leanDirection;

        //character events
        public delegate void CharacterJumped();
        public CharacterJumped OnCharacterJumped;

        public delegate void CharacterSlideBegin();
        public CharacterSlideBegin OnCharacterSlideBegin;

        public delegate void CharacterSlideEnd();
        public CharacterSlideEnd OnCharacterSlideEnd;

        public delegate void CharacterFell();
        public CharacterFell OnCharacterFell;

        public delegate void CharacterLeftGround();
        public CharacterLeftGround OnCharacterLeftGround;

        public delegate void CharacterLanded(float distanceTraveld, float distanceFell);
        public CharacterLanded OnCharacterLanded;

        public delegate void CharacterStateChanged(State newState);
        public CharacterStateChanged OnCharacterStateChanged;

        void Start()
        {
            ch = GetComponent<CharacterController>();
            fpi = GetComponent<FPInput>();

#if UNITY_EDITOR
            if (fpi == null)
                print("No Input Module Input Found, Input Modules should be attached to the player and inherit from the FPBaseInput class");
#endif

            aud = GetComponent<AudioSource>();
            cam = transform.Find("CamPivot/Camera");
            camPivot = transform.Find("CamPivot");

            desiredState = normal;
            acceleration = desiredState.onGround;
            standing = true;

            charRotation = transform.localEulerAngles;
            camRotation = transform.localEulerAngles;

            gravity = maxGravity;
        }

        void Update()
        {
            fpi.UpdateInput();

            Move();
            Look();
            CheckState();
            Lean();
        }

        void Move()
        {
            onGround = CheckGrounded();

            if (ch.isGrounded)
                onGround = true;

            AccelerateHorizonatal();
            AccelerateVertical();

            Vector3 moveDirection = transform.right + transform.forward;

            if (onGround)
            {
                rightDirection = transform.right;
                forwardDirection = transform.forward;

                acceleration = desiredState.onGround;

                if (falling)
                {
                    OnLanded();
                }

                if (speed.magnitude != 0 && desiredState.stepInterval != 0)
                {
                    if (!beganMovement)
                    {
                        beganMovement = true;
                        PlayWalkSound();
                    }

                    wInterval += Time.deltaTime;
                    if (wInterval >= desiredState.stepInterval)
                    {
                        wInterval = 0;
                        PlayWalkSound();
                    }
                }

                else
                {
                    wInterval = 0;
                    beganMovement = false;
                }
            }

            else 
            {
                acceleration = desiredState.inAir;

                if (!falling)
                {
                    if (!inAir)
                    {
                        OnLeftGround();
                    }

                    if (!jumping)
                    {
                        OnFell();
                    }
                }

                if (falling)
                    Fall();

                if (inAirDirection == InAirDirection.CharacterDirection)
                {
                    rightDirection = transform.right;
                    forwardDirection = transform.forward;
                }

                //in air movement
                if (canMove)
                {
                    if (inAirDirection == InAirDirection.JumpDirecion)
                    {
                        
                    }
                }
            }

            if (jumping)
            {
                Jump();
            }

            moveDirection = rightDirection * speed.x + forwardDirection * (speed.y + slidePower) + transform.up * (-gravity + jumpPower);

            ch.Move(moveDirection * Time.deltaTime);
        }

        void AccelerateVertical()
        {
            if (fpi.move.y > 0)
            {
                if (snapSpeedY && onGround && speed.y < 0)
                    speed.y = 0;

                speed.y = Mathf.MoveTowards(speed.y, desiredState.speed.y, acceleration.acceleration * Time.deltaTime);
            }
            else if (fpi.move.y < 0)
            {
                if (snapSpeedY && onGround && speed.y > 0)
                    speed.y = 0;

                speed.y = Mathf.MoveTowards(speed.y, -desiredState.speed.y, acceleration.acceleration * Time.deltaTime);
            }
            else
                speed.y = Mathf.MoveTowards(speed.y, 0, acceleration.deAcceleration * Time.deltaTime);
        }

        void AccelerateHorizonatal()
        {
            if (fpi.move.x > 0)
            {
                if (snapSpeedX && onGround && speed.x < 0)
                    speed.x = 0;

                speed.x = Mathf.MoveTowards(speed.x, desiredState.speed.x, acceleration.acceleration * Time.deltaTime);
            }
            else if (fpi.move.x < 0)
            {
                if (snapSpeedX && onGround && speed.x > 0)
                    speed.x = 0;

                speed.x = Mathf.MoveTowards(speed.x, -desiredState.speed.x, acceleration.acceleration * Time.deltaTime);
            }
            else
                speed.x = Mathf.MoveTowards(speed.x, 0, acceleration.deAcceleration * Time.deltaTime);
        }

        void PlayWalkSound()
        {
            aud.PlayOneShot(desiredState.MovementSounds[Random.Range(0, desiredState.MovementSounds.Length)]);
        }

        void Look()
        {
            if (!canLook)
            {
                return;
            }

            camRotation = cam.localEulerAngles;

            if (!sliding)
            {
                charRotation = transform.eulerAngles;
                charRotation.y += (fpi.look.x + fpi.altLook.x) * lookSens;

                transform.eulerAngles = charRotation;

                if (aligningView && cameraResetMethod == SlideCameraResetMethod.SmoothToCharacter)
                    AlignCamToCharacter();
            }

            else
            {
                camRotation.y -= -(fpi.look.x + fpi.altLook.x) * lookSens;

                if (camRotation.y > slideLookLimmit.x && camRotation.y < 180)
                {
                    camRotation.y = slideLookLimmit.x;
                }

                else if (camRotation.y < slideLookLimmit.y && camRotation.y > 180)
                {
                    camRotation.y = slideLookLimmit.y;
                }
            }

            camRotation.x += -(fpi.look.y + fpi.altLook.y) * lookSens;

            if (camRotation.x > cameraLimmit.x && camRotation.x < 180)
            {
                camRotation.x = cameraLimmit.x;
            }

            else if (camRotation.x < cameraLimmit.y && camRotation.x > 180)
            {
                camRotation.x = cameraLimmit.y;
            }

            cam.localEulerAngles = camRotation;
        }

        void AlignCamToCharacter()
        {
            camRotation.y = Mathf.MoveTowardsAngle(camRotation.y, 0, slideLookResetDelta * Time.deltaTime);

            if (camRotation.y < 1 && camRotation.y < 180)
                camRotation.y = 0;
            else if (camRotation.y > 359 && camRotation.y > 180)
                camRotation.y = 0;



            cam.localEulerAngles = camRotation;
        }

        void Jump()
        {
            jumpPower = Mathf.MoveTowards(jumpPower, 0, jumpPowerDelta * Time.deltaTime);

            if (HitHead(HeadHitDetect.Part, desiredState))
                jumpPower = 0;

            if (jumpPower == 0)
            {
                jumping = false;
            }
        }

        void Fall()
        {
            gravity = Mathf.MoveTowards(gravity, maxGravity, gravityDelta * Time.deltaTime);
        }

        void CheckState()
        {
            //jumping
            if (fpi.jump && onGround && !sliding)
            {
                //if we are crouching or proning then go to normal or crouch state or dont do crap
                if (crouching || proning)
                {
                    if (HitHead(headHitDetect, crouch))
                        return;

                    proning = false;

                    if(!HitHead(HeadHitDetect.Full, normal))
                    {
                        GoToState(normal, State.standing);
                        crouching = false;
                    }
                    else
                    {
                        GoToState(crouch, State.crouching);
                        crouching = true;
                    }
                }

                //or just simply jump
                else if (canJump && onGround && !jumping)
                {
                    OnJumped();
                }
            }

            //sprinting
            if (canMove && !sliding && (onGround || (!crouching && !proning)))
            {
                //forward and sides restriction
                if (sprintRestriction == SprintRestriction.forwardAndSides)
                {
                    if (speed.y < 0 && sprinting)
                    {
                        sprinting = false;
                        standing = true;

                        GoToState(normal, State.standing);
                    }

                    if (speed.y > 0 && !sprinting)
                    {
                        if (fpi.sprint)
                        {
                            sprinting = true;
                            crouching = false;
                            proning = false;
                            standing = false;

                            GoToState(sprint, State.sprinting);
                        }
                    }
                }

                //forward restriction
                else if (sprintRestriction == SprintRestriction.forward)
                {
                    if (sprinting)
                    {
                        if (speed.y < 0 || speed.x != 0)
                        {
                            sprinting = false;
                            standing = true;

                            GoToState(normal, State.standing);
                        }
                    }

                    if (speed.y > 0 && speed.x == 0 && !sprinting)
                    {
                        if (fpi.sprint)
                        {
                            sprinting = true;
                            crouching = false;
                            proning = false;
                            standing = false;

                            GoToState(sprint, State.sprinting);
                        }
                    }
                }
            }

            if (!fpi.sprint && !sliding && sprinting && onGround)
            {
                if (!sprinting)
                    return;

                sprinting = false;
                crouching = false;
                proning = false;
                standing = true;

                if (!changingState)
                    changingState = true;

                GoToState(normal, State.standing);
            }

            //crouching
            if (fpi.crouchToggle && canCrouch && !sliding)
            {
                if (sprinting && canSlide && onGround && !changingState && fpi.move.y > 0)
                {
                    OnSlideBegin();
                }

                else
                {
                    if (crouching && HitHead(headHitDetect, normal))
                        return;
                    else if (proning && HitHead(headHitDetect, crouch))
                        return;

                    crouching = !crouching;
                    standing = !crouching;

                    sprinting = false;

                    if (crouching)
                    {
                        GoToState(crouch, State.crouching);
                    }

                    else
                    {
                        GoToState(normal, State.standing);
                    }

                    proning = false;
                }
            }

            //proning
            if (fpi.ProneToggle && canProne && !sliding)
            {
                if (!changingState)
                    changingState = true;

                if (proning)
                    if (HitHead(headHitDetect, normal))
                        return;

                proning = !proning;
                standing = !proning;
                crouching = false;
                sprinting = false;

                if (proning)
                {
                    GoToState(prone, State.proning);
                }

                else
                {
                    GoToState(normal, State.standing);
                }
            }

            if (changingState)
                ChangeState();
            if (sliding)
                Slide();
        }

        void Slide()
        {
            slidePower = Mathf.MoveTowards(slidePower, 0, slideDelta * Time.deltaTime);

            lean = Mathf.MoveTowards(lean, 0.5f, leanDelta * Time.deltaTime);

            if (HitFront())
            {
                slidePower = 0;
            }

            if (slidePower == 0)
            {
                OnSlideEnd();
            }
        }

        void GoToState(CharacterState newState, State state)
        {
            if (newState == desiredState)
                return;

            changingState = true;
            desiredState = newState;

            OnStateChanged(state);
        }

        void ChangeState()
        {
            if (ch.height < desiredState.height && HitHead(headHitDetect, desiredState)) //if current state overlaps geometry
            {
                standing = false;
                sprinting = false;

                //if current height is lower than crouch go prone
                //else go crouch

                if (ch.height < crouch.height)
                {
                    desiredState = prone;

                    proning = true;
                    crouching = false;
                    OnStateChanged(State.proning);
                }

                else
                {
                    desiredState = crouch;

                    crouching = true;
                    proning = false;
                    OnStateChanged(State.crouching);
                }
            }

            ch.height = Mathf.MoveTowards(ch.height, desiredState.height, desiredState.changeDelta * Time.deltaTime);

            ch.radius = Mathf.MoveTowards(ch.radius, desiredState.radius, desiredState.changeDelta * Time.deltaTime / 5);

            ch.center = new Vector3(0, ch.height / 2);

            camPosition = cam.localPosition;
            camPosition.y = ch.center.y - 0.05f;

            cam.localPosition = camPosition;
            camPivot.localPosition = camPosition;

            if ((ch.height == desiredState.height && ch.radius == desiredState.radius))
            {
                changingState = false;
            }
        }

        void Lean()
        {
            if (fpi.leanRight && canLean && !sliding)
            {
                if (lean > 0.5f)
                {
                    leanDirection = cam.right;

                    if (!CamHit(leanDirection, 0.2f))
                        lean = Mathf.MoveTowards(lean, 1, leanDelta * Time.deltaTime);
                }
                else
                {
                    leanDirection = -cam.right;

                    lean = Mathf.MoveTowards(lean, 1, leanDelta * Time.deltaTime);
                }

                leaning = true;
            }

            else if (fpi.leanLeft && canLean && !sliding)
            {
                if (lean > 0.5f)
                {
                    leanDirection = cam.right;

                    lean = Mathf.MoveTowards(lean, 0, leanDelta * Time.deltaTime);
                }
                else
                {
                    leanDirection = -cam.right;

                    if (!CamHit(leanDirection, 0.2f))
                        lean = Mathf.MoveTowards(lean, 0, leanDelta * Time.deltaTime);
                }

                leaning = true;
            }

            else if(leaning)
            {
                lean = Mathf.MoveTowards(lean, 0.5f, leanDelta * Time.deltaTime);

                if (lean == 0.5f)
                    leaning = false;
            }

            if (CamHit(leanDirection, 0.1f))
            {
                lean = Mathf.MoveTowards(lean, 0.5f, leanDelta * Time.deltaTime);
            }

            camPivot.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(maxLean, -maxLean, lean));

            if (AlignCamRotation)
            {
                camRotation.z = -Mathf.Lerp(maxLean, -maxLean, lean);
                cam.localEulerAngles = camRotation;
            }
        }

        bool CheckGrounded()
        {
            Vector3 dwn = transform.TransformDirection(Vector3.down);
            Vector3 startPos = transform.position;
            if (Physics.Raycast(startPos, dwn, CheckDistance))
            {
                return true;
            }

            return false;
        }

        bool HitHead(HeadHitDetect headDets, CharacterState hitState)
        {
            Vector3 up = transform.TransformDirection(Vector3.up);
            Vector3 startPos = transform.position;
            startPos.y += ch.height;

            float rayDistance;

            if (headDets == HeadHitDetect.Full)
                rayDistance = hitState.height - ch.height;
            else
                rayDistance = headHitCheckDistance;

            if (Physics.Raycast(startPos, up, rayDistance))
            {
                return true;
            }

            return false;
        }

        bool HitFront()
        {
            if (ch.collisionFlags == (CollisionFlags)5 || ch.collisionFlags == CollisionFlags.CollidedSides)
                return true;

            return false;
        }

        bool CamHit(Vector3 direction, float distanceAdd)
        {
            Vector3 startPos = camPivot.position;
            startPos.y += camPivot.localPosition.y;

            Vector3 endPos = cam.position;
            endPos += direction * distanceAdd;

            RaycastHit hit;

            if (Physics.Linecast(startPos, endPos, out hit))
            {
                Debug.DrawLine(startPos, hit.point, Color.red);
                return true;
            }

            else
            {
                Debug.DrawRay(startPos, direction * 40, Color.red);
            }

            return false;
        }

        //state functions
        public void OnJumped()
        {
            falling = false;
            gravity = 0;

            jumpPower = maxJumpPower;
            jumping = true;

            if (desiredState.JumpingSounds.Length > 0)
                aud.PlayOneShot(desiredState.JumpingSounds[Random.Range(0, desiredState.JumpingSounds.Length)]);

            if (OnCharacterJumped != null)
                OnCharacterJumped();
        }

        public void OnFell()
        {
            gravity = 0;
            falling = true;

            fallBegin = transform.position;

            if (OnCharacterFell != null)
                OnCharacterFell();
        }

        public void OnLeftGround()
        {
            inAir = true;

            wInterval = 0;

            if (OnCharacterLeftGround != null)
                OnCharacterLeftGround();
        }

        public void OnLanded()
        {
            acceleration = desiredState.onGround;

            inAir = false;
            falling = false;
            gravity = maxGravity;

            if (desiredState.LandingSounds.Length > 0)
                aud.PlayOneShot(desiredState.LandingSounds[Random.Range(0, desiredState.LandingSounds.Length)]);

            if (OnCharacterLanded != null)
                OnCharacterLanded(Vector3.Distance(transform.position, fallBegin), fallBegin.y - transform.position.y);
        }

        public void OnSlideBegin()
        {
            sliding = true;
            sprinting = false;

            slidePower = maxSlidePower;

            GoToState(slide, State.sliding);

            if (OnCharacterSlideBegin != null)
                OnCharacterSlideBegin();
        }

        public void OnSlideEnd()
        {
            sliding = false;

            if(!fpi.sprint)
            {
                GoToState(crouch, State.crouching);
                crouching = true;
            }

            if (cameraResetMethod == SlideCameraResetMethod.InstantToCamera)
            {
                float currentCamRotY;

                currentCamRotY = camRotation.y;

                camRotation.y = 0;
                charRotation.y += currentCamRotY;

                cam.localEulerAngles = camRotation;
                transform.eulerAngles = charRotation;
            }

            else if (cameraResetMethod == SlideCameraResetMethod.SmoothToCharacter)
                aligningView = true;

            if (OnCharacterSlideEnd != null)
                OnCharacterSlideEnd();
        }

        public void OnStateChanged(State newState)
        {
            characterState = newState;

            if (OnCharacterStateChanged != null)
                OnCharacterStateChanged(characterState);
        }
    }

    [System.Serializable]
    public class CharacterState
    {
        [SerializeField]
        internal float height;
        [SerializeField]
        internal float radius;
        [SerializeField]
        internal Vector2 speed;

        [SerializeField]
        internal Acceleration onGround;
        [SerializeField]
        internal Acceleration inAir;

        [SerializeField]
        AudioClip[] movementSound;
        public AudioClip[] MovementSounds { get { return movementSound; } }

        [SerializeField]
        AudioClip[] jumpingSounds;
        public AudioClip[] JumpingSounds { get { return jumpingSounds; } }

        [SerializeField]
        AudioClip[] landingSounds;
        public AudioClip[] LandingSounds { get { return landingSounds; } }

        [SerializeField]
        internal float changeDelta;
        [SerializeField]
        internal float speedDelta;

        [SerializeField]
        internal float stepInterval;

        [SerializeField]
        internal float heightPerUnit;

        public CharacterState(float newHeight, float newRadius, Vector2 newSpeed, Acceleration newInGround, Acceleration newInAir, float newChangeDelta, float newSpeedDelta, float newStepInterval, float newHPU)
        {
            height = newHeight;
            radius = newRadius;
            speed = newSpeed;
            onGround = newInGround;
            inAir = newInAir;
            changeDelta = newChangeDelta;
            speedDelta = newSpeedDelta;
            stepInterval = newStepInterval;
            heightPerUnit = newHPU;
        }

        public static bool operator == (CharacterState state1, CharacterState state2)
        {
            return state1.radius == state2.radius &&
                state1.height == state2.height &&
                state1.speed == state2.speed &&
                state1.changeDelta == state2.changeDelta &&
                state1.speedDelta == state2.speedDelta &&
                state1.heightPerUnit == state2.heightPerUnit;
        }

        public static bool operator != (CharacterState state1, CharacterState state2)
        {
            return state1.radius != state2.radius &&
                state1.height != state2.height &&
                state1.speed != state2.speed &&
                state1.changeDelta != state2.changeDelta &&
                state1.speedDelta != state2.speedDelta &&
                state1.heightPerUnit != state2.heightPerUnit;
        }
    }

    [System.Serializable]
    public class Acceleration
    {
        [SerializeField]
        internal float acceleration;
        [SerializeField]
        internal float deAcceleration;

        public Acceleration(float newAcceleration, float newDeAcceleration)
        {
            acceleration = newAcceleration;
            deAcceleration = newDeAcceleration;
        }
    }

    public interface FPInput
    {
        void UpdateInput();

        Vector2 move { get; }
        Vector2 look { get; }
        Vector2 altLook { get; }

        bool crouchToggle { get; }
        bool ProneToggle { get; }
        bool jump { get; }
        bool sprint { get; }

        bool leanRight { get; }
        bool leanLeft { get; }
    }

    public enum State
    {
        standing,
        sprinting,
        crouching,
        proning,
        sliding
    }

    public enum SlideCameraResetMethod
    {
        SmoothToCharacter,
        InstantToCamera,
    }

    public enum InAirDirection
    {
        CharacterDirection,
        JumpDirecion,
    }

    public enum SprintRestriction
    {
        allDirections,
        forwardAndSides,
        forward,
    }

    public enum HeadHitDetect
    {
        Full,
        Part
    }
}