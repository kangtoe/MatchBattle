# 보드 시스템 기술 문서

**목적**: 경로 연결 방식 보드 구현 가이드
**대상**: 프로그래머

---

## 📐 데이터 구조

### Block 클래스
```csharp
public class Block {
    public BlockColor color;        // 블록 색상
    public BlockType type;          // 블록 타입
    public Vector2Int gridPos;      // 그리드 좌표
    public GameObject gameObject;   // Unity GameObject
    public SpriteRenderer sprite;   // 스프라이트

    // 효과 데이터
    public int attackValue;
    public int defenseValue;
    public int healValue;
    public int goldValue;
    public StatusEffect[] statusEffects;
}
```

### Enum 정의
```csharp
public enum BlockColor {
    Red,      // 붉은 (공격)
    Blue,     // 푸른 (방어)
    Yellow,   // 노란 (재물)
    Brown,    // 갈색 (중립)
    Purple    // 보라 (와일드카드)
}

public enum BlockType {
    // 붉은 블록
    Sword,
    Axe,
    Fire,

    // 푸른 블록
    Shield,
    Dodge,
    Counter,

    // 노란 블록
    Gold,
    Gem,
    Bonus,

    // 갈색 블록
    Trash,
    Potion,
    Buff,

    // 보라 블록
    Wildcard
}
```

### BoardManager 클래스
```csharp
public class BoardManager : MonoBehaviour {
    // 보드 데이터
    private Block[,] board = new Block[8, 8];

    // 현재 경로
    private List<Block> currentPath = new List<Block>();
    private BlockColor currentColor;

    // 블록 풀
    private BlockPool blockPool;

    // 참조
    public Transform boardParent;
    public LineRenderer pathLine;
}
```

---

## 🎮 입력 처리

### 드래그 시작
```csharp
void OnPointerDown(Vector2 screenPos) {
    Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
    Block block = GetBlockAtPosition(worldPos);

    if (block == null) return;

    // 경로 초기화
    currentPath.Clear();
    currentPath.Add(block);
    currentColor = block.color;

    // 시각적 피드백
    HighlightBlock(block, true);
    StartPathLine(block.gameObject.transform.position);
}
```

### 드래그 중
```csharp
void OnPointerDrag(Vector2 screenPos) {
    Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
    Block block = GetBlockAtPosition(worldPos);

    if (block == null) return;

    // 이미 경로에 있는 블록인가?
    int index = currentPath.IndexOf(block);
    if (index >= 0) {
        // 바로 이전 블록으로 돌아가는 경우 (Undo)
        if (index == currentPath.Count - 2) {
            Block removed = currentPath[currentPath.Count - 1];
            currentPath.RemoveAt(currentPath.Count - 1);
            HighlightBlock(removed, false);
            UpdatePathLine();
        }
        return;
    }

    // 마지막 블록과 인접한가?
    Block lastBlock = currentPath[currentPath.Count - 1];
    if (!IsAdjacent(lastBlock.gridPos, block.gridPos)) {
        return;
    }

    // 연결 가능한가?
    if (!CanConnect(lastBlock, block, currentColor)) {
        return;
    }

    // 경로에 추가
    currentPath.Add(block);
    HighlightBlock(block, true);
    UpdatePathLine();
}
```

### 드래그 종료
```csharp
void OnPointerUp() {
    // 최소 3개 이상인가?
    if (currentPath.Count < 3) {
        ClearPath();
        return;
    }

    // 효과 적용
    ApplyBlockEffects(currentPath);

    // 블록 제거
    StartCoroutine(RemoveBlocksSequence(currentPath));

    // 경로 초기화
    ClearPath();
}
```

---

## 🔗 연결 검증

### 인접성 체크
```csharp
bool IsAdjacent(Vector2Int pos1, Vector2Int pos2) {
    int dx = Mathf.Abs(pos1.x - pos2.x);
    int dy = Mathf.Abs(pos1.y - pos2.y);

    // 8방향 모두 허용 (상하좌우 + 대각선)
    // 단, 같은 위치는 제외
    return dx <= 1 && dy <= 1 && (dx + dy) > 0;
}
```

### 연결 가능 여부
```csharp
bool CanConnect(Block lastBlock, Block newBlock, BlockColor startColor) {
    // 와일드카드는 모든 색과 연결 가능
    if (lastBlock.color == BlockColor.Purple ||
        newBlock.color == BlockColor.Purple) {
        return true;
    }

    // 같은 색상만 연결 가능
    return newBlock.color == startColor;
}
```

### 블록 위치 찾기
```csharp
Block GetBlockAtPosition(Vector2 worldPos) {
    // 월드 좌표 → 그리드 좌표 변환
    int x = Mathf.RoundToInt(worldPos.x);
    int y = Mathf.RoundToInt(worldPos.y);

    // 범위 체크
    if (x < 0 || x >= 8 || y < 0 || y >= 8) {
        return null;
    }

    return board[x, y];
}
```

---

## ⚡ 효과 계산

### 블록 효과 적용
```csharp
void ApplyBlockEffects(List<Block> path) {
    // 효과 누적
    int totalAttack = 0;
    int totalDefense = 0;
    int totalGold = 0;
    int totalHeal = 0;
    List<StatusEffect> statusEffects = new List<StatusEffect>();

    // 각 블록의 효과 합산
    foreach (Block block in path) {
        totalAttack += block.attackValue;
        totalDefense += block.defenseValue;
        totalGold += block.goldValue;
        totalHeal += block.healValue;

        if (block.statusEffects != null) {
            statusEffects.AddRange(block.statusEffects);
        }
    }

    // 연쇄 보너스 적용
    float bonus = GetChainBonus(path.Count);
    totalAttack = Mathf.RoundToInt(totalAttack * bonus);
    totalDefense = Mathf.RoundToInt(totalDefense * bonus);

    // 효과 발동
    if (totalAttack > 0) {
        CombatManager.Instance.DealDamage(totalAttack);
        ShowDamagePopup(totalAttack);
    }

    if (totalDefense > 0) {
        CombatManager.Instance.AddDefense(totalDefense);
        ShowDefensePopup(totalDefense);
    }

    if (totalGold > 0) {
        GameManager.Instance.AddGold(totalGold);
        ShowGoldPopup(totalGold);
    }

    if (totalHeal > 0) {
        CombatManager.Instance.HealPlayer(totalHeal);
        ShowHealPopup(totalHeal);
    }

    foreach (var effect in statusEffects) {
        CombatManager.Instance.ApplyStatusEffect(effect);
    }
}
```

### 연쇄 보너스
```csharp
float GetChainBonus(int chainLength) {
    if (chainLength <= 3) return 1.0f;
    if (chainLength == 4) return 1.1f;
    if (chainLength == 5) return 1.25f;
    return 1.5f; // 6개 이상
}
```

---

## 🧱 블록 제거 & 보드 정리

### 블록 제거 시퀀스
```csharp
IEnumerator RemoveBlocksSequence(List<Block> path) {
    // 1. 블록 제거 애니메이션
    foreach (Block block in path) {
        PlayRemoveAnimation(block);
        board[block.gridPos.x, block.gridPos.y] = null;
    }

    yield return new WaitForSeconds(0.3f);

    // 2. 낙하 처리
    yield return StartCoroutine(DropBlocks());

    // 3. 빈 칸 채우기
    FillEmptySpaces();

    yield return new WaitForSeconds(0.2f);

    // 4. 턴 종료 알림
    CombatManager.Instance.EndPlayerTurn();
}
```

### 낙하 알고리즘
```csharp
IEnumerator DropBlocks() {
    bool moved = true;

    while (moved) {
        moved = false;

        // 아래에서 위로 스캔
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 7; y++) {
                // 현재 칸이 비어있고 위에 블록이 있는 경우
                if (board[x, y] == null && board[x, y + 1] != null) {
                    // 블록 이동
                    board[x, y] = board[x, y + 1];
                    board[x, y].gridPos = new Vector2Int(x, y);
                    board[x, y + 1] = null;

                    // 애니메이션
                    Vector3 targetPos = GridToWorld(x, y);
                    StartCoroutine(MoveBlock(board[x, y], targetPos, 0.1f));

                    moved = true;
                }
            }
        }

        if (moved) {
            yield return new WaitForSeconds(0.1f);
        }
    }
}
```

### 블록 이동 애니메이션
```csharp
IEnumerator MoveBlock(Block block, Vector3 target, float duration) {
    Vector3 start = block.gameObject.transform.position;
    float elapsed = 0;

    while (elapsed < duration) {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        block.gameObject.transform.position = Vector3.Lerp(start, target, t);
        yield return null;
    }

    block.gameObject.transform.position = target;
}
```

### 빈 칸 채우기
```csharp
void FillEmptySpaces() {
    for (int x = 0; x < 8; x++) {
        for (int y = 0; y < 8; y++) {
            if (board[x, y] == null) {
                // 블록 풀에서 생성
                BlockType type = blockPool.GetRandomBlockType();
                Block newBlock = CreateBlock(type, x, y);
                board[x, y] = newBlock;

                // 위에서 떨어지는 애니메이션
                Vector3 startPos = GridToWorld(x, 10);
                Vector3 targetPos = GridToWorld(x, y);
                newBlock.gameObject.transform.position = startPos;
                StartCoroutine(MoveBlock(newBlock, targetPos, 0.2f));
            }
        }
    }
}
```

---

## 🎲 블록 풀 시스템

### BlockPool 클래스
```csharp
public class BlockPool {
    private Dictionary<BlockType, float> weights;
    private System.Random rng;

    public BlockPool() {
        weights = new Dictionary<BlockType, float>();
        rng = new System.Random();
        InitializeDefaultPool();
    }

    void InitializeDefaultPool() {
        weights[BlockType.Sword] = 0.40f;
        weights[BlockType.Shield] = 0.40f;
        weights[BlockType.Gold] = 0.10f;
        weights[BlockType.Trash] = 0.08f;
        weights[BlockType.Potion] = 0.015f;
        weights[BlockType.Wildcard] = 0.005f;
    }

    public void AddBlockType(BlockType type, float weight) {
        if (weights.ContainsKey(type)) {
            weights[type] += weight;
        } else {
            weights[type] = weight;
        }

        NormalizeWeights();
    }

    void NormalizeWeights() {
        float sum = 0;
        foreach (var w in weights.Values) {
            sum += w;
        }

        List<BlockType> keys = new List<BlockType>(weights.Keys);
        foreach (var key in keys) {
            weights[key] /= sum;
        }
    }

    public BlockType GetRandomBlockType() {
        float rand = (float)rng.NextDouble();
        float cumulative = 0;

        foreach (var kvp in weights) {
            cumulative += kvp.Value;
            if (rand <= cumulative) {
                return kvp.Key;
            }
        }

        return BlockType.Sword; // Fallback
    }
}
```

---

## 🎬 보드 초기화

### 보드 생성
```csharp
void InitializeBoard() {
    // 1. 보드 배열 생성
    board = new Block[8, 8];

    // 2. 모든 칸에 블록 생성
    for (int x = 0; x < 8; x++) {
        for (int y = 0; y < 8; y++) {
            BlockType type = blockPool.GetRandomBlockType();
            Block block = CreateBlock(type, x, y);
            board[x, y] = block;
        }
    }

    // 3. 초기 긴 체인 방지
    while (HasLongInitialChain()) {
        ReshuffleProblematicBlocks();
    }
}

Block CreateBlock(BlockType type, int x, int y) {
    // 프리팹 로드
    GameObject prefab = Resources.Load<GameObject>($"Blocks/{type}");

    // 인스턴스 생성
    Vector3 worldPos = GridToWorld(x, y);
    GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, boardParent);

    // Block 데이터 설정
    Block block = new Block();
    block.type = type;
    block.color = GetBlockColor(type);
    block.gridPos = new Vector2Int(x, y);
    block.gameObject = obj;
    block.sprite = obj.GetComponent<SpriteRenderer>();

    // 효과 데이터 설정
    SetBlockEffect(block, type);

    return block;
}

Vector3 GridToWorld(int x, int y) {
    // 그리드 좌표 → 월드 좌표 변환
    // 예: 중앙 정렬, 각 칸 크기 1.0f
    return new Vector3(x - 3.5f, y - 3.5f, 0);
}
```

---

## 🎨 시각적 피드백

### 경로 라인 그리기
```csharp
void UpdatePathLine() {
    pathLine.positionCount = currentPath.Count;

    for (int i = 0; i < currentPath.Count; i++) {
        Vector3 pos = currentPath[i].gameObject.transform.position;
        pathLine.SetPosition(i, pos);
    }

    // 라인 색상 설정
    pathLine.startColor = GetColorForBlockColor(currentColor);
    pathLine.endColor = GetColorForBlockColor(currentColor);
}

Color GetColorForBlockColor(BlockColor blockColor) {
    switch (blockColor) {
        case BlockColor.Red: return new Color(1f, 0.3f, 0.3f);
        case BlockColor.Blue: return new Color(0.3f, 0.5f, 1f);
        case BlockColor.Yellow: return new Color(1f, 0.9f, 0.3f);
        case BlockColor.Brown: return new Color(0.6f, 0.4f, 0.2f);
        case BlockColor.Purple: return new Color(0.8f, 0.3f, 1f);
        default: return Color.white;
    }
}
```

### 블록 하이라이트
```csharp
void HighlightBlock(Block block, bool highlight) {
    if (highlight) {
        block.sprite.color = Color.white * 1.5f; // 밝게
        block.gameObject.transform.localScale = Vector3.one * 1.1f; // 확대
    } else {
        block.sprite.color = Color.white;
        block.gameObject.transform.localScale = Vector3.one;
    }
}
```

### 숫자 팝업
```csharp
void ShowDamagePopup(int damage) {
    GameObject popup = Instantiate(damagePopupPrefab);
    TextMeshPro text = popup.GetComponent<TextMeshPro>();
    text.text = damage.ToString();
    text.color = Color.red;

    // 위로 떠오르는 애니메이션
    StartCoroutine(FloatUpAndFade(popup, 1.0f));
}

IEnumerator FloatUpAndFade(GameObject obj, float duration) {
    Vector3 startPos = obj.transform.position;
    Vector3 endPos = startPos + Vector3.up * 2f;

    float elapsed = 0;
    TextMeshPro text = obj.GetComponent<TextMeshPro>();
    Color startColor = text.color;

    while (elapsed < duration) {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;

        obj.transform.position = Vector3.Lerp(startPos, endPos, t);
        text.color = new Color(startColor.r, startColor.g, startColor.b, 1 - t);

        yield return null;
    }

    Destroy(obj);
}
```

---

## 🐛 엣지 케이스 처리

### 빠른 드래그 시 블록 건너뛰기
```csharp
// 해결: 터치 위치 보간
void OnPointerDrag(Vector2 screenPos) {
    // 이전 프레임의 터치 위치
    if (lastDragPos != Vector2.zero) {
        // 두 점 사이를 보간
        float dist = Vector2.Distance(lastDragPos, screenPos);
        int steps = Mathf.CeilToInt(dist / 10f); // 10px마다 샘플링

        for (int i = 1; i <= steps; i++) {
            float t = i / (float)steps;
            Vector2 samplePos = Vector2.Lerp(lastDragPos, screenPos, t);
            TryAddBlockAtPosition(samplePos);
        }
    }

    lastDragPos = screenPos;
}
```

### 블록 풀 비율 제한
```csharp
void AddBlockType(BlockType type, float weight) {
    // 기존 로직...

    // 색상별 비율 제한
    EnforceColorLimits();
}

void EnforceColorLimits() {
    Dictionary<BlockColor, float> colorWeights = new Dictionary<BlockColor, float>();

    // 각 색상별 총 가중치 계산
    foreach (var kvp in weights) {
        BlockColor color = GetBlockColor(kvp.Key);
        if (!colorWeights.ContainsKey(color)) {
            colorWeights[color] = 0;
        }
        colorWeights[color] += kvp.Value;
    }

    // 제한 적용 (최소 10%, 최대 60%)
    // 구현 생략...
}
```

---

## 🎯 최적화

### 오브젝트 풀링
```csharp
public class BlockObjectPool {
    private Dictionary<BlockType, Queue<GameObject>> pools;

    public GameObject GetBlock(BlockType type) {
        if (!pools.ContainsKey(type)) {
            pools[type] = new Queue<GameObject>();
        }

        if (pools[type].Count > 0) {
            GameObject obj = pools[type].Dequeue();
            obj.SetActive(true);
            return obj;
        } else {
            GameObject prefab = Resources.Load<GameObject>($"Blocks/{type}");
            return Instantiate(prefab);
        }
    }

    public void ReturnBlock(BlockType type, GameObject obj) {
        obj.SetActive(false);
        pools[type].Enqueue(obj);
    }
}
```

### 배치 처리
```csharp
// 한 프레임에 너무 많은 작업 방지
IEnumerator ProcessBlocksInBatches(List<Block> blocks, int batchSize) {
    for (int i = 0; i < blocks.Count; i += batchSize) {
        int count = Mathf.Min(batchSize, blocks.Count - i);

        for (int j = 0; j < count; j++) {
            ProcessBlock(blocks[i + j]);
        }

        yield return null; // 다음 프레임
    }
}
```

---

## 📦 MVP 구현 체크리스트

### Phase 1
- [ ] Block, BlockColor, BlockType 정의
- [ ] BoardManager 기본 구조
- [ ] 8×8 그리드 생성
- [ ] 드래그 입력 감지 (OnPointerDown/Drag/Up)
- [ ] 경로 연결 검증 (IsAdjacent, CanConnect)
- [ ] LineRenderer로 경로 시각화

### Phase 2
- [ ] 블록 효과 계산 (ApplyBlockEffects)
- [ ] 블록 제거 애니메이션
- [ ] 낙하 알고리즘 (DropBlocks)
- [ ] 새 블록 생성 (FillEmptySpaces)
- [ ] 연쇄 보너스

### Phase 3
- [ ] BlockPool 시스템
- [ ] 블록 추가 기능
- [ ] 와일드카드 구현
- [ ] 숫자 팝업
- [ ] 애니메이션 polish

---

**작성일**: 2025-12-12
**버전**: 1.0
**담당**: 프로그래밍
