using UnityEngine;
using UnityEngine.SceneManagement;

namespace CityCourier
{
    public sealed class CourierMainMenu : MonoBehaviour
    {
        public string Title = "메뉴씬";
        public string GameplayScene = "sampleScene_PrefabTest";
        bool loading;

        void Start()
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void StartGameplay()
        {
            if (loading) return;
            loading = true;
            Time.timeScale = 1;
            SceneManager.LoadSceneAsync(GameplayScene);
        }

        void OnGUI()
        {
            float scale = Mathf.Max(0.25f, Mathf.Min(Screen.width / 800f, Screen.height / 500f));
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(Screen.width / 2f, Screen.height / 2f, 0), Quaternion.identity, Vector3.one * scale);
            GUI.Box(new Rect(-260, -175, 520, 350), "");
            var title = new GUIStyle(GUI.skin.label) { fontSize = 36, alignment = TextAnchor.MiddleCenter };
            var button = new GUIStyle(GUI.skin.button) { fontSize = 24 };
            GUI.Label(new Rect(-240, -145, 480, 70), Title, title);
            GUI.enabled = !loading;
            if (GUI.Button(new Rect(-180, -35, 360, 65), loading ? "불러오는 중…" : "게임플레이", button))
                StartGameplay();
            // Placeholder: settings will be connected in a later iteration.
            GUI.Button(new Rect(-180, 55, 360, 65), "설정", button);
            GUI.enabled = true;
            GUI.matrix = oldMatrix;
        }
    }
}
