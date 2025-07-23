using System;
using System.Collections;
using UnityEngine;

public class MatchService : MonoBehaviour
{
    public static MatchService Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 매칭 큐에 유저 등록
    public IEnumerator JoinMatchQueue(Action onSuccess, Action<string> onError)
    {
        var userId = GameManager.Instance.profile.user_id;
        var req = new MatchRequest { userId = userId };

        yield return APIService.Instance.Post<MatchRequest, MatchJoinResponse>(
            APIEndpoints.MatchJoin,
            req,
            res =>
            {
                Debug.Log("매칭 큐 등록됨");
                onSuccess?.Invoke();
            },
            err =>
            {
                Debug.LogWarning("매칭 큐 등록 실패: " + err);
                onError?.Invoke(err);
            }
        );
    }

    // 매칭 상태 확인
    public IEnumerator CheckMatchStatus(Action<string, string> onMatched, Action onNotMatched, Action<string> onError)
    {
        int userId = GameManager.Instance.profile.user_id;
        string url = APIEndpoints.MatchStatus + "?userId=" + userId;

        yield return APIService.Instance.Get<MatchStatusResponse>(
            url,
            res =>
            {
                if (res.matched)
                {
                    onMatched?.Invoke(res.opponentId, res.roomId);
                }
                else
                {
                    onNotMatched?.Invoke();
                }
            },
            err =>
            {
                Debug.LogWarning("매칭 상태 확인 실패: " + err);
                onError?.Invoke(err);
            }
        );
    }

    public IEnumerator NotifyGameEnd(Action onSuccess, Action<string> onError)
    {
        var userId = GameManager.Instance.profile.user_id;
        var req = new MatchRequest { userId = userId };

        yield return APIService.Instance.Post<MatchRequest, BasicResponse>(
            APIEndpoints.MatchEnd,
            req,
            res =>
            {
                Debug.Log("게임 종료 처리 완료");
                onSuccess?.Invoke();
            },
            err =>
            {
                Debug.LogWarning("게임 종료 처리 실패: " + err);
                onError?.Invoke(err);
            }
        );
    }
}


[Serializable]
public class MatchRequest
{
    public int userId;
}

[Serializable]
public class MatchJoinResponse
{
    public bool matched; // 서버에서 matched: false 로 응답해줘도 호환 위해 유지
}

[Serializable]
public class MatchStatusResponse
{
    public bool matched;
    public string opponentId;
    public string roomId;
}

[Serializable]
public class BasicResponse
{
    public bool success;
    public string message;
}