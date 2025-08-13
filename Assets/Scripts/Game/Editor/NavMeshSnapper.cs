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
            // ������/��Ÿ�� ����: agent�� ������ Warp�� ���� ����
            if (Application.isPlaying)
                agent.Warp(hit.position);
            else
                transform.position = hit.position; // �����Ϳ��� ���� ��ġ �̵�
        }
    }
}
