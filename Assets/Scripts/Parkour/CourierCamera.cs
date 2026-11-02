using UnityEngine;

namespace CityCourier
{
    public sealed class CourierCamera : MonoBehaviour
    {
        public Transform Target;
        public CourierMotor Motor;
        public LayerMask WorldMask = 1;
        [Tooltip("Approximate time used to smoothly align behind the player.")]
        public float RecenterTime = 0.3f;

        [Header("Dynamic FOV")]
        public float BaseFov = 72f;
        public float MaxFov = 81f;
        public float MinSpeedForFov = 3f;
        public float MaxSpeedForFov = 17f;
        public float FovSmoothTime = 0.18f;

        [Header("Action Tilt & Pivot")]
        [Tooltip("Subtle camera roll angle (degrees) only while wall-running.")]
        public float WallRunTiltAngle = 6f;
        [Tooltip("Keep 0 to prevent motion-sickness rocking during normal ground running.")]
        public float TurnTiltMaxAngle = 0f;
        public float TiltSmoothTime = 0.18f;
        public float StandingPivotHeight = 1.3f;
        public float SlidingPivotHeight = 1.02f;

        [Header("Smooth Landing & Subtle Shake")]
        public float LandingPitchKickScale = 0.14f;
        public float LandingVerticalDipScale = 0.006f;
        public float ShakeFrequency = 20f;
        public float ShakeDecay = 4.5f;

        public float Yaw { get; private set; }
        public float CurrentFov { get; private set; } = 72f;
        public float CurrentRoll { get; private set; }
        public float CurrentShake { get; private set; }
        public float LandingPitchOffset { get; private set; }
        public float LandingVerticalOffset { get; private set; }

        Camera cam;
        CourierMotor subscribedMotor;
        CourierStage subscribedStage;
        float pitch = 18f;
        float targetYaw, targetPitch = 18f;
        float yawVelocity, pitchVelocity;
        float fovVelocity, rollVelocity, pivotHeightVelocity;
        float targetLandingPitch, targetLandingDip;
        float targetLandingPitchVel, targetLandingDipVel;
        float landingPitchVel, landingVertVel;
        float currentPivotHeight = 1.3f;
        float currentDistance = 5.5f;
        float shakeTime;
        int lastEffectFrame = -1;
        bool recentering;
        bool snap = true;

        void Awake()
        {
            cam = GetComponent<Camera>();
            CurrentFov = BaseFov;
            currentPivotHeight = StandingPivotHeight;
            BindMotorEvents();
        }

        void OnEnable()
        {
            if (cam == null) cam = GetComponent<Camera>();
            BindMotorEvents();
        }

        void OnDisable()
        {
            UnbindMotorEvents();
        }

        void BindMotorEvents()
        {
            if (Motor == null && Target != null)
                Motor = Target.GetComponent<CourierMotor>();

            if (Motor != subscribedMotor)
            {
                UnbindMotorEvents();
                subscribedMotor = Motor;
                if (subscribedMotor != null)
                {
                    subscribedMotor.Landed += OnMotorLanded;
                    subscribedMotor.Jumped += OnMotorJumped;
                    subscribedMotor.SlideTriggered += OnMotorSlideTriggered;
                    subscribedStage = subscribedMotor.Stage;
                    if (subscribedStage != null)
                        subscribedStage.Damaged += OnStageDamaged;
                }
            }
            else if (subscribedMotor != null && subscribedStage != subscribedMotor.Stage)
            {
                if (subscribedStage != null) subscribedStage.Damaged -= OnStageDamaged;
                subscribedStage = subscribedMotor.Stage;
                if (subscribedStage != null) subscribedStage.Damaged += OnStageDamaged;
            }
        }

        void UnbindMotorEvents()
        {
            if (subscribedMotor != null)
            {
                subscribedMotor.Landed -= OnMotorLanded;
                subscribedMotor.Jumped -= OnMotorJumped;
                subscribedMotor.SlideTriggered -= OnMotorSlideTriggered;
                subscribedMotor = null;
            }
            if (subscribedStage != null)
            {
                subscribedStage.Damaged -= OnStageDamaged;
                subscribedStage = null;
            }
        }

        void OnMotorLanded(float impactSpeed, bool wasDiving)
        {
            // Never snap on frame 0: set a gentle target that Current Offset smoothly eases into and out of.
            float clampedImpact = Mathf.Clamp(impactSpeed, 0f, 24f);
            float diveMultiplier = wasDiving ? 1.25f : 1f;
            targetLandingPitch = Mathf.Clamp(clampedImpact * LandingPitchKickScale * diveMultiplier, 0f, 2.4f);
            targetLandingDip = -Mathf.Clamp(clampedImpact * LandingVerticalDipScale * diveMultiplier, 0f, 0.11f);
            float shakeImpulse = Mathf.Clamp01((clampedImpact - 4f) / 20f) * (wasDiving ? 0.35f : 0.18f);
            TriggerShake(shakeImpulse);
        }

        void OnMotorJumped(bool isWallJump)
        {
            if (isWallJump)
            {
                targetLandingPitch = Mathf.Max(targetLandingPitch - 0.8f, -1.2f);
                TriggerShake(0.14f);
            }
        }

        void OnMotorSlideTriggered(bool isAirDive)
        {
            TriggerShake(isAirDive ? 0.18f : 0.10f);
        }

        void OnStageDamaged(string reason)
        {
            TriggerShake(0.45f);
        }

        public void TriggerShake(float amount)
        {
            CurrentShake = Mathf.Clamp01(Mathf.Max(CurrentShake, amount));
        }

        public void ApplyLook(Vector2 look, bool recenter, Vector3 heading)
        {
            if (look.sqrMagnitude > 0.0001f)
            {
                recentering = false;
                Yaw += look.x;
                pitch = Mathf.Clamp(pitch - look.y, -25, 65);
                targetYaw = Yaw;
                targetPitch = pitch;
                yawVelocity = pitchVelocity = 0f;
            }
            if (recenter)
            {
                targetYaw = Mathf.Atan2(heading.x, heading.z) * Mathf.Rad2Deg;
                targetPitch = 18f;
                recentering = true;
            }
        }

        public void ResetView()
        {
            Yaw = targetYaw = 0f;
            pitch = targetPitch = 18f;
            yawVelocity = pitchVelocity = 0f;
            fovVelocity = rollVelocity = pivotHeightVelocity = 0f;
            targetLandingPitch = targetLandingDip = 0f;
            targetLandingPitchVel = targetLandingDipVel = 0f;
            landingPitchVel = landingVertVel = 0f;
            CurrentFov = BaseFov;
            CurrentRoll = 0f;
            CurrentShake = 0f;
            LandingPitchOffset = 0f;
            LandingVerticalOffset = 0f;
            currentPivotHeight = StandingPivotHeight;
            currentDistance = 5.5f;
            if (cam == null) cam = GetComponent<Camera>();
            if (cam != null) cam.fieldOfView = CurrentFov;
            recentering = false;
            snap = true;
        }

        public void StepEffects(float dt)
        {
            if (dt <= 0f) return;
            BindMotorEvents();
            if (cam == null) cam = GetComponent<Camera>();

            // 1. Dynamic FOV linked to horizontal speed and parkour state
            float speed = Motor != null ? Motor.HorizontalSpeed : 0f;
            float speedRatio = Mathf.InverseLerp(MinSpeedForFov, MaxSpeedForFov, speed);
            float targetFov = Mathf.Lerp(BaseFov, MaxFov, speedRatio);
            if (Motor != null)
            {
                if (Motor.IsSliding) targetFov = Mathf.Max(targetFov, BaseFov + (MaxFov - BaseFov) * 0.7f);
                else if (Motor.IsWallRunning) targetFov = Mathf.Max(targetFov, BaseFov + (MaxFov - BaseFov) * 0.45f);
            }
            CurrentFov = Mathf.SmoothDamp(CurrentFov, targetFov, ref fovVelocity, FovSmoothTime, Mathf.Infinity, dt);
            if (cam != null) cam.fieldOfView = CurrentFov;

            // 2. Action Tilt: ONLY tilt on wall-run (0 roll on ground running to avoid motion sickness)
            float targetRoll = 0f;
            if (Motor != null)
            {
                if (Motor.IsWallRunning && Motor.WallNormal.sqrMagnitude > 0.01f)
                {
                    Vector3 right = Vector3.Cross(Vector3.up, Motor.Heading);
                    float wallSide = Vector3.Dot(right, Motor.WallNormal);
                    targetRoll = Mathf.Sign(wallSide) * WallRunTiltAngle;
                }
                else if (TurnTiltMaxAngle > 0.001f)
                {
                    targetRoll = Mathf.Clamp(-Motor.TurnRate * 0.01f, -TurnTiltMaxAngle, TurnTiltMaxAngle);
                }
            }
            CurrentRoll = Mathf.SmoothDamp(CurrentRoll, targetRoll, ref rollVelocity, TiltSmoothTime, Mathf.Infinity, dt);

            // 3. Gentle pivot height adjustment for slide / dive
            float desiredPivotH = (Motor != null && (Motor.IsSliding || Motor.IsDiving))
                ? SlidingPivotHeight : StandingPivotHeight;
            currentPivotHeight = Mathf.SmoothDamp(currentPivotHeight, desiredPivotH, ref pivotHeightVelocity, 0.16f, Mathf.Infinity, dt);

            // 4. Two-stage smooth landing spring (eases in smoothly from 0, then eases back to 0)
            LandingPitchOffset = Mathf.SmoothDamp(LandingPitchOffset, targetLandingPitch, ref landingPitchVel, 0.055f, Mathf.Infinity, dt);
            LandingVerticalOffset = Mathf.SmoothDamp(LandingVerticalOffset, targetLandingDip, ref landingVertVel, 0.055f, Mathf.Infinity, dt);
            targetLandingPitch = Mathf.SmoothDamp(targetLandingPitch, 0f, ref targetLandingPitchVel, 0.14f, Mathf.Infinity, dt);
            targetLandingDip = Mathf.SmoothDamp(targetLandingDip, 0f, ref targetLandingDipVel, 0.14f, Mathf.Infinity, dt);

            if (CurrentShake > 0f)
            {
                shakeTime += dt * ShakeFrequency;
                CurrentShake = Mathf.Max(0f, CurrentShake - ShakeDecay * dt);
            }
            lastEffectFrame = Time.frameCount;
        }

        void LateUpdate()
        {
            if (Target == null) return;
            float dt = Time.deltaTime;
            if (lastEffectFrame != Time.frameCount)
                StepEffects(dt);

            if (recentering)
            {
                Yaw = Mathf.SmoothDampAngle(Yaw, targetYaw, ref yawVelocity, RecenterTime);
                pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, RecenterTime);
                if (Mathf.Abs(Mathf.DeltaAngle(Yaw, targetYaw)) < 0.1f && Mathf.Abs(pitch - targetPitch) < 0.1f)
                {
                    Yaw = targetYaw;
                    pitch = targetPitch;
                    yawVelocity = pitchVelocity = 0f;
                    recentering = false;
                }
            }

            float shakePitch = 0f, shakeYaw = 0f, shakeRoll = 0f;
            if (CurrentShake > 0.001f)
            {
                float s = CurrentShake * CurrentShake;
                shakePitch = (Mathf.PerlinNoise(shakeTime, 1.3f) - 0.5f) * 2f * (1.2f * s);
                shakeYaw = (Mathf.PerlinNoise(2.7f, shakeTime) - 0.5f) * 2f * (0.9f * s);
                shakeRoll = (Mathf.PerlinNoise(shakeTime, 5.1f) - 0.5f) * 2f * (1.4f * s);
            }

            // Use stable pivot for SphereCast so landing dips never cause raycast distance jumps
            Vector3 rayOrigin = Target.position + Vector3.up * currentPivotHeight;
            Quaternion lookRotation = Quaternion.Euler(pitch, Yaw, 0);
            Vector3 backward = lookRotation * Vector3.back;
            float hitDistance = 5.5f;
            if (Physics.SphereCast(rayOrigin, 0.2f, backward, out RaycastHit hit, hitDistance,
                WorldMask, QueryTriggerInteraction.Ignore))
            {
                hitDistance = Mathf.Max(0.25f, hit.distance - 0.15f);
            }
            currentDistance = snap ? hitDistance : Mathf.Lerp(currentDistance, hitDistance, 1f - Mathf.Exp(-20f * dt));

            Vector3 pivot = rayOrigin + Vector3.up * LandingVerticalOffset;
            Quaternion finalRotation = Quaternion.Euler(pitch + LandingPitchOffset + shakePitch, Yaw + shakeYaw, CurrentRoll + shakeRoll);
            Vector3 desired = pivot + backward * currentDistance;
            transform.position = snap ? desired : Vector3.Lerp(transform.position, desired, 1 - Mathf.Exp(-18 * dt));
            transform.rotation = finalRotation;
            snap = false;
        }
    }
}
