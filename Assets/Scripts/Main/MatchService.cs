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
    public IEnumerator CheckMatchStatus(
        Action<string, string, long> onMatched,  // opponentId, roomId, startAt (long)
        Action onNotMatched,
        Action<string> onError)
    {
        int userId = GameManager.Instance.profile.user_id;
        string url = APIEndpoints.MatchStatus + "?userId=" + userId;

        yield return APIService.Instance.Get<MatchStatusResponse>(
            url,
            res =>
            {
                if (res.matched)
                {
                    // start_at → long 변환 시도
                    long startAtLong = 0;

                    if (long.TryParse(res.start_at, out startAtLong))
                    {
                        onMatched?.Invoke(res.opponentId, res.roomId, startAtLong);
                    }
                    else if (DateTimeOffset.TryParse(res.start_at, out var startDate))
                    {
                        // ISO8601 포맷이라면 UnixTime(초)로 변환
                        startAtLong = startDate.ToUnixTimeSeconds();
                        onMatched?.Invoke(res.opponentId, res.roomId, startAtLong);
                    }
                    else
                    {
                        Debug.LogWarning($"start_at 변환 실패: {res.start_at}");
                        onError?.Invoke("start_at parsing failed");
                    }
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
    public bool matched;
}

[Serializable]
public class MatchStatusResponse
{
    public bool matched;
    public string opponentId;
    public string roomId;
    public string start_at;  // 서버 응답 키와 동일하게 snake_case 사용
}

[Serializable]
public class BasicResponse
{
    public bool success;
    public string message;
}
