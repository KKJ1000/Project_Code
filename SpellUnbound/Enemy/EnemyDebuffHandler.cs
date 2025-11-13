using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDebuffHandler : MonoBehaviour
{
    private BaseMonster m_Monster;
    private NavMeshAgent m_navMeshAgent;
    private Animator m_animator;
    private int m_fireStack = 0;
    private float m_fireImmuneTimer = 0f;

    private int m_iceStack = 0;
    private float m_iceImmuneTimer = 0f;

    private bool isFireImmune => m_fireImmuneTimer > 0f;
    private bool isIceImmune => m_iceImmuneTimer > 0f;

    private EnemyState m_previousState;

    void Start()
    {
        m_Monster = GetComponent<BaseMonster>();
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (m_fireImmuneTimer > 0f)
        {
            m_fireImmuneTimer -= Time.deltaTime;
        }

        if (m_iceImmuneTimer > 0f)
        {
            m_iceImmuneTimer -= Time.deltaTime;
        }
    }

    public void ApplyFireDebuff(float damage)
    {
        if (isFireImmune)
        {
            return;
        }

        m_fireStack++;
        Debug.Log("화염 스택 +1");
        if (m_fireStack >= 5)
        {
            m_fireStack = 0;
            m_fireImmuneTimer = 7f;
            StartCoroutine(ApplyFireDotDamage(damage * 0.2f));
        }
    }

    public void ApplyIceDebuff()
    {
        if (isIceImmune)
        {
            return;
        }

        m_iceStack++;
        if (m_iceStack >= 5)
        {
            m_iceStack = 0;
            m_iceImmuneTimer = 7f;
            StartCoroutine(FreezeEnemy(2f));
        }
    }

    private IEnumerator ApplyFireDotDamage(float damage)
    {
        float duration = 3f;
        float tickInterval = 0.5f; 
        float tickDamage = damage / (duration / tickInterval); 
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (m_Monster == null || m_Monster.currentEnemyState == EnemyState.DEAD)
            {
                yield break;
            }

            Debug.Log($"{tickDamage}");
            GetComponent<BaseMonster>().TakeDamage(SkillType.Fire, tickDamage);
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }
    }

    private IEnumerator FreezeEnemy(float duration)
    {
        var CrystalElementalState = m_Monster.GetComponent<CrystalElementalStateMachine>();

        if (CrystalElementalState != null)
        {
            CrystalElementalState.TransitionToState(CrystalElementalState.GetIceState);
        }

        m_previousState = m_Monster.currentEnemyState;
        m_Monster.currentEnemyState = EnemyState.ICE;
        m_navMeshAgent.isStopped = true;
        m_animator.speed = 0f;
        yield return new WaitForSeconds(duration);
        if (m_Monster.currentEnemyState == EnemyState.DEAD)
        {
            yield break;
        }

        m_Monster.currentEnemyState = EnemyState.IDLE;
        m_animator.speed = 1f;
        m_navMeshAgent.isStopped = false;
        m_Monster.currentEnemyState = m_previousState;
    }
}
