// ChampionEntityAdapter.cs
using UnityEngine;

[CreateAssetMenu(fileName = "ChampionEntityAdapter", menuName = "Adapters/Champion��EntityData")]
public class ChampionEntityAdapter : ScriptableObject
{
    //// Champion�� �ʵ���� ���� ������Ʈ�� ���缭 �����ϼ���.
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

    //    // ���� ����(�ʵ���� ������Ʈ�� �°� ��ü)
    //    //data.displayName = champ.championName;
    //    data.redPrefab = champ.prefab;
    //    data.attackProjectile = champ.attackProjectile; // ���Ÿ� ���ݿ�
    //    data.maxHP = star == 1 ? curve.hp1 : star == 2 ? curve.hp2 : curve.hp3;
    //    data.attackDamage = star == 1 ? curve.atk1 : star == 2 ? curve.atk2 : curve.atk3;
    //    data.attackCoreDamage = star == 1 ? curve.atkSpd1 : star == 2 ? curve.atkSpd2 : curve.atSpd3;
    //    data.attackType = champ.attackType;
    //    data.projectilePrefab = champ.projectilePrefab;    // ���Ÿ���
    //    data.projectileData = champ.projectileData;      // ����/���� ����Ʈ
    //    data.attackEffectPrefab = champ.attackEffectPrefab;  // Ÿ�� ����Ʈ
    //    data.takeDamageEffectPrefeb = champ.hitEffectPrefab;
    //    data.deathEffectPrefab = champ.deathEffectPrefab;
    //    data.summonEffectPrefab = champ.summonEffectPrefab;

    //    // �ʿ� �� �̵��ӵ�, ��Ÿ�, �ó��� �±� �� �߰� ����
    //    return data;
    //}
}
