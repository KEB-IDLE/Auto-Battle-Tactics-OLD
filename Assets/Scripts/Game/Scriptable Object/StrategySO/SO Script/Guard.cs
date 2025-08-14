using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Guard", menuName = "AttackStrategy/Guard")]
public class Guard : AttackStrategyBaseSO
{
    [Header("Shield Raise Pose (Left Upper Arm)")]
    [SerializeField] private string leftUpperArmPath = "Bip001 L UpperArm";
    [SerializeField] private float raiseZ = -80f;       // 요구: z = -80°
    [SerializeField] private float raiseTime = 0.12f;   // 올리기
    [SerializeField] private float holdTime = 0.15f;    // 유지
    [SerializeField] private float downTime = 0.12f;    // 내리기

    [Header("Where VFX spawns from")]
    [SerializeField] private string shieldContainerPath = "Bip001 L Hand/L_shield_container";

    // SO는 공유 자산이라, 인스턴스별 캐시가 필요함
    private class Cache
    {
        public Transform arm;
        public Vector3 initialEuler;
        public Coroutine co;
        public int lastAppliedFrame;
    }
    private readonly Dictionary<int, Cache> _cache = new();

    // ─────────────────────────────────────────────────────────────────────────────
    // 공격: Melee_Normal 규칙 + 이펙트는 방패(firePoint)에서 1회 발신
    public override void Attack(AttackComponent context, IDamageable target)
    {
        if (context == null || context.teamProvider == null) return;

        // firePoint 없으면 방패 컨테이너를 찾아 세팅(1회)
        EnsureFirePoint(context);

        MonoBehaviour mb = target as MonoBehaviour;
        Transform aim = context.firePoint != null ? context.firePoint : context.transform;

        if (target != null && target.IsAlive() && mb != null && mb.gameObject != context.gameObject)
        {
            // 아군 제외
            var provider = mb.GetComponent<ITeamProvider>();
            bool isEnemy = provider == null || provider.Team != context.teamProvider.Team;

            // 실피해는 disengageRange 기준
            float sqrDist = (mb.transform.position - context.transform.position).sqrMagnitude;
            bool inDisengage = sqrDist <= (context.disengageRange * context.disengageRange);

            if (isEnemy && inDisengage)
            {
                context.lockedTarget = target;

                var core = mb.GetComponent<Core>();
                target.RequestDamage(core != null ? context.attackCoreDamage : context.attackDamage);
                (target as HealthComponent)?.ApplyImmediateDamage();
            }
        }

        // 방패 기준으로 이펙트 1회 발신
        context.EventSender(aim); // AttackComponent.EventSender() 지원. :contentReference[oaicite:0]{index=0}
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // AttackComponent에서 OnTakeDamageEffect가 호출될 때 불러 줄 공개 훅
    // (피해 반감은 AttackComponent가 처리하고, 여기서는 "방패 올리기" 연출만 수행)
    public void OnOwnerDamaged(AttackComponent context, Transform victim)
    {
        if (context == null || victim == null) return;
        if (victim != context.transform) return; // 내 유닛이 맞은 경우만

        var c = GetOrBuildCache(context);
        if (c.arm == null) return;

        // 같은 프레임 중복 방지(동 프레임 다중 히트 대비)
        if (c.lastAppliedFrame == Time.frameCount) return;
        c.lastAppliedFrame = Time.frameCount;

        if (c.co != null) context.StopAllCoroutines(); // 중첩 방지용으로 현재 코루틴만 끊고
        //c.co = context.ExecuteStrategyCoroutine(RaiseShieldRoutine(context, c)); // AttackComponent가 코루틴 실행 허용. :contentReference[oaicite:1]{index=1}
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Gizmo: 공격 진입(빨강), 실피해(연녹색) 반경
    public override void DrawGizmos(AttackComponent context, EntityData data)
    {
#if UNITY_EDITOR
        if (context == null) return;

        Vector3 origin = context.transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, data.attackRange);

        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.15f);
        Gizmos.DrawWireSphere(origin, data.disengageRange);
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 내부 유틸
    private void EnsureFirePoint(AttackComponent context)
    {
        if (context.firePoint != null) return;

        var t = FindDeep(context.transform, shieldContainerPath);
        if (t != null) context.firePoint = t;
    }

    private Cache GetOrBuildCache(AttackComponent context)
    {
        int id = context.GetInstanceID();
        if (_cache.TryGetValue(id, out var c) && c.arm != null) return c;

        c = new Cache();
        c.arm = FindDeep(context.transform, leftUpperArmPath);
        if (c.arm != null) c.initialEuler = c.arm.localEulerAngles;
        _cache[id] = c;

        // firePoint도 여기서 한 번 더 보장
        EnsureFirePoint(context);
        return c;
    }

    private IEnumerator RaiseShieldRoutine(AttackComponent context, Cache c)
    {
        // 올리기
        yield return LerpArmZ(c.arm, c.initialEuler.z, raiseZ, raiseTime);
        // 유지
        yield return new WaitForSeconds(holdTime);
        // 내리기
        yield return LerpArmZ(c.arm, raiseZ, c.initialEuler.z, downTime);
        c.co = null;
    }

    private IEnumerator LerpArmZ(Transform arm, float fromZ, float toZ, float time)
    {
        if (arm == null) yield break;
        if (time <= 0f) { SetArmZ(arm, toZ); yield break; }

        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            float z = Mathf.LerpAngle(fromZ, toZ, t / time);
            SetArmZ(arm, z);
            yield return null;
        }
        SetArmZ(arm, toZ);
    }

    private void SetArmZ(Transform arm, float z)
    {
        var e = arm.localEulerAngles;
        e.z = z;
        arm.localEulerAngles = e;
    }

    private Transform FindDeep(Transform root, string pathOrName)
    {
        // 슬래시 경로 우선
        if (pathOrName.Contains("/"))
            return root.Find(pathOrName);

        // 일반적인 깊이 탐색
        if (root.name == pathOrName) return root;
        foreach (Transform c in root)
        {
            var r = FindDeep(c, pathOrName);
            if (r != null) return r;
        }
        return null;
    }
}
