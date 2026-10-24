# Lina Style — 산 모양 3D 객체 예제 (Unity 6 URP)

이 패키지에는 공개 게임 화면의 파스텔톤 풍경에서 영감을 얻은 **완전히 새로 제작한 산 메시(OBJ)** 와, 기존 `LinaStyle/StylizedSoftLit_URP` 커스텀 쉐이더를 이 산에 적용할 수 있는 Unity 에디터 도구가 포함됩니다. 원작 게임의 리소스를 추출한 것이 아닙니다.

## 무엇을 만들었나요?

- `Models/Mountain_Alpine.obj`: 실제 산 모양 **3D 모델** (메시)
- `Models/Mountain_Alpine.mtl`: OBJ 전용 기본 색상 정의. Unity 데모에서는 이 값 대신 **Lina 전용 URP 머티리얼**을 사용합니다.
- `Materials/MAT_05_Mint_Terrain.mat`: 산 아래쪽의 민트색 지표면
- `Materials/MAT_04_Lavender_Stone.mat`: 산 중턱의 보라색 바위
- `Materials/MAT_09_Snow_Cream.mat`: 밝고 은은하게 빛나는 정상부
- `Editor/LinaMountainDemoCreator.cs`: 데모 씬과 드래그 가능한 산 프리팹을 만드는 에디터 메뉴
- `Demo/Lina_Mountain_Demo.unity`: **메뉴 실행 시 생성됨**
- `Demo/PF_Lina_Mountain.prefab`: **메뉴 실행 시 생성됨**

## 설치하기 (처음 쓰는 사람용)

1. Unity Hub에서 **Universal 3D / URP** 프로젝트를 엽니다. 대상은 Unity 6 (6000.6.x 계열)입니다.
2. `LinaStylePack_Unity6_MountainDemo.zip`의 압축을 풀어 **`LinaStylePack` 폴더 전체를** Unity 프로젝트의 `Assets` 폴더 안에 복사하세요.
   - 예시: `MyProject/Assets/LinaStylePack/Models/Mountain_Alpine.obj`
   - 이미 이전 LinaStylePack을 사용 중이면, 기존 파일을 별도로 백업한 뒤 새 패키지 내용으로 병합하세요.
3. Unity가 파일을 읽고 C# 코드를 컴파일할 때까지 기다립니다. Console에 **빨간 오류가 없는지** 확인합니다.
4. 상단 메뉴 **`Tools > Lina Style > 2 - Create and Open Mountain Demo`** 를 선택합니다.
5. Unity가 변경된 기존 씬의 저장을 물으면 저장할지 선택합니다. 새로운 데모 씬이 열립니다.
6. **Game 탭**을 클릭합니다. 산을 중심으로 하늘색 배경, 바닥, 조명, 발광 장식이 보입니다.
7. 생성된 `Assets/LinaStylePack/Demo/PF_Lina_Mountain.prefab`을 다른 씬에 드래그하면 산을 재사용할 수 있습니다.

### Unity 패키지 파일로 설치하고 싶다면

`Assets > Import Package > Custom Package`에서 `.unitypackage` 파일을 선택한 뒤 Import합니다. 기존 파일과 이름이 같으면 가져올 항목을 확인하세요. 이후 위 **4번부터** 진행합니다.

## 쉐이더 조절하기

1. `Project > Assets > LinaStylePack > Materials`를 엽니다.
2. `MAT_04_Lavender_Stone.mat`을 선택합니다.
3. Inspector의 `Base Color / Tint`를 바꾸면 산 중턱 색이 변합니다.
4. `Shadow Tint`, `Shadow Transition Softness`, `Soft Light Wrap`, `Edge Light Strength` 등도 시험해 보세요.
5. **원본 머티리얼을 유지**하고 싶으면 `Ctrl + D`로 복제하여 수정한 뒤 산 메시의 MeshRenderer 슬롯에 넣으세요.
6. `MAT_09_Snow_Cream.mat`으로 정상부 색상을 조절할 수 있습니다.

## 문제가 생길 경우

- 산이 전부 보라색으로만 보인다: 산 모델을 선택하고 MeshRenderer의 Materials 슬롯이 3개(민트 / 바위 / 눈)인지 확인하세요. OBJ 임포트 과정에 따라 세 부분이 별도 자식 오브젝트로 분리되기도 합니다. 각 자식의 이름 Grass, LavenderRock, SnowCap에 해당하는 머티리얼을 적용하세요.
- 산이 핑크색으로 보인다: 반드시 **URP 프로젝트**이고 `Shaders/Lina_StylizedSoftLit_URP.shader`가 오류 없이 컴파일되었는지 확인하세요.
- 위 메뉴가 없다: `Editor/LinaMountainDemoCreator.cs` 파일을 Unity가 불러왔는지, `Window > General > Console`에서 오류가 없는지 확인하세요.
- 하늘이 매우 어둡거나 빛이 부자연스럽다: Directional Light, Global Volume, 프로젝트의 URP Render Pipeline Asset을 확인하세요.
- OBJ는 Unity가 아닌 Blender에서도 열 수 있습니다. OBJ/MTL 기본 색상은 편의용이며 전용 URP 쉐이더는 Unity에서만 적용됩니다.

## 미리보기의 의미

제공되는 `Lina_Mountain_Preview.png`는 동일한 산 메시의 색상/형태를 설명하기 위해 **별도 3D 렌더러로 생성한 이미지**입니다. 실제 Unity URP/HLSL 쉐이더를 직접 렌더링한 결과는 아닙니다. Unity 에디터가 설치되지 않은 환경에서 제작했으므로 Unity 6000.6.4f1에서 실제 컴파일과 실행을 검증했다고 주장하지 않습니다.
