using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource; // 음악 틀어줄 스피커

    [Header("음악 파일들")]
    public AudioClip stageBGM; // 평소 브금
    public AudioClip bossBGM;  // 보스전 브금

    void Start()
    {
        // 게임 시작하면 평소 브금 재생
        PlayStageMusic();
    }

    // 평소 브금 트는 함수
    public void PlayStageMusic()
    {
        if (audioSource.clip != stageBGM) // 이미 틀어져 있으면 굳이 또 안 틈
        {
            audioSource.clip = stageBGM;
            audioSource.Play();
        }
    }

    // 보스 브금 트는 함수
    public void PlayBossMusic()
    {
        if (audioSource.clip != bossBGM)
        {
            audioSource.clip = bossBGM;
            audioSource.Play();
        }
    }
}