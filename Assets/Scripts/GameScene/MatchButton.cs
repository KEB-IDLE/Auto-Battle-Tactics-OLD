using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchButton : MonoBehaviour
{
    [SerializeField] private Button matchButton;   // 이 버튼 자신을 드래그해서 넣어도 됨
    [SerializeField] private Text statusText;      // 선택(없어도 OK)

    [Serializable] private class JoinReq { public int user_id; }
    [Serializable] private class JoinRes { public bool matched; }
    [Serializable]
    private class StatusRes
    {
        public bool matched;
        public string opponentId;
        public string roomId;
        public string start_at;   // epoch seconds (string)
    }

    private bool matching;

    void Reset() { matchButton = GetComponent<Button>(); }
    void Awake()
    {
        if (matchButton == null) matchButton = GetComponent<Button>();
        if (matchButton != null) matchButton.onClick.AddListener(OnClickMatch);
        SetStatus("");
    }

    public void OnClickMatch()
    {
        if (matching) return;

        int userId = SessionManager.Instance != null
            ? SessionManager.Instance.GetOrCreateUserId()
            : Mathf.Abs(System.Guid.NewGuid().GetHashCode()); // 극단적 예외 대비

        matching = true;
        if (matchButton) matchButton.interactable = false;
        SetStatus("큐 등록…");
        StartCoroutine(MatchFlow(userId));   // ← 이 userId가 /api/match/join 으로 전송됨
    }

    IEnumerator MatchFlow(int userId)
    {
        // 1) 큐 등록
        bool joined = false;
        yield return APIService.Instance.Post<JoinReq, JoinRes>(
            "/match/join",
            new JoinReq { user_id = userId },
            _ => joined = true,
            err => Debug.LogError("join 실패: " + err)
        );
        if (!joined) { Done("등록 실패"); yield break; }

        // 2) 상태 폴링
        SetStatus("상대 찾는 중…");
        StatusRes st = null;
        float waited = 0f, timeout = 60f, interval = 0.75f;

        while (waited < timeout)
        {
            bool ok = false;
            yield return APIService.Instance.Get<StatusRes>(
                "/match/status",
                res => { ok = true; st = res; },
                err => Debug.LogError("status 실패: " + err)
            );

            if (ok && st != null && st.matched) break;

            yield return new WaitForSeconds(interval);
            waited += interval;
        }

        if (st == null || !st.matched) { Done("시간 초과"); yield break; }

        // 3) 세션 기록(선택)
        SessionManager.Instance.opponentId = st.opponentId;
        SessionManager.Instance.roomId = st.roomId;

        // 4) 시작 시간 동기화(선택)
        if (!string.IsNullOrEmpty(st.start_at) && long.TryParse(st.start_at, out long startEpoch))
        {
            double wait = startEpoch - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (wait > 0) { SetStatus($"시작까지 {wait:0}s"); yield return new WaitForSeconds((float)wait); }
        }

        // 5) 게임(배치) 씬으로 이동
        SetStatus("게임 진입…");
        SceneManager.LoadScene("2-GameScene");

        Done("");
    }

    void Done(string msg)
    {
        matching = false;
        if (matchButton) matchButton.interactable = true;
        SetStatus(msg);
    }

    void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        if (!string.IsNullOrEmpty(msg)) Debug.Log("MatchButton: " + msg);
    }
}
