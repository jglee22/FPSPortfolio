# FPS Portfolio

웨이브를 클리어하는 1인칭 슈팅 포트폴리오입니다.  
Unity 2021.3.39f1 LTS + URP.

적 웨이브 1~5를 막으면서 아이템·강화를 모으고, 보스 웨이브 후 Mission Clear가 목표입니다.

## 실행 방법

1. Unity 2021.3.39f1에서 프로젝트를 연다.
2. 유료 에셋 **KINEMATION FPS Animation Ultimate**를 임포트한다. (`Assets/KINEMATION`은 저장소에 없음)
3. `Main` 씬에서 `Camera` 아래에 `SK_Arms_Mono`를 두고, 총을 `ik_hand_gun`에 연결한다.
4. 타이틀은 `Assets/Scenes/Lobby.unity`, 플레이는 `Assets/Scenes/Main.unity`에서 Play.

사격·재장전·탄약·반동은 에셋 플레이어 스크립트가 아니라 아래 `Gun` 계열을 사용합니다.

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

## 영상 / 스크린샷

플레이 영상이나 스크린샷 링크가 있으면 여기에 넣으면 됩니다.

## 주요 스크립트 및 기능

### 1. Player
- **PlayerMovement.cs** — 이동, 점프, 앉기, 스프린트
- **PlayerHealth.cs** — 체력, 체력바 UI, 피격 시 화면 흔들림과 붉은 오버레이
- **DamageOverlay.cs** — 피격 화면 효과
- **Leaning.cs** — Q/E 좌우 기울기
- **CameraShake.cs** / 카메라 리그 — 피격·폭발·사격 시 카메라 흔들림

### 2. 무기
- **Gun.cs** — 사격, 재장전, 크로스헤어, 머즐 플래시. 샷건은 더블배럴이라 장탄 2발 고정(`lockMaxAmmo`)
- **FPSViewModel.cs** — 씬에 올려 둔 팔/총 Animator로 Idle, Fire, Reload 재생
- **WeaponRecoil.cs** / **GunRecoil.cs** — 뷰모델 반동, 카메라 반동
- **GunController.cs** — 1/2 무기 전환 (라이플, 샷건)
- **WeaponUpgradeItem.cs** — 웨이브 보상용 강화 (데미지, 탄창, 재장전, 연사). 탄창 증가는 샷건에 적용되지 않음

### 3. 웨이브 / 적
- **WaveData.cs** — 웨이브별 적 구성, 스폰 간격, 보스 여부, 체력/데미지/이속 배율
- **EnemyPoolManager.cs** — 웨이브 1~5 유한 스폰, 풀링, 웨이브 배너, 보스 HP UI. 마지막 웨이브 클리어 시 Mission Clear
- **WaveRewardUI.cs** — 웨이브 클리어 후 강화 3개 중 선택
- **EnemyAI.cs** — 이동, 공격, 사망, 드랍. `EnemyData`와 웨이브 배율 적용
- **EnemyCounterManager.cs** — 생존 적 수 UI

### 4. 아이템 / 수류탄
- **ConsumableItem.cs** — 탄약, 체력, 수류탄 소모품
- **ItemPickup.cs** — 드랍 획득 및 픽업 메시지
- **GrenadeThrower.cs** / **Grenade.cs** — 투척, 쿨다운, 범위 데미지

### 5. UI / 로비
- **HealthBar.cs** — 체력바
- **LobbyManager.cs** — 로비, 최고 점수
- **MenuManager.cs** — 일시정지, 로비 이동, 게임 종료
- **ScoreManager.cs** — JSON 최고 점수 저장/로드
- **PickupMessageManager.cs** — 아이템 획득 메시지
- **ButtonScaleEffect.cs** — 버튼 오버 스케일
