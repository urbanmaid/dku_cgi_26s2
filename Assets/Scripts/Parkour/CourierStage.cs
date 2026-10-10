using System;
using UnityEngine;

namespace CityCourier
{
    public sealed class CourierStage : MonoBehaviour
    {
        public CourierMotor Player;
        public CourierInteractable[] Interactables;
        public GameObject Road;
        public GameObject RoadBarrier;
        public GameObject EntryBarrier;
        public Renderer GoalBeacon;
        public Material CompletedMaterial;
        public int RequiredLetters = 5;
        public Vector3 StartPosition = new Vector3(0, 0.1f, 2);
        public bool Started { get; private set; }
        public bool RoadOpen { get; private set; }
        public bool Completed { get; private set; }
        public int LetterCount { get; private set; }
        public int DamageCount { get; private set; }
        public Vector3 Checkpoint { get; private set; } = new Vector3(0, 0.1f, -8);
        public event Action<int, int> LettersChanged;
        public event Action StageCompleted;
        bool[] letters;
        float damageCooldown;

        void Awake() { letters = new bool[RequiredLetters]; }
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Debug.Log("[택배 러너] 청록색 안내원에게 E / X로 말을 걸고, 파란 포탈에서 E / X로 출발하세요. 편지 5개를 모아 초록색 목적지에 도착하세요. Esc: 마우스 해제, 클릭: 복귀.");
        }

        public void Interact(CourierMotor player)
        {
            if (Completed) return;
            CourierInteractable nearest = null;
            float distance = 3f;
            foreach (var item in Interactables)
            {
                if (item == null || !item.isActiveAndEnabled) continue;
                float candidate = Vector3.Distance(player.transform.position, item.transform.position);
                if (candidate < distance) { nearest = item; distance = candidate; }
            }
            if (nearest != null) nearest.Activate(this);
            else Debug.Log("[상호작용] 안내원, 출발 포탈 또는 도로 제어기 가까이에서 E / X를 누르세요.");
        }

        public void EnterStage()
        {
            if (Started) return;
            Started = true;
            EntryBarrier.SetActive(false);
            Checkpoint = StartPosition;
            Player.Teleport(Checkpoint);
            Debug.Log("[출발] WASD / LS 이동, Ctrl 천천히 걷기. 노란 편지를 모두 수집하세요.");
        }

        public bool Collect(int index)
        {
            if (!Started || Completed || index < 0 || index >= letters.Length || letters[index]) return false;
            letters[index] = true;
            LetterCount++;
            LettersChanged?.Invoke(LetterCount, RequiredLetters);
            Debug.Log($"[편지] {LetterCount}/{RequiredLetters} 수집 완료. 수집 상태는 복귀 후에도 유지됩니다.");
            return true;
        }

        public void SetCheckpoint(Vector3 point)
        {
            if (!Started || Completed || point.z <= Checkpoint.z) return;
            Checkpoint = point;
            Debug.Log("[체크포인트] " + point.z.ToString("F0") + "m 구간 저장");
        }

        public void Damage(string reason)
        {
            if (Completed || Time.time < damageCooldown) return;
            damageCooldown = Time.time + 0.5f;
            DamageCount++;
            Debug.Log($"[피격] {reason}. 체크포인트로 복귀합니다. 누적 피격: {DamageCount}");
            Player.Teleport(Checkpoint);
        }

        public void OpenRoad()
        {
            if (!Started || RoadOpen) return;
            RoadOpen = true;
            Road.SetActive(true);
            RoadBarrier.SetActive(false);
            Debug.Log("[도로 개방] 다리가 연결되었습니다. 편지를 모두 모은 뒤 초록색 목적지로 이동하세요.");
        }

        public bool TryComplete()
        {
            if (Completed) return true;
            if (!Started || !RoadOpen || LetterCount != RequiredLetters)
            {
                Debug.Log($"[도착 조건] 편지 {LetterCount}/{RequiredLetters}, 도로 개방: {RoadOpen}. 조건을 모두 충족해야 합니다.");
                return false;
            }
            Completed = true;
            GoalBeacon.sharedMaterial = CompletedMaterial;
            StageCompleted?.Invoke();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log($"[배달 완료] 편지 {RequiredLetters}개 전달 성공! 누적 피격: {DamageCount}. 다시 하려면 Play를 재시작하세요.");
            return true;
        }
    }
}
