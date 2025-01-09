주요 스크립트 및 기능
1. Player 관련
PlayerMovement.cs
플레이어의 이동, 점프, 앉기, 스프린트 등 기본 동작을 구현.
PlayerHealth.cs
플레이어 체력 관리, 체력 바 UI와 연동, 피격 시 화면 흔들림과 붉은 효과 적용.
DamageOverlay.cs
피격 시 화면에 붉은 효과 표시.

2. 무기 시스템
Gun.cs
무기 타입별 설정, 사격 및 재장전, 반동 처리, 크로스헤어 UI 업데이트.
WeaponRecoil.cs / GunRecoil.cs
무기 반동과 카메라 흔들림 구현.
GunController.cs
무기 전환 시스템 구현.
WeaponUpgradeItem.cs
무기 업그레이드 데이터 관리 (데미지 증가, 탄약 증가 등).

3. 적 AI 및 관리
EnemyAI.cs
적의 이동, 공격, 사망, 드랍 아이템 관리.
EnemyPoolManager.cs
웨이브 기반 적 스폰 및 적 풀링 시스템 관리.
EnemyCounterManager.cs
살아있는 적 수를 관리하고 UI에 표시.

4. 수류탄 시스템
GrenadeThrower.cs
수류탄 투척 시스템, 쿨다운 및 개수 관리.
Grenade.cs
수류탄 폭발 효과, 범위 데미지 및 물리 충격 처리.

5. UI 관련
HealthBar.cs
플레이어 체력바 UI 업데이트.
ButtonScaleEffect.cs
UI 버튼 마우스 오버 시 스케일 애니메이션 효과.
LobbyManager.cs
로비 화면 UI 관리, 최고 점수 로드 및 표시.
MenuManager.cs
일시정지 메뉴, 게임 종료 및 로비 이동 기능.
ScoreManager.cs
Json을 이용한 최고 점수 저장 및 다음 게임에서 불러와 화면에 표시.

6. 추가 기능
Leaning.cs
플레이어 기울기 동작(Q/E 키를 이용한 좌/우 기울기).
CameraShake.cs
피격 및 폭발 시 카메라 흔들림 효과.