using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Clip (Editor 등록)")]
    [SerializeField] private AudioClip battleClip;

    [Header("Output (선택)")]
    [SerializeField] private AudioMixerGroup musicGroup;

    [Header("재생 옵션")]
    [Range(0f, 1f)][SerializeField] private float volume = 1f;
    [SerializeField] private float fadeInSeconds = 1.0f;
    [SerializeField] private float fadeOutSeconds = 0.8f;

    private AudioSource _src;
    private Coroutine _fadeCo;

    private void Awake()
    {
        // 싱글톤 + 씬 넘어가도 유지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 오디오 소스 초기화(2D, 루프)
        _src = gameObject.AddComponent<AudioSource>();
        _src.loop = true;
        _src.playOnAwake = false;
        _src.spatialBlend = 0f;   // 2D
        _src.dopplerLevel = 0f;
        _src.outputAudioMixerGroup = musicGroup;
        _src.volume = 0f;
    }

    /// <summary>
    /// 전투 BGM 재생(에디터에서 등록한 battleClip 사용)
    /// </summary>
    public void PlayBattle(float? fadeOverride = null)
    {
        if (battleClip == null)
        {
            Debug.LogWarning("[MusicManager] battleClip이 지정되지 않았습니다.");
            return;
        }

        // 동일 곡이 이미 재생 중이면 무시
        if (_src.isPlaying && _src.clip == battleClip) return;

        // 페이드 코루틴 정리
        if (_fadeCo != null) StopCoroutine(_fadeCo);

        _src.clip = battleClip;
        _fadeCo = StartCoroutine(FadeIn(fadeOverride ?? fadeInSeconds));
    }

    /// <summary>
    /// 현재 곡 정지(페이드 아웃)
    /// </summary>
    public void StopMusic(float? fadeOverride = null)
    {
        if (!_src.isPlaying) return;
        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(FadeOutAndStop(fadeOverride ?? fadeOutSeconds));
    }

    /// <summary>
    /// 일시정지/재개가 필요하면 사용
    /// </summary>
    public void PauseMusic() { if (_src.isPlaying) _src.Pause(); }
    public void ResumeMusic() { if (!_src.isPlaying && _src.clip != null) _src.UnPause(); }

    public bool IsPlaying => _src.isPlaying && _src.volume > 0.0001f;
    public void SetVolume01(float v) { volume = Mathf.Clamp01(v); if (_src.isPlaying && _fadeCo == null) _src.volume = volume; }

    private IEnumerator FadeIn(float dur)
    {
        // 즉시 재생 모드
        if (dur <= 0f)
        {
            _src.volume = volume;
            _src.Play();
            yield break;
        }

        float t = 0f;
        _src.volume = 0f;
        _src.Play();

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            _src.volume = Mathf.Lerp(0f, volume, t / dur);
            yield return null;
        }
        _src.volume = volume;
        _fadeCo = null;
    }

    private IEnumerator FadeOutAndStop(float dur)
    {
        if (dur <= 0f)
        {
            _src.Stop();
            _src.volume = 0f;
            yield break;
        }

        float t = 0f;
        float start = _src.volume;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            _src.volume = Mathf.Lerp(start, 0f, t / dur);
            yield return null;
        }
        _src.Stop();
        _src.volume = 0f;
        _fadeCo = null;
    }
}
