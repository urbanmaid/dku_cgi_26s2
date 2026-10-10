using UnityEngine;
using UnityEngine.InputSystem;

namespace CityCourier
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CourierMotor : MonoBehaviour
    {
        public CourierCamera View;
        public CourierStage Stage;
        public Transform Body;
        [Header("Movement")]
        public float RunSpeed = 10f;
        public float WalkSpeed = 3f;
        public float Acceleration = 36f;
        public float JumpSpeed = 10f;
        public float Gravity = 27f;
        public float SlideSpeed = 14f;
        public float WallRunSpeed = 12f;
        public float WallRunDuration = 2.5f;
        public LayerMask WorldMask = 1;
        public bool AutomaticInput = true;
        public bool IsSliding { get; private set; }
        public bool IsWallRunning { get; private set; }
        public bool IsDiving { get; private set; }
        public Vector3 Velocity => horizontal + Vector3.up * vertical;
        public Vector3 Heading { get; private set; } = Vector3.forward;
        public bool Grounded => controller != null && controller.isGrounded;

        CharacterController controller;
        Vector3 horizontal, wallNormal;
        float vertical, slideTime, wallTime, wallLock, coyote, jumpBuffer;
        const float StandingHeight = 1.8f;
        const float SlidingHeight = 0.8f;

        void Awake() { controller = GetComponent<CharacterController>(); }

        void Update()
        {
            if (!AutomaticInput) return;
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                Cursor.lockState = CursorLockMode.None;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
            CourierInput input = CourierInput.Read();
            if (View != null) View.ApplyLook(input.Look, input.Recenter, Heading);
            if (Stage == null || !Stage.Completed) Simulate(input, Mathf.Min(Time.deltaTime, 0.05f));
            if (input.Interact && Stage != null) Stage.Interact(this);
        }

        public void Simulate(CourierInput input, float dt)
        {
            if (controller == null) controller = GetComponent<CharacterController>();
            bool grounded = controller.isGrounded;
            wallLock = Mathf.Max(0, wallLock - dt);
            jumpBuffer = input.Jump ? 0.14f : Mathf.Max(0, jumpBuffer - dt);
            coyote = grounded ? 0.12f : Mathf.Max(0, coyote - dt);
            if (grounded)
            {
                if (vertical < 0) vertical = -2f;
                wallTime = 0;
                if (IsDiving) { BeginSlide(); IsDiving = false; }
            }

            float yaw = View != null ? View.Yaw : 0;
            Vector3 wish = Quaternion.Euler(0, yaw, 0) * new Vector3(input.Move.x, 0, input.Move.y);
            if (wish.sqrMagnitude > 0.01f && !IsSliding) Heading = wish.normalized;
            if (input.Slide)
            {
                if (grounded) BeginSlide();
                else
                {
                    IsDiving = true;
                    IsSliding = true;
                    SetHeight(SlidingHeight);
                    horizontal = Heading * SlideSpeed;
                    vertical = -24;
                }
            }

            IsWallRunning = !grounded && !IsDiving && !IsSliding && wallLock <= 0 &&
                wallTime < WallRunDuration && input.Move.sqrMagnitude > 0.1f && FindWall(out wallNormal);
            if (IsWallRunning)
            {
                wallTime += dt;
                Vector3 along = Vector3.ProjectOnPlane(wish, wallNormal).normalized;
                if (along.sqrMagnitude < 0.1f) along = Vector3.ProjectOnPlane(Heading, wallNormal).normalized;
                horizontal = along * WallRunSpeed - wallNormal * 1.5f;
                vertical = Mathf.Clamp(vertical - 3f * dt, -0.7f, 2f);
            }
            else if (!IsSliding && wallLock <= 0)
                horizontal = Vector3.MoveTowards(horizontal, wish * (input.Walk ? WalkSpeed : RunSpeed),
                    Acceleration * (grounded ? 1 : 0.55f) * dt);

            if (IsSliding && !IsDiving)
            {
                slideTime -= dt;
                horizontal = Vector3.MoveTowards(horizontal, Heading * SlideSpeed, 12f * dt);
                if (slideTime <= 0 && !input.SlideHeld && CanStand())
                { IsSliding = false; SetHeight(StandingHeight); }
            }

            if (jumpBuffer > 0 && !IsDiving)
            {
                bool wallJump = !grounded && wallLock <= 0 && FindWall(out wallNormal);
                if ((coyote > 0 || wallJump) && CanStand())
                {
                    IsSliding = false;
                    SetHeight(StandingHeight);
                    vertical = JumpSpeed;
                    if (wallJump)
                    {
                        horizontal = Vector3.ProjectOnPlane(horizontal, wallNormal) * 0.8f + wallNormal * 7f;
                        wallLock = 0.25f;
                        IsWallRunning = false;
                    }
                    coyote = jumpBuffer = 0;
                }
            }
            if (!IsWallRunning) vertical -= Gravity * dt;
            CollisionFlags flags = controller.Move((horizontal + Vector3.up * vertical) * dt);
            if ((flags & CollisionFlags.Above) != 0 && vertical > 0) vertical = 0;
            if (Body != null)
            {
                Body.localPosition = new Vector3(0, controller.height / 2, 0);
                Body.localScale = new Vector3(0.65f, controller.height / 2, 0.65f);
                Body.rotation = Quaternion.LookRotation(Heading);
            }
            if (transform.position.y < -7 && Stage != null) Stage.Damage("추락");
        }

        void BeginSlide()
        {
            IsSliding = true;
            slideTime = 0.65f;
            SetHeight(SlidingHeight);
            horizontal = Heading * SlideSpeed;
        }

        void SetHeight(float height)
        {
            controller.height = height;
            controller.center = Vector3.up * (height / 2);
        }

        public bool CanStand()
        {
            return !Physics.CheckCapsule(transform.position + Vector3.up * 0.4f,
                transform.position + Vector3.up * (StandingHeight - 0.35f), 0.32f,
                WorldMask, QueryTriggerInteraction.Ignore);
        }

        bool FindWall(out Vector3 normal)
        {
            Vector3 origin = transform.position + Vector3.up * 1f;
            Vector3 right = Vector3.Cross(Vector3.up, Heading);
            Vector3[] directions = { right, -right, Heading };
            foreach (Vector3 direction in directions)
            {
                if (Physics.Raycast(origin, direction, out RaycastHit hit, 0.85f, WorldMask, QueryTriggerInteraction.Ignore)
                    && Mathf.Abs(hit.normal.y) < 0.15f && hit.collider.GetComponent<WallRunSurface>() != null)
                { normal = hit.normal; return true; }
            }
            normal = Vector3.zero;
            return false;
        }

        public void Teleport(Vector3 position)
        {
            if (controller == null) controller = GetComponent<CharacterController>();
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
            horizontal = Vector3.zero;
            vertical = wallTime = wallLock = slideTime = coyote = jumpBuffer = 0;
            IsSliding = IsDiving = IsWallRunning = false;
            Heading = Vector3.forward;
            SetHeight(StandingHeight);
            if (View != null) View.ResetView();
        }

        void OnDisable() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }
}
