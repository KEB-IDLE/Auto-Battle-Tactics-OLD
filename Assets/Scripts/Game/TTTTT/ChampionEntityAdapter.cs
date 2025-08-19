// ChampionEntityAdapter.cs
using UnityEngine;

[CreateAssetMenu(fileName = "ChampionEntityAdapter", menuName = "Adapters/Champion→EntityData")]
public class ChampionEntityAdapter : ScriptableObject
{
    //// Champion의 필드명을 실제 프로젝트에 맞춰서 매핑하세요.
    //[System.Serializable]
    //public class TierCurve
    //{
    //    public float hp1, hp2, hp3;
    //    public float atk1, atk2, atk3;
    //    public float atkSpd1, atkSpd2, atkSpd3;
    //}

    //public EntityData Build(Champion champ, int star, TierCurve curve)
    //{
    //    var data = ScriptableObject.CreateInstance<EntityData>();

    //    // 예시 매핑(필드명은 프로젝트에 맞게 교체)
    //    //data.displayName = champ.championName;
    //    data.redPrefab = champ.prefab;
    //    data.attackProjectile = champ.attackProjectile; // 원거리 공격용
    //    data.maxHP = star == 1 ? curve.hp1 : star == 2 ? curve.hp2 : curve.hp3;
    //    data.attackDamage = star == 1 ? curve.atk1 : star == 2 ? curve.atk2 : curve.atk3;
    //    data.attackCoreDamage = star == 1 ? curve.atkSpd1 : star == 2 ? curve.atkSpd2 : curve.atSpd3;
    //    data.attackType = champ.attackType;
    //    data.projectilePrefab = champ.projectilePrefab;    // 원거리만
    //    data.projectileData = champ.projectileData;      // 연출/비행 이펙트
    //    data.attackEffectPrefab = champ.attackEffectPrefab;  // 타격 이펙트
    //    data.takeDamageEffectPrefeb = champ.hitEffectPrefab;
    //    data.deathEffectPrefab = champ.deathEffectPrefab;
    //    data.summonEffectPrefab = champ.summonEffectPrefab;

    //    // 필요 시 이동속도, 사거리, 시너지 태그 등 추가 매핑
    //    return data;
    //}
}
