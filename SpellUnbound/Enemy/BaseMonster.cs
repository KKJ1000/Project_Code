using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    IDLE,
    PATROL,
    CHASE,
    RETURN,
    ATTACK_MELEE,
    ATTACK_MAGIC,
    STAGGER,
    ICE,
    DEAD,
}

public abstract class BaseMonster : MonoBehaviour
{
    protected NavMeshAgent agent;
    protected Animator enemyAnimator;
    protected Transform player;
    protected MonsterHealthUI monsterHealthUI;
    protected MonsterDamageUI monsterDamageUI;
    protected Transform defenseCrystal;
    private EnemyDebuffHandler enemyDebuffHandler;

    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private Collider[] mainColliders;

    [Header("몬스터 기본 설정")]
    [SerializeField, Tooltip("이동 속도")] protected float moveSpeed;
    [Tooltip("체력")] public float healthPoint;
    [SerializeField, Tooltip("레그돌 물리력")] protected float ragdollForce = 200f;

    [Header("디졸브 관련 설정")]
    [SerializeField] protected float dissolveDuration = 2f;
    [SerializeField] protected SkinnedMeshRenderer[] skinnedMeshRenderers;
    [SerializeField] protected Material dissolveMaterial;
    [SerializeField] protected Material outlineMaterial;
    [SerializeField] protected Material defaultMaterial;

    [Header("현재 몬스터 상태")]
    public EnemyState currentEnemyState;
    protected bool _isDead = false;

    [Header("스폰 지역")]
    [SerializeField] protected Transform spawnArea;
    [SerializeField] private GameObject ragdollRoot;

    [Header("보스가 소환한 몬스터")]
    protected bool isBossSummoned = false;

    [Header("Drop Item Setting")]
    [SerializeField, Tooltip("재화 아이템 프리팹")] private GameObject goldPrefab;
    [SerializeField, Tooltip("아이템 하나의 최대값")] private int maxGoldItem = 10;
    [SerializeField, Tooltip("드롭되는 범위")] private float dropRadius = 1.5f;

    [Header("몬스터 스폰 이펙트")]
    [SerializeField] protected GameObject spawnEffectPrefab;
    [SerializeField] protected float spawnEffectDuration = 1.5f;

    public bool IsBoss { get; protected set; } = false;
    public bool IsDead => _isDead;
    protected bool isSpawning = false;
    protected bool isLivingArmor = false;
    private bool isTutorialMonster = false;            

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        monsterHealthUI = GetComponent<MonsterHealthUI>();
        enemyAnimator = GetComponent<Animator>();
        enemyDebuffHandler = GetComponent<EnemyDebuffHandler>();
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        mainColliders = GetComponents<Collider>();

        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }

        if (!IsBoss)
        {
            isSpawning = true;
            StartCoroutine(StartAppearEffect());
            transform.LookAt(player.position);
        }

        if (agent != null)
        {
            agent.speed = moveSpeed;
        }

        if (!IsBoss)
        {
            monsterDamageUI = GetComponent<MonsterDamageUI>();

            if (spawnEffectPrefab != null)
            {
                GameObject effect = Instantiate(spawnEffectPrefab, transform.position, Quaternion.identity);
                effect.transform.SetParent(transform);
                Destroy(effect, spawnEffectDuration);
            }
        }

        SetRagdollActive(false);

        if (ragdollRoot != null)
        {
            ragdollRoot.SetActive(false);
        }
    }

    public virtual void TakeDamage(SkillType type, float damage, bool isTripleChain = false)
    {
        if (_isDead || isSpawning)
        {
            return;
        }

        healthPoint -= damage;

        if (enemyDebuffHandler != null && type == SkillType.Fire)
        {
            enemyDebuffHandler.ApplyFireDebuff(damage);
        }

        if (enemyDebuffHandler != null && type == SkillType.Ice)
        {
            enemyDebuffHandler.ApplyIceDebuff();
        }

        if (monsterHealthUI != null)
        {
            monsterHealthUI.EnableHpUI();
            monsterHealthUI.UpdateHpUI();
        }

        if (monsterDamageUI != null)
        {
            monsterDamageUI.EnableDamageText(damage);
        }

        Debug.Log($"몬스터 HP : {healthPoint}");

        if (type == SkillType.Ice && isTripleChain)
        {
            ApplyIceTripleChainSplash(damage * 0.1f);
        }

        if (healthPoint <= 0)
        {
            healthPoint = 0;
            Die(type, isTripleChain);
        }
    }

    protected virtual void Die(SkillType type, bool isTripleChain)
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;
        currentEnemyState = EnemyState.DEAD;
        // enemyAnimator.SetTrigger("Die");
        if (agent != null)
        {
            agent.isStopped = true;
        }

        if (monsterHealthUI != null)
        {
            monsterHealthUI.DisableHpUI();
        }

        if (monsterDamageUI != null)
        {
            monsterDamageUI.DisableDamageText();
        }

        if (!isBossSummoned && !isTutorialMonster)
        {
            int totalGold = Random.Range(50, 101); // 일반 몬스터 획득 재화 5 ~ 10 -> 테스트용으로 범위 50 ~ 100으로 확대
            int goldCount = totalGold / maxGoldItem; // 재화 10개당 재화 아이템 1개 생성
            int remainder = totalGold % maxGoldItem;

            for (int i = 0; i < goldCount; i++)
            {
                SpawnGold(maxGoldItem);
            }

            if (remainder > 0)
            {
                SpawnGold(remainder);
            }
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnMonsterDead(gameObject);
        }

        StopAllCoroutines();

        if (ragdollRoot != null)
        {
            ragdollRoot.SetActive(true);
        }

        if (!isLivingArmor)
        {
            SetRagdollActive(true);
            AddRagdollForce();
        }

        OnKilledBy(type, isTripleChain);

        StartCoroutine(DissolveAfterDelay(isLivingArmor ? 0.1f : 1.5f));
    }

    private void SpawnGold(int value)
    {
        Vector3 randomOffset = new Vector3(Random.Range(-dropRadius, dropRadius), 0.5f, Random.Range(-dropRadius, dropRadius));
        Vector3 spawnPosition = transform.position + randomOffset;
        if (goldPrefab != null)
        {
            GameObject gold = Instantiate(goldPrefab, spawnPosition, Quaternion.identity);
            gold.GetComponent<GoldValue>().SetGoldValue(value);
            WaveManager.Instance.RegisterGold(gold); // 생성된 골드를 현재 드랍된 골드 리스트에 추가
        }
    }

    public void ForceDie()
    {
        Die(SkillType.Default, false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerStatManager>().TakeDamage(10);
        }
    }

    IEnumerator StartAppearEffect()
    {
        foreach (var renderer in skinnedMeshRenderers)
        {
            Material[] newMaterials = new Material[renderer.materials.Length];

            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = new Material(dissolveMaterial);
                newMaterials[i].SetFloat("_Amount", 1f);
            }
            renderer.materials = newMaterials;
        }

        float elapsedTime = 0f;

        while (elapsedTime < spawnEffectDuration)
        {
            float time = elapsedTime / spawnEffectDuration;
            float dissolveValue = Mathf.Lerp(1f, -1f, time);

            foreach (var renderer in skinnedMeshRenderers)
            {
                foreach (var mat in renderer.materials)
                {
                    mat.SetFloat("_Amount", dissolveValue);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (var renderer in skinnedMeshRenderers)
        {
            Material[] newMaterials = new Material[2];
            newMaterials[0] = new Material(defaultMaterial);
            newMaterials[1] = new Material(outlineMaterial);

            renderer.materials = newMaterials;
        }

        isSpawning = false;
    }

    private void SetRagdollActive(bool isActive)
    {
        foreach (var rb in ragdollRigidbodies)
        {
            rb.isKinematic = !isActive;
            rb.detectCollisions = isActive;

            // rb.gameObject.SetActive(isActive);
        }

        foreach (var col in ragdollColliders)
        {
            bool isMain = false;

            foreach (Collider mainCol in mainColliders)
            {
                if (col == mainCol)
                {
                    isMain = true;
                    break;
                }
            }

            if (!isMain)
            {
                col.enabled = isActive;
            }
        }

        if (mainColliders != null)
        {
            foreach (Collider col in mainColliders)
            {
                col.enabled = !isActive;
            }
        }

        // if (agent != null)
        // {
        //     agent.enabled = !isActive;
        // }

        if (enemyAnimator != null)
        {
            enemyAnimator.enabled = !isActive;
        }

        var joints = GetComponentsInChildren<Joint>();
        foreach (var joint in joints)
        {
            joint.enableCollision = isActive;
            joint.enablePreprocessing = isActive;
        }
    }

    private void AddRagdollForce()
    {
        Vector3 forceDirection = (transform.position - player.position).normalized;

        foreach (var rb in ragdollRigidbodies)
        {
            rb.AddForce(forceDirection * ragdollForce, ForceMode.Impulse);
        }
    }

    protected IEnumerator DissolveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var col in ragdollColliders)
        {
            col.enabled = false;
        }

        foreach (var col in mainColliders)
        {
            col.enabled = false;
        }

        foreach (var renderer in skinnedMeshRenderers)
        {
            Material[] newMaterials = new Material[renderer.materials.Length];

            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = new Material(dissolveMaterial);
            }
            renderer.materials = newMaterials;
        }

        float elapsedTime = 0f;

        while (elapsedTime < dissolveDuration)
        {
            float time = elapsedTime / dissolveDuration;
            foreach (var renderer in skinnedMeshRenderers)
            {
                foreach (var mat in renderer.materials)
                {
                    mat.SetFloat("_Amount", time);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    #region 3체인
    /// <summary>
    /// 화염 3체인 스킬로 처치 시 주변 몬스터에게 피해를 입히는 메서드
    /// </summary>
    /// <param name="type">스킬 타입</param>
    /// <param name="isTripleChain">3체인 여부</param>
    private void OnKilledBy(SkillType type, bool isTripleChain)
    {
        if (type != SkillType.Fire || !isTripleChain)
        {
            return;
        }

        // 요청시 전역변수로 변경
        float explosionRadius = 5f;
        float explosionDamage = monsterHealthUI.maxHealth * 0.3f;

        // 나중에 레이어 마스크 전역으로
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("Monster"));

        foreach (var hit in hits)
        {
            var monster = hit.GetComponent<BaseMonster>();

            if (monster != null && monster != this && !monster._isDead && monster.gameObject.activeInHierarchy)
            {
                monster.TakeDamage(type, explosionDamage);
            }
        }
    }

    /// <summary>
    /// 얼음 3체인 스킬로 처치 시 주변 몬스터에게 피해를 입히는 메서드
    /// </summary>
    /// <param name="splashDamage">스플래시 데미지</param>
    private void ApplyIceTripleChainSplash(float splashDamage)
    {
        // OnKilledBy 메서드와 마찬가지로 레이어마스크 전역으로
        Collider[] hits = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Monster"));

        foreach (var hit in hits)
        {
            var monster = hit.GetComponent<BaseMonster>();

            if (monster != null && monster != this && !monster._isDead && monster.gameObject.activeInHierarchy)
            {
                monster.TakeDamage(SkillType.Ice, splashDamage);
            }
        }
    }
    #endregion

    public void SetDefenseCrystal(Transform crystal)
    {
        defenseCrystal = crystal;
    }

    /// <summary>
    /// 보스가 소환한 일반 몬스터일 때 호출하는 함수
    /// </summary>
    public void SetSummonedByBoss()
    {
        isBossSummoned = true;
    }

    public void SetTutorialMonster()
    {
        isTutorialMonster = true;
    }

    protected virtual void Attack()
    {

    }

    protected virtual void Move()
    {

    }

    public virtual void Stagger(float duration)
    {

    }
}
