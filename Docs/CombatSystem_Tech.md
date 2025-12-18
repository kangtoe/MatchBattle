# 전투 시스템 기술 문서

**목적**: 순수 턴제 RPG 전투 구현 가이드
**대상**: 프로그래머

---

## 📐 데이터 구조

### Player 클래스
```csharp
public class Player {
    // 기본 스탯
    public int currentHP;
    public int maxHP;
    public int defense;
    public int maxDefense;
    public int gold;

    // 상태 효과
    public List<StatusEffect> statusEffects;

    // 이벤트
    public UnityEvent<int> OnHPChanged;
    public UnityEvent<int> OnDefenseChanged;
    public UnityEvent OnDeath;
}
```

### Enemy 클래스
```csharp
public class Enemy {
    // 기본 정보
    public string enemyName;
    public int currentHP;
    public int maxHP;
    public int defense;

    // AI 패턴
    public List<EnemyAction> actionPool;
    public EnemyAction nextAction;
    public EnemyAction currentAction;

    // 특수 능력
    public bool hasEnragePhase;  // HP 50% 이하 강화
    public int enrageBonus;

    // 이벤트
    public UnityEvent<int> OnHPChanged;
    public UnityEvent OnDeath;
}
```

### EnemyAction 클래스
```csharp
public class EnemyAction {
    public EnemyActionType type;
    public int value;              // 데미지 or 방어력 등
    public bool needsTelegraph;    // 예고 필요 여부
    public float weight;           // 선택 확률 가중치
    public string description;     // UI 표시용
}

public enum EnemyActionType {
    Attack,        // 공격
    HeavyAttack,   // 강공격 (예고 필요)
    Defend,        // 방어
    Buff,          // 버프
    Debuff         // 디버프
}
```

### StatusEffect 클래스
```csharp
public class StatusEffect {
    public StatusEffectType type;
    public int value;       // 효과량
    public int duration;    // 남은 턴 수
    public string icon;     // UI 아이콘
}

public enum StatusEffectType {
    DOT,           // 지속 데미지
    AttackBuff,    // 공격력 증가
    DefenseBuff,   // 방어력 증가
    Evasion,       // 회피
    // ...
}
```

---

## 🎮 CombatManager 구조

### CombatManager 클래스
```csharp
public class CombatManager : MonoBehaviour {
    // 싱글톤
    public static CombatManager Instance { get; private set; }

    // 전투 참가자
    public Player player;
    public Enemy currentEnemy;

    // 전투 상태
    public CombatState currentState;
    public int turnCount;

    // 참조
    public BoardManager boardManager;
    public CombatUI combatUI;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
}

public enum CombatState {
    Start,          // 전투 시작
    PlayerTurn,     // 플레이어 턴
    EnemyTurn,      // 적 턴
    Victory,        // 승리
    Defeat          // 패배
}
```

---

## 🔄 턴 관리 시스템

### 전투 시작
```csharp
public void StartCombat(EnemyData enemyData) {
    // 1. 전투 초기화
    turnCount = 0;
    currentState = CombatState.Start;

    // 2. 플레이어 초기화 (런 진행 중이면 이전 상태 유지)
    if (player == null) {
        player = new Player();
        player.currentHP = player.maxHP;
        player.defense = 0;
    }

    // 3. 적 생성
    currentEnemy = CreateEnemy(enemyData);

    // 4. UI 업데이트
    combatUI.SetupBattle(player, currentEnemy);

    // 5. 적 첫 행동 결정
    currentEnemy.nextAction = SelectEnemyAction(currentEnemy);
    combatUI.ShowEnemyIntent(currentEnemy.nextAction);

    // 6. 플레이어 턴 시작
    StartPlayerTurn();
}
```

### 플레이어 턴
```csharp
void StartPlayerTurn() {
    currentState = CombatState.PlayerTurn;
    turnCount++;

    // 1. 턴 시작 효과 (DOT 등)
    ApplyTurnStartEffects(player);

    // 2. 보드 활성화
    boardManager.EnablePlayerInput();

    // 3. UI 업데이트
    combatUI.ShowPlayerTurn();

    // 플레이어가 블록 매치 후 EndPlayerTurn() 호출
}

public void EndPlayerTurn() {
    // 1. 보드 비활성화
    boardManager.DisablePlayerInput();

    // 2. 턴 종료 효과
    ApplyTurnEndEffects(player);

    // 3. 적 턴으로 전환
    StartCoroutine(StartEnemyTurnDelayed(1.0f));
}
```

### 적 턴
```csharp
IEnumerator StartEnemyTurnDelayed(float delay) {
    yield return new WaitForSeconds(delay);

    currentState = CombatState.EnemyTurn;

    // 1. 턴 시작 효과
    ApplyTurnStartEffects(currentEnemy);

    // 2. 현재 행동 실행
    yield return StartCoroutine(ExecuteEnemyAction(currentEnemy.nextAction));

    // 3. 다음 행동 선택
    currentEnemy.nextAction = SelectEnemyAction(currentEnemy);
    combatUI.ShowEnemyIntent(currentEnemy.nextAction);

    // 4. 승패 판정
    if (player.currentHP <= 0) {
        StartCoroutine(HandleDefeat());
        yield break;
    }

    if (currentEnemy.currentHP <= 0) {
        StartCoroutine(HandleVictory());
        yield break;
    }

    // 5. 플레이어 턴으로 복귀
    yield return new WaitForSeconds(1.0f);
    StartPlayerTurn();
}
```

---

## ⚔️ 데미지 시스템

### 플레이어 → 적 공격
```csharp
public void DealDamage(int baseDamage) {
    // 1. 버프 적용
    float multiplier = 1.0f;
    foreach (var effect in player.statusEffects) {
        if (effect.type == StatusEffectType.AttackBuff) {
            multiplier += effect.value / 100f;
        }
    }

    int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);

    // 2. 적 방어력 적용
    int actualDamage = Mathf.Max(0, finalDamage - currentEnemy.defense);
    currentEnemy.defense = Mathf.Max(0, currentEnemy.defense - finalDamage);

    // 3. HP 감소
    currentEnemy.currentHP -= actualDamage;
    currentEnemy.currentHP = Mathf.Max(0, currentEnemy.currentHP);

    // 4. UI 업데이트
    combatUI.ShowDamage(currentEnemy, actualDamage);
    combatUI.UpdateEnemyHP(currentEnemy);

    // 5. 적 사망 체크
    if (currentEnemy.currentHP <= 0) {
        currentEnemy.OnDeath?.Invoke();
    }
}
```

### 적 → 플레이어 공격
```csharp
void DealDamageToPlayer(int damage) {
    // 1. 회피 체크
    if (CheckEvasion(player)) {
        combatUI.ShowEvasion(player);
        return;
    }

    // 2. 방어력 계산
    if (player.defense >= damage) {
        // 방어력으로 완전히 막음
        player.defense -= damage;
        combatUI.ShowDefenseAbsorb(player, damage);
    } else {
        // 방어력 먼저 소모, 남은 데미지는 HP
        int remainingDamage = damage - player.defense;

        if (player.defense > 0) {
            combatUI.ShowDefenseAbsorb(player, player.defense);
        }

        player.defense = 0;
        player.currentHP -= remainingDamage;
        player.currentHP = Mathf.Max(0, player.currentHP);

        combatUI.ShowDamage(player, remainingDamage);
    }

    // 3. UI 업데이트
    combatUI.UpdatePlayerHP(player);
    combatUI.UpdatePlayerDefense(player);

    // 4. 사망 체크
    if (player.currentHP <= 0) {
        player.OnDeath?.Invoke();
    }
}
```

### 방어력 추가
```csharp
public void AddDefense(int amount) {
    player.defense += amount;
    player.defense = Mathf.Min(player.defense, player.maxDefense);

    combatUI.ShowDefenseGain(player, amount);
    combatUI.UpdatePlayerDefense(player);
}
```

### 회복
```csharp
public void HealPlayer(int amount) {
    int actualHeal = Mathf.Min(amount, player.maxHP - player.currentHP);
    player.currentHP += actualHeal;

    combatUI.ShowHeal(player, actualHeal);
    combatUI.UpdatePlayerHP(player);
}
```

---

## 🤖 적 AI 시스템

### 행동 선택 알고리즘
```csharp
EnemyAction SelectEnemyAction(Enemy enemy) {
    // 1. 가능한 행동 리스트
    List<EnemyAction> availableActions = new List<EnemyAction>(enemy.actionPool);

    // 2. 특수 조건 체크 (Enrage 등)
    if (enemy.hasEnragePhase &&
        enemy.currentHP <= enemy.maxHP / 2 &&
        !enemy.isEnraged) {

        enemy.isEnraged = true;
        ApplyEnrage(enemy);
    }

    // 3. 가중치 기반 랜덤 선택
    float totalWeight = 0;
    foreach (var action in availableActions) {
        totalWeight += action.weight;
    }

    float rand = Random.Range(0f, totalWeight);
    float cumulative = 0;

    foreach (var action in availableActions) {
        cumulative += action.weight;
        if (rand <= cumulative) {
            return action;
        }
    }

    return availableActions[0]; // Fallback
}
```

### 행동 실행
```csharp
IEnumerator ExecuteEnemyAction(EnemyAction action) {
    // 애니메이션 재생
    PlayEnemyAnimation(action.type);

    yield return new WaitForSeconds(0.5f);

    switch (action.type) {
        case EnemyActionType.Attack:
            DealDamageToPlayer(action.value);
            break;

        case EnemyActionType.HeavyAttack:
            DealDamageToPlayer(action.value);
            CameraShake();
            break;

        case EnemyActionType.Defend:
            currentEnemy.defense += action.value;
            combatUI.ShowDefenseGain(currentEnemy, action.value);
            break;

        case EnemyActionType.Buff:
            ApplyBuff(currentEnemy, action);
            break;

        case EnemyActionType.Debuff:
            ApplyDebuff(player, action);
            break;
    }

    yield return new WaitForSeconds(0.5f);
}
```

### Enrage 시스템
```csharp
void ApplyEnrage(Enemy enemy) {
    // 모든 공격 행동의 데미지 증가
    foreach (var action in enemy.actionPool) {
        if (action.type == EnemyActionType.Attack ||
            action.type == EnemyActionType.HeavyAttack) {
            action.value += enemy.enrageBonus;
        }
    }

    // UI 표시
    combatUI.ShowEnrageEffect(enemy);
    combatUI.ShowMessage("오크가 분노했다!");
}
```

---

## 🎁 상태 효과 시스템

### 상태 효과 적용
```csharp
public void ApplyStatusEffect(StatusEffect effect) {
    // 기존 같은 타입 효과 찾기
    StatusEffect existing = player.statusEffects.Find(e => e.type == effect.type);

    if (existing != null) {
        // 스택 or 갱신
        existing.duration = Mathf.Max(existing.duration, effect.duration);
        existing.value += effect.value;
    } else {
        // 새로 추가
        player.statusEffects.Add(effect);
    }

    combatUI.UpdateStatusEffects(player);
}
```

### 턴 시작 효과 처리
```csharp
void ApplyTurnStartEffects(Player player) {
    List<StatusEffect> toRemove = new List<StatusEffect>();

    foreach (var effect in player.statusEffects) {
        switch (effect.type) {
            case StatusEffectType.DOT:
                player.currentHP -= effect.value;
                combatUI.ShowDOT(player, effect.value);
                break;

            // 다른 효과들...
        }

        // 지속 시간 감소
        effect.duration--;
        if (effect.duration <= 0) {
            toRemove.Add(effect);
        }
    }

    // 만료된 효과 제거
    foreach (var effect in toRemove) {
        player.statusEffects.Remove(effect);
        combatUI.RemoveStatusEffect(player, effect);
    }
}
```

### 회피 체크
```csharp
bool CheckEvasion(Player player) {
    foreach (var effect in player.statusEffects) {
        if (effect.type == StatusEffectType.Evasion) {
            float chance = effect.value / 100f;
            if (Random.value < chance) {
                return true;
            }
        }
    }
    return false;
}
```

---

## 🏆 승패 처리

### 승리
```csharp
IEnumerator HandleVictory() {
    currentState = CombatState.Victory;

    // 1. 애니메이션
    PlayVictoryAnimation();

    yield return new WaitForSeconds(1.0f);

    // 2. 보상 계산
    int goldReward = CalculateGoldReward(currentEnemy);
    player.gold += goldReward;

    // 3. UI 표시
    combatUI.ShowVictoryScreen(goldReward);

    yield return new WaitForSeconds(2.0f);

    // 4. 보상 선택 화면으로
    RewardManager.Instance.ShowRewardSelection();
}
```

### 패배
```csharp
IEnumerator HandleDefeat() {
    currentState = CombatState.Defeat;

    // 1. 애니메이션
    PlayDefeatAnimation();

    yield return new WaitForSeconds(1.0f);

    // 2. UI 표시
    combatUI.ShowDefeatScreen();

    yield return new WaitForSeconds(2.0f);

    // 3. 런 종료
    RunManager.Instance.EndRun();
}
```

---

## 🎨 UI 연동

### CombatUI 클래스
```csharp
public class CombatUI : MonoBehaviour {
    // UI 요소
    public Slider playerHPBar;
    public Slider enemyHPBar;
    public Text playerDefenseText;
    public Text enemyDefenseText;
    public Image enemyIntentIcon;
    public Text enemyIntentText;
    public Transform statusEffectContainer;

    public void UpdatePlayerHP(Player player) {
        playerHPBar.value = player.currentHP / (float)player.maxHP;
        playerHPText.text = $"{player.currentHP}/{player.maxHP}";
    }

    public void UpdatePlayerDefense(Player player) {
        playerDefenseText.text = $"🛡️ {player.defense}/{player.maxDefense}";
    }

    public void ShowEnemyIntent(EnemyAction action) {
        // 적 의도 표시
        switch (action.type) {
            case EnemyActionType.Attack:
                enemyIntentIcon.sprite = attackIcon;
                enemyIntentText.text = $"공격 {action.value}";
                break;

            case EnemyActionType.HeavyAttack:
                enemyIntentIcon.sprite = heavyAttackIcon;
                enemyIntentText.text = $"⚠️ 강공격 {action.value}";
                enemyIntentIcon.color = Color.red;
                break;

            case EnemyActionType.Defend:
                enemyIntentIcon.sprite = defendIcon;
                enemyIntentText.text = $"방어 +{action.value}";
                break;
        }
    }

    public void ShowDamage(Character target, int damage) {
        // 데미지 팝업
        GameObject popup = Instantiate(damagePopupPrefab, target.transform.position, Quaternion.identity);
        TextMeshPro text = popup.GetComponent<TextMeshPro>();
        text.text = $"-{damage}";
        text.color = Color.red;

        StartCoroutine(FloatAndFade(popup));
    }
}
```

### 데미지 팝업
```csharp
IEnumerator FloatAndFade(GameObject popup) {
    Vector3 startPos = popup.transform.position;
    Vector3 endPos = startPos + Vector3.up * 2f;
    TextMeshPro text = popup.GetComponent<TextMeshPro>();

    float duration = 1.0f;
    float elapsed = 0;

    while (elapsed < duration) {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;

        popup.transform.position = Vector3.Lerp(startPos, endPos, t);
        text.alpha = 1 - t;

        yield return null;
    }

    Destroy(popup);
}
```

---

## 🔊 이벤트 시스템

### UnityEvent 사용
```csharp
public class Player {
    public UnityEvent<int> OnHPChanged;
    public UnityEvent<int> OnDefenseChanged;
    public UnityEvent<int> OnGoldChanged;
    public UnityEvent OnDeath;

    public void TakeDamage(int damage) {
        currentHP -= damage;
        OnHPChanged?.Invoke(currentHP);

        if (currentHP <= 0) {
            OnDeath?.Invoke();
        }
    }
}

// 구독
void Start() {
    player.OnHPChanged.AddListener(combatUI.UpdatePlayerHP);
    player.OnDeath.AddListener(HandlePlayerDeath);
}
```

---

## 📦 적 데이터 (ScriptableObject)

### EnemyData ScriptableObject
```csharp
[CreateAssetMenu(fileName = "Enemy", menuName = "Combat/Enemy")]
public class EnemyData : ScriptableObject {
    public string enemyName;
    public Sprite sprite;
    public int maxHP;
    public int baseAttack;

    public List<EnemyActionData> actions;

    public bool hasEnragePhase;
    public int enrageBonus;
}

[System.Serializable]
public class EnemyActionData {
    public EnemyActionType type;
    public int value;
    public float weight;
    public bool needsTelegraph;
    public string description;
}
```

### 적 생성
```csharp
Enemy CreateEnemy(EnemyData data) {
    Enemy enemy = new Enemy();
    enemy.enemyName = data.enemyName;
    enemy.maxHP = data.maxHP;
    enemy.currentHP = data.maxHP;
    enemy.defense = 0;

    enemy.hasEnragePhase = data.hasEnragePhase;
    enemy.enrageBonus = data.enrageBonus;

    // 행동 풀 생성
    enemy.actionPool = new List<EnemyAction>();
    foreach (var actionData in data.actions) {
        EnemyAction action = new EnemyAction();
        action.type = actionData.type;
        action.value = actionData.value;
        action.weight = actionData.weight;
        action.needsTelegraph = actionData.needsTelegraph;
        action.description = actionData.description;

        enemy.actionPool.Add(action);
    }

    return enemy;
}
```

---

## 🎯 최적화

### 오브젝트 풀링 (팝업)
```csharp
public class PopupPool : MonoBehaviour {
    public GameObject popupPrefab;
    private Queue<GameObject> pool = new Queue<GameObject>();

    public GameObject GetPopup() {
        if (pool.Count > 0) {
            GameObject popup = pool.Dequeue();
            popup.SetActive(true);
            return popup;
        } else {
            return Instantiate(popupPrefab);
        }
    }

    public void ReturnPopup(GameObject popup) {
        popup.SetActive(false);
        pool.Enqueue(popup);
    }
}
```

---

## 📦 MVP 구현 체크리스트

### Phase 1
- [ ] Player, Enemy 클래스
- [ ] CombatManager 기본 구조
- [ ] 턴 관리 (StartPlayerTurn, StartEnemyTurn)
- [ ] 기본 공격 (DealDamage, DealDamageToPlayer)
- [ ] HP 시스템
- [ ] 승패 판정

### Phase 2
- [ ] 방어력 시스템
- [ ] 적 AI (행동 선택)
- [ ] 행동 예고 UI
- [ ] 적 데이터 (ScriptableObject)
- [ ] 슬라임, 고블린, 오크 생성

### Phase 3
- [ ] 상태 효과 시스템
- [ ] DOT, 버프, 디버프
- [ ] Enrage 시스템
- [ ] 전투 UI 완성
- [ ] 애니메이션 & 이펙트

---

**작성일**: 2025-12-12
**버전**: 1.0
**담당**: 프로그래밍
