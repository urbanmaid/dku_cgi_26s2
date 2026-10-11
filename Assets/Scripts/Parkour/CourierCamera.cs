using UnityEngine;

namespace CityCourier
{
    public sealed class CourierCamera : MonoBehaviour
    {
        public Transform Target;
        public LayerMask WorldMask = 1;
        [Tooltip("Approximate time used to smoothly align behind the player.")]
        public float RecenterTime = 0.3f;
        public float Yaw { get; private set; }
        float pitch = 18;
        float targetYaw, targetPitch = 18;
        float yawVelocity, pitchVelocity;
        bool recentering;
        bool snap = true;

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
            recentering = false;
            snap = true;
        }

        void LateUpdate()
        {
            if (Target == null) return;
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
            Vector3 pivot = Target.position + Vector3.up * 1.3f;
            Quaternion rotation = Quaternion.Euler(pitch, Yaw, 0);
            Vector3 backward = rotation * Vector3.back;
            float distance = 5.5f;
            if (Physics.SphereCast(pivot, 0.2f, backward, out RaycastHit hit, distance,
                WorldMask, QueryTriggerInteraction.Ignore)) distance = Mathf.Max(0.25f, hit.distance - 0.15f);
            Vector3 desired = pivot + backward * distance;
            transform.position = snap ? desired : Vector3.Lerp(transform.position, desired, 1 - Mathf.Exp(-18 * Time.deltaTime));
            transform.rotation = rotation;
            snap = false;
        }
    }
}
