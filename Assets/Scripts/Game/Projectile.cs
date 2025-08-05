using UnityEngine;
using System;
using System.Runtime.CompilerServices;


[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private Rigidbody _rb;
    private string poolName; // � Ǯ�� ���� ������Ʈ����
    private GameObject flightEffect;

    [Header("Projectile Scriptable Object�� �Ҵ��ϼ���.")]
    [SerializeField] private ProjectileData data;

    private Team team;
    private float timer;
    private float lifeTime;
    private float damage;
    private float coreDamage;
    private Transform target;
    private float speedWeight;
    private float verticalSpeedWeight;
    private float disengageRange = 100f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
    }

    // ObjectPoolManager�� ���� �ʱ�ȭ
    public void Initialize(Entity owner, float damage, float coreDamage, Transform target, string poolName, float disengageRange)
    {
        this.damage = damage;
        this.target = target;
        this.coreDamage = coreDamage;
        this.poolName = poolName;
        this.disengageRange = disengageRange;
        timer = 0f;
        lifeTime = 5f;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;


        SetTeam(owner);
        AttachFlightEffect();

        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            // ����ġ(Weight)�� ���� �ڵ� ����
            float autoSpeed = Mathf.Max(distance * data.speedWeight, 4f);
            float autoVerticalSpeed = Math.Max(distance * data.verticalSpeedWeight, 4f);

            Vector3 direction = (target.position - transform.position).normalized;
            _rb.linearVelocity = direction * autoSpeed + Vector3.up * autoVerticalSpeed;
        }
    }

    private void SetTeam(Entity owner)
    {
        var teamComponent = owner.GetComponent<TeamComponent>();
        if (teamComponent != null)
            this.team = teamComponent.Team;
        else
            Debug.LogWarning("[Projectile] Owner�� TeamComponent�� �����ϴ�!");
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (target == null || timer > lifeTime)
        {
            ReturnFlightEffect();
            ReturnToPool();
            return;
        }
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > disengageRange)
        {
            ReturnFlightEffect();
            ReturnToPool();
            return;
        }
        Move();
        CheckHit();
    }

    private void Move()
    {

        if (_rb == null || target == null) return;
        Vector3 toTarget = (target.position - transform.position).normalized;
        float currentSpeed = _rb.linearVelocity.magnitude;

        // ��¦ ���� (0.08~0.18 ���̿��� ���� ��õ)
        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, toTarget * currentSpeed, 0.12f);
        if (_rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.forward = _rb.linearVelocity.normalized;

    }

   protected virtual void CheckHit()
{
    if (target != null)
    {
        float dist = Vector3.Distance(transform.position, target.position);

            if (dist <= data.detectionRadius)
            {
                Debug.Log($"[Projectile] 충돌 감지됨 → 대상: {target.name}"); // ✅ 추가

                GameObject targetGO = target.gameObject;
                var hp = targetGO.GetComponent<HealthComponent>();
                if (hp == null)
                {
                    Debug.LogWarning($"❌ [Projectile] HealthComponent 없음 → {targetGO.name}");
                }
                else
                {
                    float dmg = (coreDamage != 0 ? coreDamage : damage);
                    Debug.Log($"💥 [Projectile] {targetGO.name}에게 {dmg} 데미지 적용"); // ✅ 핵심 로그
                    hp.RequestDamage(dmg);
                    hp.ApplyImmediateDamage();
                }

                ReturnFlightEffect();
                ReturnToPool();
            return;
        }
    }
}

    /// <summary>
    /// Ǯ �̸��� ���� ��������
    /// </summary>
    public void SetPoolName(string poolName) => this.poolName = poolName;

    private void ReturnToPool()
    {
        ReturnFlightEffect();

        if (!string.IsNullOrEmpty(poolName))
        {
            ObjectPoolManager.Instance.Return(poolName, this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// FlightEffect�� EffectPool���� �޾ƿ� ����
    /// </summary>
    private void AttachFlightEffect()
    {
        ReturnFlightEffect();

        if (data.FlightEffectPrefab == null) return;

        var pool = ObjectPoolManager.Instance.GetPool(data.FlightEffectPrefab.name);
        if (pool != null)
        {
            flightEffect = pool.Get(transform.position, Quaternion.identity);
            flightEffect.transform.SetParent(transform, false);
            flightEffect.transform.localPosition = Vector3.zero;
        }
        else
        {
            flightEffect = Instantiate(data.FlightEffectPrefab, transform);
            flightEffect.transform.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// ���� FlightEffect�� �ݵ�� Ǯ�� ��ȯ/����
    /// </summary>
    private void ReturnFlightEffect()
    {
        if (flightEffect == null) return;
        flightEffect.transform.SetParent(null);

        var pool = ObjectPoolManager.Instance.GetPool(data.FlightEffectPrefab.name);
        if (pool != null)
        {
            if (pool is MonoBehaviour poolObj)
                flightEffect.transform.SetParent(poolObj.transform, false);
            pool.Return(flightEffect);
        }
        else
        {
            Destroy(flightEffect);
        }
        flightEffect = null;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRadius);

        Gizmos.color = Color.brown;
        Gizmos.DrawWireSphere(transform.position, data.explosionRadius);
    }
#endif
}
