using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;  // ✅ 추가

public class StartUIController : MonoBehaviour
{
    public TMP_Text countdownText;
    public static StartUIController Instance { get; private set; }
    public float countdownTime = 10f;

    private bool battleStarted = false;
    private bool countdownStarted = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (countdownText == null)
        {
            Debug.LogError("❌ countdownText가 Inspector에서 할당되지 않았습니다!");
            return;
        }

        countdownText.text = "Waiting...";
        countdownText.gameObject.SetActive(true);
    }

    public void BeginCountdown()
    {
        if (countdownStarted) return;
        countdownStarted = true;

        Debug.Log("⏳ [UI] 카운트다운 시작 요청 수신");
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

        countdownText.text = "Waiting...";

        if (UserNetwork.Instance != null)
        {
            Debug.Log("📤 [UI] 준비 완료 전송");
            UserNetwork.Instance.SendReady();
        }
        else
        {
            Debug.LogWarning("⚠️ UserNetwork.Instance가 아직 null입니다. ready 메시지 전송 실패");
        }
    }

    public void OnAllPlayersReady()
    {
        Debug.Log("💥 [UI] 모든 플레이어 준비됨 → BattleStartSequence 시작");
        if (battleStarted) return;
        battleStarted = true;
        StartCoroutine(BattleStartSequence());
    }

    public void ResetUI()
    {
        StopAllCoroutines();
        battleStarted = false;
        countdownStarted = false;

        if (countdownText != null)
        {
            countdownText.text = "Waiting...";
            countdownText.gameObject.SetActive(true);
        }
    }

    private IEnumerator BattleStartSequence()
    {
        Debug.Log("[UI] BattleStartSequence 시작");

        countdownText.text = "Battle Start!";
        yield return new WaitForSeconds(1f);
        countdownText.text = string.Empty;
        countdownText.gameObject.SetActive(false);

        if (GameManager2.Instance != null)
        {
            Debug.Log("[UI] GameManager2.StartBattle() 호출");
            GameManager2.Instance.StartBattle();
            GameManager2.Instance.LockAllUnitsMovement();
            GameManager2.Instance.SendInitMessages();
            Debug.Log($"🧾 InitMessage 저장 개수: {GameManager2.Instance.GetInitMessages().Count}");
            yield return new WaitForSeconds(0.2f);
            GameManager2.Instance.DeactivateAllMyUnits();
        }
        else
        {
            Debug.LogWarning("GameManager2 인스턴스가 존재하지 않습니다.");
        }

        string sceneName = "4-BattleScene";
        Debug.Log($"[UI] 씬 전환 시도: {sceneName}");

        try
        {
            SceneManager.LoadScene(sceneName);
            Debug.Log("[UI] 씬 로드 호출 성공 (예외 없음)");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ 씬 로드 실패: {ex.Message}");
        }

    }
}
