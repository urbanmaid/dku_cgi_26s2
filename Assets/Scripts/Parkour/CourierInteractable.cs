using UnityEngine;

namespace CityCourier
{
    public sealed class CourierInteractable : MonoBehaviour
    {
        public enum InteractionKind { Guide, Portal, RoadSwitch }
        public InteractionKind Kind;
        [TextArea] public string[] Dialogue;
        public Renderer Indicator;
        public Material ActivatedMaterial;
        int line;

        public void Activate(CourierStage stage)
        {
            switch (Kind)
            {
                case InteractionKind.Guide:
                    if (Dialogue != null && Dialogue.Length > 0)
                    { Debug.Log("[안내원] " + Dialogue[line]); line = (line + 1) % Dialogue.Length; }
                    break;
                case InteractionKind.Portal: stage.EnterStage(); break;
                case InteractionKind.RoadSwitch:
                    stage.OpenRoad();
                    if (stage.RoadOpen && Indicator != null) Indicator.sharedMaterial = ActivatedMaterial;
                    break;
            }
        }
    }
}
