using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator StartMatchFlow()
    {
        yield return MatchService.Instance.JoinMatchQueue(
            onSuccess: () =>
            {
                Debug.Log("큐 등록 완료, 매칭 대기 시작...");
                StartCoroutine(PollUntilMatched());
            },
            onError: err =>
            {
                Debug.LogWarning("큐 등록 실패: " + err);
                // TODO: 실패 UI 띄우기
            }
        );
    }

    private IEnumerator PollUntilMatched()
    {
        float pollInterval = 2f;
        float timeout = 30f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            yield return MatchService.Instance.CheckMatchStatus(
                onMatched: (opponentId, roomId) =>
                {
                    Debug.Log("매칭 성공! 상대: " + opponentId + ", room: " + roomId);
                    GameManager.Instance.opponentId = opponentId;
                    GameManager.Instance.roomId = roomId;

                    SceneManager.LoadScene(2); // 게임씬 로드
                },
                onNotMatched: () =>
                {
                    Debug.Log("매칭 대기 중...");
                },
                onError: err =>
                {
                    Debug.LogWarning("상태 확인 실패: " + err);
                }
            );

            yield return new WaitForSeconds(pollInterval);
            elapsed += pollInterval;
        }

        Debug.LogWarning("매칭 시간 초과");
        // TODO: 타임아웃 UI 처리
    }

    public IEnumerator EndMatchFlow()
    {
        yield return MatchService.Instance.NotifyGameEnd(
            onSuccess: () =>
            {
                Debug.Log("게임 종료 처리 완료");

                //SceneManager.LoadScene("MainLobby");
            },
            onError: err =>
            {
                Debug.LogWarning("게임 종료 처리 실패: " + err);
                // TODO: 실패 UI 처리
            }
        );
    }
}
