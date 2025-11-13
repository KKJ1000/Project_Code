using System.Collections;
using UnityEngine;

public enum ProjectileType
{
    AttackMush_00,
    AttackMush_01,
    RabbitAttack_00,
    RabbitAttack_01,
}

public class Projectile : MonoBehaviour
{
    [Header("발사체 타입")]
    [SerializeField] private ProjectileType projectileType;

    [Header("발사체 속성")]
    [SerializeField, Tooltip("발사체 속도")] private float speed;
    [SerializeField, Tooltip("발사체 데미지")] private float damage;
    [SerializeField, Tooltip("발사체 최대 거리")] private float maxDistance;

    [Header("RabbitAttack_01_Effect")]
    [SerializeField, Tooltip("폭발 이펙트 지속 시간")] private float destroyEffectTime;

    private Vector3 startPosition;
    private GameObject exploreRangePrefab;
    private GameObject hitEffectPrefab;

    private GameObject player;
    private PlayerMovement m_playerMovement;
    private PlayerStateMachine playerStateMachine;
    private CorruptionGauge playerCorruptionGauge;

    private float attackMush_01_time = 5f;
    private bool isCancelInvoke = false;
    private bool isJamsick = false;

    public void Init(Vector3 projectileStartPoint, GameObject exploreRangePrefab = null, GameObject hitEffectPrefab = null)
    {
        this.startPosition = projectileStartPoint;
        this.exploreRangePrefab = exploreRangePrefab;
        this.hitEffectPrefab = hitEffectPrefab;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            m_playerMovement = player.GetComponent<PlayerMovement>();
            playerStateMachine = player.GetComponent<PlayerStateMachine>();
            playerCorruptionGauge = player.GetComponent<CorruptionGauge>();
        }
    }

    void Update()
    {
        if (projectileType == ProjectileType.AttackMush_00)
        {
            transform.position += transform.forward * speed * Time.deltaTime;

            if (Vector3.Distance(transform.position, startPosition) > maxDistance)
            {
                Destroy(gameObject);
            }
        }
        else if (projectileType == ProjectileType.AttackMush_01)
        {
            Vector3 direction = (player.transform.position + Vector3.up * 1.2f) - transform.position;
            transform.position += direction * speed * Time.deltaTime;
            Invoke("DestroyProjectile", 5f);
        }
        else if (projectileType == ProjectileType.RabbitAttack_01)
        {
            Vector3 direction = transform.forward;
            transform.position += direction * speed * Time.deltaTime;

            if (Vector3.Distance(transform.position, startPosition) > maxDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (projectileType == ProjectileType.AttackMush_00 || projectileType == ProjectileType.RabbitAttack_01)
        {
            if (other.CompareTag("Player"))
            {
                playerCorruptionGauge.Increase(2f);
                PlayerStatManager.Instance.TakeDamage((int)damage);

                Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
                m_playerMovement?.ApplyKnockback(knockbackDirection, 5f);

                GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(hitEffect, 0.5f);
                Destroy(gameObject);
            }
            else if (other.CompareTag("Crystal"))
            {
                DefenseCrystal crystal = other.GetComponent<DefenseCrystal>();
                if (crystal != null)
                {
                    crystal.TakeDamage((int)damage);

                    GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                    Destroy(hitEffect, 0.5f);
                    Destroy(gameObject);
                }
            }
            else if (other.CompareTag("Floor"))
            {
                GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(hitEffect, 0.5f);
                Destroy(gameObject);
            }
        }
        else if (projectileType == ProjectileType.AttackMush_01)
        {
            if (other.CompareTag("Player"))
            {
                if (!isCancelInvoke)
                {
                    CancelInvoke();
                    isCancelInvoke = true;
                }

                if (!isJamsick)
                {
                    isJamsick = true;
                    StartCoroutine(ApplyCorrputionTime(playerCorruptionGauge));
                }

                transform.position = other.transform.position;
                transform.SetParent(other.transform);
                GetComponent<SphereCollider>().enabled = false;
            }
            else if (other.CompareTag("Floor"))
            {
                DestroyProjectile();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (projectileType == ProjectileType.RabbitAttack_00)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerStatManager.Instance.TakeDamage(3);
                playerCorruptionGauge.Increase(5f);
                Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
                m_playerMovement.ApplyKnockback(knockbackDirection, 5f);
                Destroy(gameObject);
            }
            else if (collision.gameObject.CompareTag("Floor"))
            {
                Rigidbody rigid = GetComponent<Rigidbody>();
                rigid.isKinematic = true;
                rigid.useGravity = false;

                GameObject exploreRangeObject = Instantiate(exploreRangePrefab, transform.position, Quaternion.identity);
                exploreRangeObject.transform.localScale = new Vector3(3f, 0.01f, 3f);
                Destroy(exploreRangeObject, 2f);
                Invoke("Explode", 2f);
            }
        }
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + Vector3.up, 1.5f);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                PlayerStatManager.Instance.TakeDamage((int)damage + 12);
                Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
                m_playerMovement.ApplyKnockback(knockbackDirection, 5f);
            }
        }
        GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        Destroy(hitEffect, destroyEffectTime);
        Destroy(gameObject, 0.5f);
    }

    IEnumerator ApplyCorrputionTime(CorruptionGauge corruptionGauge)
    {
        float elapsedTime = 0f;
        while (elapsedTime < attackMush_01_time)
        {
            corruptionGauge.Increase(2f);
            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }
        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
