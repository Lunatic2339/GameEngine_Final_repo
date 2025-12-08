using UnityEngine;
using System.Collections;

public class DisappearingPlatform : MonoBehaviour
{
    public enum PlatformMode { Blink, Fade }

    [Header("모드 설정")]
    public PlatformMode platformMode = PlatformMode.Blink;

    // ★ [추가된 기능]
    [Header("작동 방식")]
    public bool activateOnTouch = false; // 체크하면 "닿았을 때" 작동, 해제하면 "자동" 반복

    [Header("공통 시간 설정")]
    public float activeTime = 0.5f;  // 밟고 나서 사라지기까지 버티는 시간 (자동 모드에선 켜져있는 시간)
    public float inactiveTime = 2f;  // 사라진 뒤 재생성까지 걸리는 시간
    public float startDelay = 0f;    // (자동 모드용) 시작 지연

    [Header("Blink 모드 전용 설정")]
    public float blinkDuration = 0.5f; // 사라지기 전 깜빡이는 시간
    public float blinkInterval = 0.1f; // 깜빡이는 속도

    [Header("Fade 모드 전용 설정")]
    public float fadeDuration = 1.0f; 

    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isRunning = false; // 현재 루틴이 실행 중인지 체크

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        // ★ 설정에 따라 시작 방식 분기
        if (!activateOnTouch)
        {
            // 1. 자동 반복 모드 (기존 동작)
            StartCoroutine(AutoCycleRoutine());
        }
        else
        {
            // 2. 터치 대기 모드 (일단 켜두고 대기)
            EnablePlatform(true);
            SetAlpha(1f);
        }
    }

    // ★ [추가됨] 플레이어가 닿았을 때 실행 (Touch 모드일 때만)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 터치 모드이고 + 아직 실행 중이 아니고 + 플레이어가 닿았다면
        if (activateOnTouch && !isRunning && collision.gameObject.CompareTag("Player"))
        {
            // 발판 윗면을 밟았을 때만 작동 (옆에서 스치면 작동 X)
            // (필요 없으면 이 if문은 지워도 됨)
            if (collision.GetContact(0).normal.y < -0.5f)
            {
                StartCoroutine(OneShotRoutine());
            }
        }
    }

    // =================================================================
    // [루틴 1] 자동 반복 (Auto)
    // =================================================================
    IEnumerator AutoCycleRoutine()
    {
        if (startDelay > 0)
            yield return new WaitForSeconds(startDelay);

        while (true)
        {
            if (platformMode == PlatformMode.Blink)
                yield return StartCoroutine(BlinkModeRoutine());
            else
                yield return StartCoroutine(FadeModeRoutine());
            
            // Blink 모드는 루틴이 끝날 때 꺼져 있으므로 다시 켜줘야 함
            // (Fade 모드는 루틴 안에 켜는 게 포함돼 있음)
            if (platformMode == PlatformMode.Blink)
            {
                EnablePlatform(true);
                SetAlpha(1f);
            }
        }
    }

    // =================================================================
    // [루틴 2] 한 번 작동하고 재생성 (Touch)
    // =================================================================
    IEnumerator OneShotRoutine()
    {
        isRunning = true; // 중복 실행 방지 잠금

        if (platformMode == PlatformMode.Blink)
        {
            yield return StartCoroutine(BlinkModeRoutine());
            
            // 재생성 (켜주기)
            EnablePlatform(true);
            SetAlpha(1f);
        }
        else
        {
            yield return StartCoroutine(FadeModeRoutine());
            // Fade 모드는 내부에서 다시 켜짐
        }

        isRunning = false; // 잠금 해제
    }

    // =================================================================
    // 동작 상세 로직 (기존 코드 재활용)
    // =================================================================
    
    IEnumerator BlinkModeRoutine()
    {
        // [ON 상태 유지] (activeTime - 깜빡임 시간) 만큼 대기
        // * 터치 모드일 경우: 밟고 나서 이 시간만큼 버티다 깜빡임
        float waitTime = activeTime - blinkDuration;
        if (waitTime > 0)
            yield return new WaitForSeconds(waitTime);

        // [WARNING] 깜빡임
        float timer = 0f;
        while (timer < blinkDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }
        spriteRenderer.enabled = true; 

        // [OFF] 끄기
        EnablePlatform(false);
        yield return new WaitForSeconds(inactiveTime);
    }

    IEnumerator FadeModeRoutine()
    {
        // 1. [ON 유지]
        EnablePlatform(true);
        SetAlpha(1f);
        yield return new WaitForSeconds(activeTime); // 밟고 나서 버티는 시간

        // 2. [Fade Out] 서서히 사라짐
        yield return StartCoroutine(FadeRoutine(1f, 0f));

        // 3. [OFF]
        boxCollider.enabled = false; 
        yield return new WaitForSeconds(inactiveTime); // 사라져 있는 시간

        // 4. [Fade In] 재생성
        boxCollider.enabled = true;
        yield return StartCoroutine(FadeRoutine(0f, 1f));
    }

    IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            SetAlpha(Mathf.Lerp(startAlpha, endAlpha, t));
            yield return null;
        }
        SetAlpha(endAlpha);
    }

    void EnablePlatform(bool isActive)
    {
        spriteRenderer.enabled = isActive;
        boxCollider.enabled = isActive;
    }

    void SetAlpha(float alpha)
    {
        Color newColor = originalColor;
        newColor.a = alpha;
        spriteRenderer.color = newColor;
    }
}