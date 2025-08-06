using UnityEngine;
using TMPro;
using System.Collections;

public class TimerManager : MonoBehaviour
{
    public TMP_Text countdownText;
    public static TimerManager Instance { get; private set; }
    public float countdownTime;

    private bool countdownStarted = false;

    private void OnEnable()
    {
        Debug.Log("⏲ TimerManager OnEnable 호출됨");

        countdownStarted = false;

        if (countdownText == null)
        {
            countdownText = GameObject.Find("Timer")?.GetComponent<TMP_Text>();
            Debug.Log($"⏲ TimerManager: countdownText 다시 연결됨 → {countdownText != null}");
        }

        if (countdownText != null)
        {
            countdownText.text = "...";
            countdownText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ TimerManager: countdownText 연결 실패");
        }
    }


    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (countdownText == null)
        {
            Debug.LogError("❌ countdownText가 Inspector에서 할당되지 않았습니다!");
            return;
        }

        countdownText.text = "...";
        countdownText.gameObject.SetActive(true);
        ResetUI();
    }

    public void BeginCountdown()
    {
        if (countdownStarted)
        {
            Debug.Log("⏭ [UI] 카운트다운 이미 진행 중 → 무시됨");
            return;
        }

        if (countdownText == null)
        {
            countdownText = GameObject.Find("Timer")?.GetComponent<TMP_Text>();
            Debug.Log($"⏲ [UI] countdownText 연결 시도 결과 → {(countdownText != null ? "성공" : "실패")}");

            if (countdownText == null)
            {
                Debug.LogError("❌ [TimerManager] countdownText 연결 실패. 'Timer' 오브젝트가 씬에 존재하지 않거나 이름이 다름.");
                return;
            }
        }

        countdownText.gameObject.SetActive(true);
        countdownText.text = "...";

        StopAllCoroutines();
        countdownStarted = true;

        Debug.Log("⏳ [UI] 카운트다운 시작 요청 수신");
        Debug.Log($"⏳ countdownTime = {countdownTime}");

        StartCoroutine(CountdownAndNotifyReady());
    }


    private IEnumerator CountdownAndNotifyReady()
    {
        float currentTime = countdownTime;

        while (currentTime > 0f)
        {
            countdownText.text = Mathf.CeilToInt(currentTime).ToString();
            yield return new WaitForSeconds(1f);
            currentTime -= 1f;
        }

        countdownText.text = "...";

        Debug.Log("📤 [UI] 준비 완료 전송");
        UserNetwork.Instance?.SendReady();

        countdownStarted = false;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Battle"))
        {
            var battleManager = Object.FindFirstObjectByType<BattleSceneManager>();
            if (battleManager != null)
            {
                battleManager.EndBattleByTimeout();
            }
        }
    }

    public void ResetUI()
    {
        StopAllCoroutines();
        countdownStarted = false;

        if (countdownText != null)
        {
            countdownText.text = "...";
            countdownText.gameObject.SetActive(true);
        }

        UserNetwork.Instance?.ResetReadyState();
    }
}
