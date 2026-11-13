using UnityEngine;

namespace CityCourier
{
    // Practice maps do not use the delivery stage's objective state.
    public sealed class CourierPracticeHazard : MonoBehaviour
    {
        public Vector3 RespawnPoint = new Vector3(0, 0.1f, -8);
        void OnTriggerEnter(Collider other)
        {
            var motor = other.GetComponent<CourierMotor>();
            if (motor != null)
            {
                var menu = Object.FindFirstObjectByType<CourierMapMenu>();
                motor.Teleport(menu != null ? menu.PracticeCheckpoint : RespawnPoint);
            }
        }
    }
}
