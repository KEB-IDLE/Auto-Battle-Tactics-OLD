using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndController : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "1-MainScene"; // 너의 메인화면 씬 이름
    [SerializeField] private float delayBeforeLoad = 1.0f;          // 잠깐 연출 시간(원하면 0)

    private bool _ending;

    private void OnEnable()
    {
        CombatManager.OnGameEnd += HandleGameEnd;
    }

    private void OnDisable()
    {
        CombatManager.OnGameEnd -= HandleGameEnd;
    }

    private void HandleGameEnd()
    {
        if (_ending) return; // 중복 방지
        _ending = true;

        // 1) 전 유닛 행동 정지 (MoveComponent.SafeStopAgent 가드가 있어야 예외 없음)
        GameManager2.Instance?.LockAllUnits(); // 네 프로젝트에 이미 있는 메서드 재사용

        // 2) 입력/스폰/UI 타이머 등 멈출 게 있으면 여기서 끊기 (선택)
        // GameManager2.Instance?.DisableInputs(); ...
        // TimerManager.Instance?.Stop(); ...

        // 3) 메인화면으로 전환
        StartCoroutine(LoadMainMenu());
    }

    private System.Collections.IEnumerator LoadMainMenu()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}