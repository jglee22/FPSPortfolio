# WAVEBREAK

웨이브 브레이크. 웨이브를 클리어하는 1인칭 슈팅 포트폴리오입니다.  
Unity 2021.3.39f1 LTS + URP.

적 웨이브 1~5를 막으면서 아이템·강화를 모으고, 보스 웨이브 후 Mission Clear가 목표입니다.  
같은 5웨이브에서 콤보와 클리어 시간으로 기록을 갱신하는 짧은 아케이드 루프입니다.

## 실행 방법

1. Unity 2021.3.39f1에서 프로젝트를 연다.
2. 유료 에셋 **KINEMATION FPS Animation Ultimate**를 임포트한다. (`Assets/KINEMATION`은 저장소에 없음)
3. `Main` 씬에서 `Camera` 아래에 `SK_Arms_Mono`를 두고, 총을 `ik_hand_gun`에 연결한다.
4. 타이틀은 `Assets/Scenes/Lobby.unity`, 플레이는 `Assets/Scenes/Main.unity`에서 Play.

저장소만 Clone하면 유료 에셋이 없어 바로 Play하기 어렵다. 리뷰는 플레이 영상·스크린샷·실행 빌드와 GitHub 소스를 함께 보면 된다.

사격·재장전·탄약·반동은 에셋 플레이어 스크립트가 아니라 아래 `Gun` 계열을 사용합니다.  
KINEMATION은 1인칭 팔/총 애니메이션 클립과 Animator Override만 사용합니다.

## 조작

| 키 | 동작 |
|---|---|
| WASD | 이동 |
| 마우스 | 시점 |
| Shift | 스프린트 |
| Space | 점프 |
| C | 앉기 |
| Q / E | 좌 / 우 기울기 |
| 마우스 왼쪽 | 사격 |
| R | 재장전 |
| 1 / 2 | 라이플 / 샷건 |
| B | 샷건 연사/단발 전환 |
| G | 수류탄 |
| Esc | 일시정지 |

## 플레이 영상 / 스크린샷

- 추가 예정

## 문제 해결 사례

### 1. Camera Look / Recoil / Lean / Shake 충돌 → CameraRig 분리

마우스 시야, 반동, 기울기, 피격 흔들림을 Camera transform에 각각 쓰면 값이 덮어씌워진다.  
`CameraRig`가 pitch/recoil/lean/shake를 한곳에서 합산하고, 각 시스템은 자기 오프셋만 넣는다.

### 2. 풀 재사용 시 Enemy 상태 누적 → Spawn 초기화

풀에서 꺼낸 적이 이전 체력·NavMesh·사망 플래그를 그대로 들고 나오는 문제가 있었다.  
`EnemyAI.InitializeForSpawn`에서 `EnemyData` + `WaveData` 배율로 스탯을 다시 넣고, 콜라이더/Animator/경로를 리셋한다.

### 3. 무한 웨이브 → WaveData 기반 5 Wave + Reward + Boss + Result

스폰만 반복하면 종료 조건이 없고 난이도 곡선도 없다.  
`WaveData` 5개로 구성·배율을 고정하고, 웨이브 클리어 보상 → 보스 → Mission Clear Result로 한 판이 끝나게 했다.

## 주요 스크립트 및 기능

### 1. Player
- **PlayerMovement.cs** — 이동, 점프, 앉기, 스프린트
- **PlayerHealth.cs** — 체력, 체력바 UI, 피격 시 화면 흔들림과 붉은 오버레이
- **DamageOverlay.cs** — 피격 화면 효과
- **Leaning.cs** — Q/E 좌우 기울기
- **CameraShake.cs** / **CameraRig.cs** — 피격·폭발·사격 시 카메라 흔들림과 시야 합성

### 2. 무기
- **Gun.cs** — 사격, 재장전, 크로스헤어, 머즐 플래시. 샷건은 더블배럴이라 장탄 2발 고정(`lockMaxAmmo`). 재장전 완료는 `ReloadComplete()` (애니메이션 길이에 맞춰 Animator 속도 조정, Animation Event에서도 호출 가능)
- **FPSViewModel.cs** — 씬에 올려 둔 팔/총 Animator로 Idle, Fire, Reload 재생. 라이플 연사는 캐릭터 Fire를 매 발 처음부터 끊지 않고, 무기 Fire(0.1s)만 재시작한다
- **WeaponRecoil.cs** / **GunRecoil.cs** — 뷰모델 반동, 카메라 반동
- **GunController.cs** — 1/2 무기 전환 (라이플, 샷건)
- **WeaponUpgradeItem.cs** — 웨이브 보상용 강화 (데미지 +4, 라이플 탄창 +6, 연사/재장전 ×0.88). 샷건 장탄은 고정

### 3. 웨이브 / 적
- **WaveData.cs** — 웨이브별 적 구성, 스폰 간격, 보스 여부, 체력/데미지/이속 배율
- **EnemyPoolManager.cs** — 웨이브 1~5 유한 스폰, 풀링, 웨이브 배너, 보스 HP UI. 마지막 웨이브 클리어 시 Mission Clear
- **WaveRewardUI.cs** — 웨이브 클리어 후 강화 3개 중 선택. 카드에 실제 수치 표시
- **EnemyData.cs** — 적 스탯. 일반 좀비는 같은 텍스처에 색만 다른 변주. 러너는 Run 애니 + 녹색 틴트, 이속 8 / 체력 35
- **EnemyAI.cs** — 이동, 공격, 사망, 드랍. `EnemyData`와 웨이브 배율 적용. 보스는 HP 50% 이하에서 이동/공격 속도 상승
- **EnemyCounterManager.cs** — 생존 적 수 UI

### 4. 아이템 / 수류탄
- **ConsumableItem.cs** — 탄약, 체력, 수류탄 소모품
- **ItemPickup.cs** — 드랍 획득 및 픽업 메시지
- **GrenadeThrower.cs** / **Grenade.cs** — 투척, 쿨다운, 범위 데미지

### 5. UI / 로비
- **HealthBar.cs** — 체력바
- **LobbyManager.cs** — 로비, 최고 점수 / 최고 콤보 / 최단 클리어
- **MenuManager.cs** — 일시정지, 로비 이동, 게임 종료. Result에 점수·콤보·클리어/생존 시간 표시. Reward 화면이 열려 있으면 Pause가 열리지 않음
- **ScoreManager.cs** — 킬 콤보(2.5초). 연속 처치는 제한 없고 점수 배율만 최대 x5. JSON에 최고 점수·최고 콤보·최단 클리어 저장. Retry는 Main 씬을 다시 로드해 점수/웨이브/풀을 초기화한다
- **PickupMessageManager.cs** — 아이템 획득 메시지
- **ButtonScaleEffect.cs** — 버튼 오버 스케일
