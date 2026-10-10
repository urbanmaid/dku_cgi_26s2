using UnityEngine;
namespace CityCourier
{
    public sealed class CourierLetter : MonoBehaviour
    {
        Vector3 origin;
        void Awake() { origin = transform.localPosition; }
        void Update()
        {
            transform.Rotate(0, 65 * Time.deltaTime, 0, Space.Self);
            transform.localPosition = origin + Vector3.up * (Mathf.Sin(Time.time * 3) * 0.12f);
        }
    }
}
