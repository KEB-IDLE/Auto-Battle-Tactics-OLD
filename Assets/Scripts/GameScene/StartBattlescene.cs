// ✅ StartBattleScene.cs
using UnityEngine;
using System;
using System.Collections;

public class StartBattleScene : MonoBehaviour
{
    IEnumerator Start()
    {
        Debug.Log("🟢 [BattleSceneManager] 전투 씬 시작됨 → 유닛 복원 시도");

        // GameManager2 대기
        while (GameManager2.Instance == null)
            yield return null;

        // InitMessage 수신 대기 (최대 3초)
        float timeout = 3f;
        while (GameManager2.Instance.GetInitMessages().Count == 0 && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        var initMessages = GameManager2.Instance.GetInitMessages();
        Debug.Log($"📦 [BattleScene] 복원할 InitMessage 개수: {initMessages.Count}");

        foreach (var msg in initMessages)
        {
            Vector3 position = new Vector3(msg.position[0], msg.position[1], msg.position[2]);
            var data = UnitManager.Instance.GetEntityData(msg.unitType);
            if (data == null || data.entityPrefab == null)
            {
                Debug.LogError($"❌ [복원 실패] 프리팹 없음: {msg.unitType}");
                continue;
            }

            GameObject go = Instantiate(data.entityPrefab, position, Quaternion.identity);
            var entity = go.GetComponent<Entity>();
            entity.SetUnitId(msg.unitId);
            entity.SetOwnerId(msg.ownerId);
            GameManager2.Instance.Register(entity);

            if (Enum.TryParse(msg.team, out Team parsedTeam))
                go.GetComponent<TeamComponent>()?.SetTeam(parsedTeam);

            int parsedLayer = LayerMask.NameToLayer(msg.layer);
            if (parsedLayer != -1) go.layer = parsedLayer;

            go.GetComponent<HealthComponent>()?.Initialize(data);
            go.GetComponent<AnimationComponent>()?.Initialize(data);
            go.GetComponent<AttackComponent>()?.Initialize(data);
            go.GetComponent<EffectComponent>()?.Initialize(data);
            go.GetComponent<UnitNetwork>()?.InitializeNetwork(msg.ownerId == UserNetwork.Instance.MyId);

            Debug.Log($"✅ 복원 완료: {msg.unitType} ({msg.unitId})");
        }

        Debug.Log("🚩 [BattleSceneManager] 복원 완료 신호 전송됨");
    }
}
