using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossProjectileType
{
    Mush_1,
    Mush_2
}

public class BossProjectile : MonoBehaviour
{
    private BossProjectileType m_projectileType;
    private Transform m_target;
    private float m_damage;
    private float m_speed;
    private Vector3 m_direction;
    private GameObject m_MushHitEffect;

    private GameObject m_player;
    private PlayerMovement m_playerMovement;
    private PlayerStateMachine m_playerStateMachine;


    public void Init(BossProjectileType type, Transform targetPos, float damage, float speed, GameObject hitEffect = null)
    {
        m_projectileType = type;
        m_target = targetPos;
        m_damage = damage;
        m_speed = speed;
        m_MushHitEffect = hitEffect;

        m_player = targetPos.gameObject;

        if (m_player != null)
        {
            m_playerMovement = m_player.GetComponent<PlayerMovement>();
            m_playerStateMachine = m_player.GetComponent<PlayerStateMachine>();
        }

        m_direction = ((m_player.transform.position + Vector3.up) - transform.position).normalized;

        Invoke("DestroyProjectile", 8f);
    }

    private void Update()
    {
        if (m_projectileType == BossProjectileType.Mush_1 || m_projectileType == BossProjectileType.Mush_2)
        {
            transform.position += m_direction * m_speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_projectileType == BossProjectileType.Mush_1)
        {
            if (other.CompareTag("Player"))
            {
                PlayerStatManager.Instance.TakeDamage((int)m_damage);
                // TODO : 포자 공격 (넉백 넣을까 말까)
            }
            else if (other.CompareTag("Floor") || other.CompareTag("Wall"))
            {
                Destroy(gameObject);
            }
        }
        else if (m_projectileType == BossProjectileType.Mush_2)
        {
            if (other.CompareTag("Player"))
            {
                PlayerStatManager.Instance.TakeDamage((int)m_damage);
                GameObject hitEffect = Instantiate(m_MushHitEffect, transform.position, Quaternion.identity);
                Destroy(hitEffect, 1f);
                Destroy(gameObject);
            }
            else if (other.CompareTag("Floor") || other.CompareTag("Wall"))
            {
                GameObject hitEffect = Instantiate(m_MushHitEffect, transform.position, Quaternion.identity);
                Destroy(hitEffect, 1f);
                Destroy(gameObject);
            }
        }
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
