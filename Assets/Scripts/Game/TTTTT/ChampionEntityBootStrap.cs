// ChampionEntityBootstrap.cs
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class ChampionEntityBootstrap : MonoBehaviour
{
    //[Header("Design (from shop/controller)")]
    //public Champion champion;      // ��Ʈ�ѷ��� �����ϰų� �����տ� �̸� �ھƵ� �� ����
    //[Range(1, 3)] public int star = 1;

    //[Header("Adapter")]
    //public ChampionEntityAdapter adapter;
    //public ChampionEntityAdapter.TierCurve tierCurve;

    //private Entity _entity;

    //void Awake()
    //{
    //    _entity = GetComponent<Entity>();
    //}

    //void Start()
    //{
    //    if (champion == null || adapter == null) return;
    //    var entityData = adapter.Build(champion, star, tierCurve);
    //    _entity.SetData(entityData); // �� Entity.TryInitialize()�� ���ο��� ȣ���
    //}

    //// ���� �ռ�/���׷��̵� �� ȣ��
    //public void ApplyStar(int newStar)
    //{
    //    star = Mathf.Clamp(newStar, 1, 3);
    //    var entityData = adapter.Build(champion, star, tierCurve);
    //    _entity.SetData(entityData); // �� ���� ������Ʈ ���ʱ�ȭ ��� ����
    //}
}
