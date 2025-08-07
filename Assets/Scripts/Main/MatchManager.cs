using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour, IMatchInterface
{
    public TMP_Text matchTimerText;  // 매칭 대기 시간 표시 텍스트
    private Coroutine matchTimerCoroutine;  // 타이머 코루틴 참조

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
                StartMatchTimer();  // 매칭 타이머 시작
                StartCoroutine(PollUntilMatched());
            },
            onError: err =>
            {
                Debug.LogWarning("큐 등록 실패: " + err);
                ClearMatchUI();  // 실패 시 타이머 종료
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
                onMatched: (opponentId, roomId, startAt) =>
                {
                    Debug.Log($"매칭 성공! 상대: {opponentId}, room: {roomId}, start_at: {startAt}");
                    SessionManager.Instance.opponentId = opponentId;
                    SessionManager.Instance.roomId = roomId;

                    StartCoroutine(OnMatchSuccess(startAt));  // 매칭 성공 처리 & 동시 시작 대기
                    elapsed = timeout;  // 폴링 종료
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

            if (elapsed >= timeout) break;

            yield return new WaitForSeconds(pollInterval);
            elapsed += pollInterval;
        }

        if (elapsed >= timeout)
        {
            Debug.LogWarning("매칭 시간 초과");
            ClearMatchUI();
        }
    }

    private IEnumerator OnMatchSuccess(long startAtUnix)
    {
        // 매칭 타이머만 중지하고 UI 전체는 비활성화하지 않음
        if (matchTimerCoroutine != null)
        {
            StopCoroutine(matchTimerCoroutine);
            matchTimerCoroutine = null;
        }

        // "MATCHED!" 표시
        if (matchTimerText != null)
            matchTimerText.text = "MATCHED!";

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        float wait = Mathf.Max(0, startAtUnix - now);

        yield return new WaitForSeconds(wait);

        ClearMatchUI();  // 씬 전환 전 UI 초기화
        SceneManager.LoadScene(2);
    }

    public void StartMatchTimer()
    {
        if (matchTimerText != null)
            matchTimerText.transform.parent.gameObject.SetActive(true);

        if (matchTimerCoroutine != null)
            StopCoroutine(matchTimerCoroutine);

        matchTimerCoroutine = StartCoroutine(MatchTimerRoutine());
    }

    private IEnumerator MatchTimerRoutine()
    {
        float elapsed = 0f;

        while (true)
        {
            elapsed += Time.deltaTime;
            int minutes = (int)(elapsed / 60);
            int seconds = (int)(elapsed % 60);
            matchTimerText.text = $"WAITING... {minutes:00}:{seconds:00}";
            yield return null;
        }
    }

    public void ClearMatchUI()
    {
        if (matchTimerCoroutine != null)
        {
            StopCoroutine(matchTimerCoroutine);
            matchTimerCoroutine = null;
        }

        if (matchTimerText != null)
        {
            matchTimerText.text = string.Empty;
            matchTimerText.transform.parent.gameObject.SetActive(false);
        }
    }

    public IEnumerator EndMatchFlow()
    {
        yield return MatchService.Instance.NotifyGameEnd(
            onSuccess: () =>
            {
                Debug.Log("게임 종료 처리 완료");
            },
            onError: err =>
            {
                Debug.LogWarning("게임 종료 처리 실패: " + err);
            }
        );
    }
}