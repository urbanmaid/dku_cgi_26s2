using UnityEngine;
namespace CityCourier
{
    public sealed class CourierPracticeCheckpoint : MonoBehaviour
    {
        public Vector3 SpawnPoint;
        void OnTriggerEnter(Collider other)
        {
            var motor = other.GetComponent<CourierMotor>();
            var menu = Object.FindFirstObjectByType<CourierMapMenu>();
            if (motor != null && menu != null && menu.Player == motor)
                menu.SetPracticeCheckpoint(SpawnPoint);
        }
    }
}
