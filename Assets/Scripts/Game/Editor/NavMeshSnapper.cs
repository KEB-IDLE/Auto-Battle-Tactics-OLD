using UnityEngine;
using UnityEngine.AI;

[ExecuteAlways]
public class NavMeshSnapper : MonoBehaviour
{
    public float maxSnapDistance = 5f;
    public bool snapInEditMode = true;
    public bool snapOnPlay = true;

    NavMeshAgent agent;

    void OnEnable()
    {
        agent = GetComponent<NavMeshAgent>();
#if UNITY_EDITOR
        if (!Application.isPlaying && snapInEditMode) TrySnap();
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && snapInEditMode) TrySnap();
#endif
        if (Application.isPlaying && snapOnPlay) TrySnap();
    }

    void TrySnap()
    {
        if (agent == null) return;

        if (NavMesh.SamplePosition(transform.position, out var hit, maxSnapDistance, NavMesh.AllAreas))
        {
            // 에디터/런타임 공통: agent가 있으면 Warp가 가장 안전
            if (Application.isPlaying)
                agent.Warp(hit.position);
            else
                transform.position = hit.position; // 에디터에선 직접 위치 이동
        }
    }
}
