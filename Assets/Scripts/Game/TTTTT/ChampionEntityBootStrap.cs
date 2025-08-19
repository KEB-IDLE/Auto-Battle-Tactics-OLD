// ChampionEntityBootstrap.cs
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class ChampionEntityBootstrap : MonoBehaviour
{
    //[Header("Design (from shop/controller)")]
    //public Champion champion;      // 컨트롤러가 지정하거나 프리팹에 미리 박아둘 수 있음
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
    //    _entity.SetData(entityData); // ▶ Entity.TryInitialize()가 내부에서 호출됨
    //}

    //// 상점 합성/업그레이드 시 호출
    //public void ApplyStar(int newStar)
    //{
    //    star = Mathf.Clamp(newStar, 1, 3);
    //    var entityData = adapter.Build(champion, star, tierCurve);
    //    _entity.SetData(entityData); // ▶ 기존 컴포넌트 재초기화 경로 유지
    //}
}
