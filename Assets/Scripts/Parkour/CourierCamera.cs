using UnityEngine;

namespace CityCourier
{
    public sealed class CourierCamera : MonoBehaviour
    {
        public Transform Target;
        public LayerMask WorldMask = 1;
        public float Yaw { get; private set; }
        float pitch = 18;
        bool snap = true;

        public void ApplyLook(Vector2 look, bool recenter, Vector3 heading)
        {
            Yaw += look.x;
            pitch = Mathf.Clamp(pitch - look.y, -25, 65);
            if (recenter) { Yaw = Mathf.Atan2(heading.x, heading.z) * Mathf.Rad2Deg; pitch = 18; }
        }

        public void ResetView() { Yaw = 0; pitch = 18; snap = true; }

        void LateUpdate()
        {
            if (Target == null) return;
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
