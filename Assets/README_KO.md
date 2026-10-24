# Lina Style — Unity 6 URP Shader & Material Pack

**비공식 팬 스타일 연습용 프리셋입니다.** Lightspeed Lina의 공식 에셋이나 실제 게임 내부 셰이더를 추출한 것이 아닙니다. 공개 화면의 밝은 보라/분홍/민트 색상, 부드러운 명암과 테두리빛을 참고하여 새로 제작한 독립적인 예제입니다.

## 0. 가장 중요한 조건
- Unity 6 / 6000.6.4f1 계열
- **URP(Universal Render Pipeline)** 프로젝트 (Unity Hub에서 `Universal 3D` 템플릿 사용)
- Built-in 3D 또는 HDRP 프로젝트에는 사용할 수 없습니다.
- Unity Editor 직접 실행 검증은 이루어지지 않았습니다. 문제 발생 시 Console 오류를 알려주세요.

## 1. ZIP 압축파일 설치 방법 (가장 간단하고 안전함)
1. 이 파일이 들어 있는 ZIP을 풀어 `LinaStylePack` 폴더를 찾습니다.
2. Unity를 열고 하단 **Project** 창에서 `Assets` 폴더를 엽니다.
3. 컴퓨터에서 `LinaStylePack` 폴더 전체를 Unity의 `Assets` 아래로 복사/드래그합니다.
   - 예상 경로: `Assets/LinaStylePack/Shaders/Lina_StylizedSoftLit_URP.shader`
   - 예상 경로: `Assets/LinaStylePack/Materials/MAT_01_Pink_Hair.mat`
4. 스크립트 컴파일/셰이더 임포트가 끝나면 상단 메뉴에서 **Tools > Lina Style > 1 - Create and Open Demo Scene**을 클릭합니다.
5. `Game` 탭을 눌러 구체/캐릭터 연습 모델과 머티리얼 프리셋을 확인하세요.
6. `.mat` 파일을 원하는 3D 모델에 드래그하면 바로 적용됩니다.

**UnityPackage 설치 방법:** `.unitypackage`가 동작하면 Unity 메뉴 `Assets > Import Package > Custom Package`로 가져올 수 있습니다. ZIP 직접 복사가 더 보편적으로 문제를 해결하기 쉽습니다.

## 2. 미리 제작한 8가지 머티리얼
| 파일명 | 용도 | 특징 |
|---|---|---|
| MAT_01_Pink_Hair.mat | 머리카락/핑크 장식 | 분홍색 + 강조된 윤곽 빛 |
| MAT_02_Violet_Outfit.mat | 의상 | 보라색 + 은은한 원단 패턴 |
| MAT_03_Skin_Peach.mat | 피부/얼굴 | 살구색 + 매우 부드러운 명암 |
| MAT_04_Lavender_Stone.mat | 바위/석재 | 보라빛 돌 텍스처 + 노멀 맵 |
| MAT_05_Mint_Terrain.mat | 배경/잔디 | 민트색 지형 텍스처 |
| MAT_06_Cyan_Crystal.mat | 광석/빛나는 장치 | 시안 발광 + 림 라이트 |
| MAT_07_Pink_Neon.mat | 네온/장비 포인트 | 분홍 발광 |
| MAT_08_Warm_Sandstone.mat | 따뜻한 석재 | 황갈색 돌 텍스처 |

## 3. 머티리얼 값 바꾸는 법
1. Project > Assets > LinaStylePack > Materials에서 바꿀 `.mat` 파일을 한 번 클릭합니다.
2. 오른쪽 Inspector에서 **Base Color / Tint**를 바꾸면 물체 기본 색상이 변합니다.
3. **Shadow Tint**는 어두운 쪽의 색, **Shadow Transition Softness**는 명암 경계의 부드러움을 정합니다.
4. **Edge Light Strength**는 가장자리 빛, **Soft Highlight**는 반짝임을 정합니다.
5. **Glow Strength**를 높이면 발광합니다. 실제 Glow가 화면에서 번지려면 URP HDR + Volume Bloom이 필요합니다.
6. 수정 전 원본을 보존하려면 Material을 Ctrl+D로 복제한 후 복제본을 수정하세요.

## 4. 셰이더 구성
- `Shaders/Lina_StylizedSoftLit_URP.shader`: Unity URP HLSL 커스텀 셰이더
- 방향광, 메인 광원 그림자, 간단한 추가 광원, 환경광, 소프트 카툰형 명암, Rim Light, Specular, Emission, Fog
- Base Texture 및 Optional Normal Map 지원
- URP Lit 내장 ShadowCaster/DepthOnly pass 재사용
- 직접 Shader Graph 파일은 아닙니다. HLSL 셰이더이지만 Inspector에서 숫자 및 색상을 바꾸는 데 별도 코딩은 필요하지 않습니다.

## 5. 그림자 / 노멀 맵 주의
- 이 예제는 이해하기 쉬운 전방 렌더링용 셰이더입니다. URP Deferred, 고급 GI, Screen Space Ambient Occlusion 등 모든 파이프라인 기능을 완벽하게 구현한 셰이더는 아닙니다.
- `TX_LavenderRock_Normal.png`는 Normal Map 형식으로 지정해야 합니다. Unity에서 텍스처를 선택하고 Inspector의 **Texture Type > Normal map**이 아니라면 변경하고 Apply 합니다.
- 간혹 이전 버전 임포터에서 .mat 텍스처 연결이 끊어질 수 있습니다. 그럴 때는 각 Stone Material의 `Base Texture`에 `TX_LavenderRock`, `Normal Map`에 `TX_LavenderRock_Normal`을 넣으세요.
- HDRP/Built-in 파이프라인에서는 **핑크색 오류 머티리얼**이 나올 수 있습니다.

## 6. 빛나는 물체가 충분히 빛나지 않으면
- Main Camera > Rendering > Post Processing을 체크합니다.
- Global Volume에 Bloom이 활성화되었는지 확인합니다.
- URP Asset과 Camera에서 HDR을 켭니다.
- Bloom Threshold 1.1 / Intensity 0.3 정도부터 조절합니다.

## 7. 문제가 있을 때
`Window > General > Console`에서 붉은 오류를 클릭하고 전체 메시지를 복사하세요.
- Shader 컴파일 오류: URP 버전/설치, Shader include 경로 확인
- C# 오류: URP 패키지를 설치하고 Universal 3D 템플릿에서 재시도
- 머티리얼 핑크색: Edit > Project Settings > Graphics의 렌더 파이프라인 확인
- 텍스처가 검거나 이상함: Normal Map 임포트 유형 확인

## 8. 저작권 및 한계
이 파일의 텍스처/코드/데모 오브젝트는 독립적으로 새로 제작한 것입니다. 원작의 모델, 텍스처, 사운드, 셰이더 소스는 포함되어 있지 않습니다. 게임과 비슷한 스타일을 연습하기 위한 출발점이며 원본과 픽셀 단위로 동일한 결과를 보장하지 않습니다.


## 9. 새 기능: 산 모양 3D 객체 예제
- `Models/Mountain_Alpine.obj`는 직접 만든 실제 산 메시입니다.
- 상단 `Tools > Lina Style > 2 - Create and Open Mountain Demo`에서 전용 데모를 만듭니다.
- `MOUNTAIN_README_KO.md`에 초보자 설치법, 정상부/바위 머티리얼 수정법을 자세히 적었습니다.
