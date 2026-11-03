using UnityEngine;
using CityCourier;

public class MapManager : MonoBehaviour
{
    // 맵 프리팹 목록
    public GameObject[] mapPrefabs;

    // 현재 맵이 배치될 부모
    public Transform mapContainer;

    // 기존 플레이어
    public CourierMotor player;

    // 현재 생성된 맵
    private GameObject currentMap;

    void Start()
    {
        // 게임 시작 시 첫 번째 맵 불러오기
        LoadMap(0);
    }

    public void LoadMap(int index)
    {
        // 맵 번호 검사
        if (index < 0 || index >= mapPrefabs.Length)
            return;

        if (mapPrefabs[index] == null ||
            mapContainer == null ||
            player == null)
            return;

        // 기존 맵 제거
        if (currentMap != null)
        {
            Destroy(currentMap);
        }

        // 새로운 맵 프리팹 생성
        currentMap = Instantiate(
            mapPrefabs[index],
            Vector3.zero,
            Quaternion.identity,
            mapContainer
        );

        // 새 맵의 CourierStage 가져오기
        CourierStage stage =
            currentMap.GetComponent<CourierStage>();

        if (stage == null)
        {
            Debug.LogError("맵에 CourierStage가 없습니다.");
            return;
        }

        // 플레이어와 스테이지 연결
        stage.Player = player;
        player.Stage = stage;

        // 플레이어를 맵 시작 위치로 이동
        player.Teleport(new Vector3(0, 0.1f, -8));

        Debug.Log("맵 로딩 완료: " + index);
    }
}

