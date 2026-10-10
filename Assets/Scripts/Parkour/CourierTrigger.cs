using UnityEngine;

namespace CityCourier
{
    public sealed class CourierTrigger : MonoBehaviour
    {
        public enum TriggerKind { Letter, Checkpoint, Hazard, Hint, Goal }
        public TriggerKind Kind;
        public CourierStage Stage;
        public int LetterIndex;
        public Vector3 SpawnPoint;
        [TextArea] public string Message;
        bool announced;

        void OnTriggerEnter(Collider other) { Visit(other); }
        void OnTriggerStay(Collider other)
        {
            if (Kind == TriggerKind.Hazard) Visit(other);
        }
        void Visit(Collider other)
        {
            var motor = other.GetComponent<CourierMotor>();
            if (motor == null || motor != Stage.Player) return;
            switch (Kind)
            {
                case TriggerKind.Letter:
                    if (Stage.Collect(LetterIndex)) gameObject.SetActive(false);
                    break;
                case TriggerKind.Checkpoint: Stage.SetCheckpoint(SpawnPoint); break;
                case TriggerKind.Hazard: Stage.Damage(Message); break;
                case TriggerKind.Hint:
                    if (!announced) { Debug.Log("[조작 팁] " + Message); announced = true; }
                    break;
                case TriggerKind.Goal: Stage.TryComplete(); break;
            }
        }
    }
}
