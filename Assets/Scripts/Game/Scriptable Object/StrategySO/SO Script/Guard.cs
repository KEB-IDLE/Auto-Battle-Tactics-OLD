using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Guard", menuName = "AttackStrategy/Guard")]
public class Guard : AttackStrategyBaseSO
{
    [Header("Shield Raise Pose (Left Upper Arm)")]
    [SerializeField] private string leftUpperArmPath = "Bip001 L UpperArm";
    [SerializeField] private float raiseZ = -80f;       // �䱸: z = -80��
    [SerializeField] private float raiseTime = 0.12f;   // �ø���
    [SerializeField] private float holdTime = 0.15f;    // ����
    [SerializeField] private float downTime = 0.12f;    // ������

    [Header("Where VFX spawns from")]
    [SerializeField] private string shieldContainerPath = "Bip001 L Hand/L_shield_container";

    // SO�� ���� �ڻ��̶�, �ν��Ͻ��� ĳ�ð� �ʿ���
    private class Cache
    {
        public Transform arm;
        public Vector3 initialEuler;
        public Coroutine co;
        public int lastAppliedFrame;
    }
    private readonly Dictionary<int, Cache> _cache = new();

    // ����������������������������������������������������������������������������������������������������������������������������������������������������������
    // ����: Melee_Normal ��Ģ + ����Ʈ�� ����(firePoint)���� 1ȸ �߽�
    public override void Attack(AttackComponent context, IDamageable target)
    {
        if (context == null || context.teamProvider == null) return;

        // firePoint ������ ���� �����̳ʸ� ã�� ����(1ȸ)
        EnsureFirePoint(context);

        MonoBehaviour mb = target as MonoBehaviour;
        Transform aim = context.firePoint != null ? context.firePoint : context.transform;

        if (target != null && target.IsAlive() && mb != null && mb.gameObject != context.gameObject)
        {
            // �Ʊ� ����
            var provider = mb.GetComponent<ITeamProvider>();
            bool isEnemy = provider == null || provider.Team != context.teamProvider.Team;

            // �����ش� disengageRange ����
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

        // ���� �������� ����Ʈ 1ȸ �߽�
        context.EventSender(aim); // AttackComponent.EventSender() ����. :contentReference[oaicite:0]{index=0}
    }

    // ����������������������������������������������������������������������������������������������������������������������������������������������������������
    // AttackComponent���� OnTakeDamageEffect�� ȣ��� �� �ҷ� �� ���� ��
    // (���� �ݰ��� AttackComponent�� ó���ϰ�, ���⼭�� "���� �ø���" ���⸸ ����)
    public void OnOwnerDamaged(AttackComponent context, Transform victim)
    {
        if (context == null || victim == null) return;
        if (victim != context.transform) return; // �� ������ ���� ��츸

        var c = GetOrBuildCache(context);
        if (c.arm == null) return;

        // ���� ������ �ߺ� ����(�� ������ ���� ��Ʈ ���)
        if (c.lastAppliedFrame == Time.frameCount) return;
        c.lastAppliedFrame = Time.frameCount;

        if (c.co != null) context.StopAllCoroutines(); // ��ø ���������� ���� �ڷ�ƾ�� ����
        //c.co = context.ExecuteStrategyCoroutine(RaiseShieldRoutine(context, c)); // AttackComponent�� �ڷ�ƾ ���� ���. :contentReference[oaicite:1]{index=1}
    }

    // ����������������������������������������������������������������������������������������������������������������������������������������������������������
    // Gizmo: ���� ����(����), ������(�����) �ݰ�
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

    // ����������������������������������������������������������������������������������������������������������������������������������������������������������
    // ���� ��ƿ
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

        // firePoint�� ���⼭ �� �� �� ����
        EnsureFirePoint(context);
        return c;
    }

    private IEnumerator RaiseShieldRoutine(AttackComponent context, Cache c)
    {
        // �ø���
        yield return LerpArmZ(c.arm, c.initialEuler.z, raiseZ, raiseTime);
        // ����
        yield return new WaitForSeconds(holdTime);
        // ������
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
        // ������ ��� �켱
        if (pathOrName.Contains("/"))
            return root.Find(pathOrName);

        // �Ϲ����� ���� Ž��
        if (root.name == pathOrName) return root;
        foreach (Transform c in root)
        {
            var r = FindDeep(c, pathOrName);
            if (r != null) return r;
        }
        return null;
    }
}
