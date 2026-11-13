using UnityEngine;
namespace CityCourier
{
    public sealed class CourierPracticeLetter : MonoBehaviour
    {
        public bool Collected { get; private set; }
        void OnTriggerEnter(Collider other)
        {
            if (Collected || other.GetComponent<CourierMotor>() == null) return;
            Collected = true;
            Debug.Log("[맵 1-2] 노란 편지를 수집했습니다!");
            gameObject.SetActive(false);
        }
    }
}
