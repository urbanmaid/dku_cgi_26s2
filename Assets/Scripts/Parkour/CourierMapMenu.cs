using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CityCourier
{
    [DefaultExecutionOrder(-100)]
    public sealed class CourierMapMenu : MonoBehaviour
    {
        public GameObject[] Maps;
        public CourierMotor Player;
        public CourierCamera FollowCamera;
        public bool IsOpen { get; private set; }
        public int CurrentMap { get; private set; } = -1;
        int selected;
        bool returningToMenu;
        float previousTimeScale = 1;
        readonly Vector3 spawn = new Vector3(0, 0.1f, -8);
        public Vector3 PracticeCheckpoint { get; private set; } = new Vector3(0, 0.1f, -8);
        public void SetPracticeCheckpoint(Vector3 point)
        {
            if (point.z > PracticeCheckpoint.z) PracticeCheckpoint = point;
        }

        void Start() { OpenMenu(); }

        public void OpenMenu()
        {
            if (IsOpen) return;
            previousTimeScale = Time.timeScale;
            IsOpen = true;
            Time.timeScale = 0;
            Player.enabled = false;
            FollowCamera.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void PlayMap(int index)
        {
            if (Maps == null || index < 0 || index >= Maps.Length || Maps[index] == null) return;
            bool changed = CurrentMap != index;
            for (int i = 0; i < Maps.Length; i++) Maps[i].SetActive(false);
            var stage = Maps[index].GetComponentInChildren<CourierStage>(true);
            Player.Stage = stage;
            if (stage != null) stage.Player = Player;
            Maps[index].SetActive(true);
            if (changed) Player.Teleport(stage != null ? (stage.Started ? stage.Checkpoint : spawn) : PracticeCheckpoint);
            CurrentMap = index;
            IsOpen = false;
            Time.timeScale = previousTimeScale;
            Player.enabled = true;
            FollowCamera.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) OpenMenu();
            if (!IsOpen && CurrentMap >= 0 && Player.Stage == null && Player.transform.position.y < -7)
                Player.Teleport(PracticeCheckpoint);
        }

        public void ReturnToMenu()
        {
            if (returningToMenu) return;
            returningToMenu = true;
            previousTimeScale = 1;
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadSceneAsync("MenuScene");
        }

        void LateUpdate()
        {
            if (!IsOpen) return;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void OnGUI()
        {
            if (!IsOpen) return;
            float scale = Mathf.Max(0.25f, Mathf.Min(Screen.width / 800f, Screen.height / 500f));
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(Screen.width / 2f, Screen.height / 2f, 0), Quaternion.identity, Vector3.one * scale);
            GUI.Box(new Rect(-310, -195, 620, 435), "");
            var title = new GUIStyle(GUI.skin.label) { fontSize = 28, alignment = TextAnchor.MiddleCenter };
            var body = new GUIStyle(GUI.skin.label) { fontSize = 17, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            var button = new GUIStyle(GUI.skin.button) { fontSize = 20 };
            GUI.Label(new Rect(-290, -175, 580, 50), "CITY COURIER · 맵 선택", title);
            selected = GUI.SelectionGrid(new Rect(-270, -105, 540, 65), selected, new[] { "01  파쿠르 코스", "02  자유 연습" }, 2, button);
            GUI.Label(new Rect(-260, -20, 520, 65), selected == 0
                ? "도시 파쿠르 코스에서 편지를 모아 배달하세요."
                : "도시 배경과 평평한 바닥으로 구성된 빈 맵입니다.", body);
            if (GUI.Button(new Rect(-180, 65, 360, 55), "선택한 맵 플레이", button)) PlayMap(selected);
            GUI.enabled = !returningToMenu;
            if (GUI.Button(new Rect(-180, 130, 360, 45), "메뉴로 돌아가기", button)) ReturnToMenu();
            GUI.enabled = true;
            GUI.Label(new Rect(-280, 185, 560, 45), "WASD 이동 · Space 점프 · Esc 맵 선택\n메뉴로 돌아가면 현재 맵 진행 상태가 초기화됩니다.", body);
            GUI.matrix = oldMatrix;
        }

        void OnDisable()
        {
            if (IsOpen) Time.timeScale = previousTimeScale;
        }
    }
}
