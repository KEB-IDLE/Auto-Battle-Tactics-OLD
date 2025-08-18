using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Clip (Editor ���)")]
    [SerializeField] private AudioClip battleClip;

    [Header("Output (����)")]
    [SerializeField] private AudioMixerGroup musicGroup;

    [Header("��� �ɼ�")]
    [Range(0f, 1f)][SerializeField] private float volume = 1f;
    [SerializeField] private float fadeInSeconds = 1.0f;
    [SerializeField] private float fadeOutSeconds = 0.8f;

    private AudioSource _src;
    private Coroutine _fadeCo;

    private void Awake()
    {
        // �̱��� + �� �Ѿ�� ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ����� �ҽ� �ʱ�ȭ(2D, ����)
        _src = gameObject.AddComponent<AudioSource>();
        _src.loop = true;
        _src.playOnAwake = false;
        _src.spatialBlend = 0f;   // 2D
        _src.dopplerLevel = 0f;
        _src.outputAudioMixerGroup = musicGroup;
        _src.volume = 0f;
    }

    /// <summary>
    /// ���� BGM ���(�����Ϳ��� ����� battleClip ���)
    /// </summary>
    public void PlayBattle(float? fadeOverride = null)
    {
        if (battleClip == null)
        {
            Debug.LogWarning("[MusicManager] battleClip�� �������� �ʾҽ��ϴ�.");
            return;
        }

        // ���� ���� �̹� ��� ���̸� ����
        if (_src.isPlaying && _src.clip == battleClip) return;

        // ���̵� �ڷ�ƾ ����
        if (_fadeCo != null) StopCoroutine(_fadeCo);

        _src.clip = battleClip;
        _fadeCo = StartCoroutine(FadeIn(fadeOverride ?? fadeInSeconds));
    }

    /// <summary>
    /// ���� �� ����(���̵� �ƿ�)
    /// </summary>
    public void StopMusic(float? fadeOverride = null)
    {
        if (!_src.isPlaying) return;
        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(FadeOutAndStop(fadeOverride ?? fadeOutSeconds));
    }

    /// <summary>
    /// �Ͻ�����/�簳�� �ʿ��ϸ� ���
    /// </summary>
    public void PauseMusic() { if (_src.isPlaying) _src.Pause(); }
    public void ResumeMusic() { if (!_src.isPlaying && _src.clip != null) _src.UnPause(); }

    public bool IsPlaying => _src.isPlaying && _src.volume > 0.0001f;
    public void SetVolume01(float v) { volume = Mathf.Clamp01(v); if (_src.isPlaying && _fadeCo == null) _src.volume = volume; }

    private IEnumerator FadeIn(float dur)
    {
        // ��� ��� ���
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
