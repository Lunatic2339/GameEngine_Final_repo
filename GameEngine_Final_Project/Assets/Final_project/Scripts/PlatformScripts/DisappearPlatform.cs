using UnityEngine;
using System.Collections;

public class DisappearingPlatform : MonoBehaviour
{
    // 모드 선택을 위한 열거형(Enum) 정의
    public enum PlatformMode { Blink, Fade }

    [Header("모드 설정")]
    public PlatformMode platformMode = PlatformMode.Blink; // 기본은 깜빡임 모드

    [Header("공통 시간 설정")]
    public float activeTime = 2f;    // 완전히 켜져서 버티는 시간
    public float inactiveTime = 2f;  // 완전히 꺼져서 안 보이는 시간
    public float startDelay = 0f;    // 시작 지연 (엇박자용)

    [Header("Blink 모드 전용 설정")]
    public float blinkDuration = 0.5f; // 사라지기 전 깜빡이는 총 시간
    public float blinkInterval = 0.1f; // 깜빡이는 속도

    [Header("Fade 모드 전용 설정")]
    public float fadeDuration = 1.0f;  // 서서히 사라지거나 나타나는 데 걸리는 시간

    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Color originalColor; // 원래 색상 기억용

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color; // 시작할 때 색상 저장

        // 코루틴 시작
        StartCoroutine(CycleRoutine());
    }

    IEnumerator CycleRoutine()
    {
        // 1. 시작 대기 (엇박자)
        if (startDelay > 0)
            yield return new WaitForSeconds(startDelay);

        while (true) // 무한 반복
        {
            if (platformMode == PlatformMode.Blink)
            {
                // === 모드 1: 깜빡이다 사라지기 ===
                yield return StartCoroutine(BlinkModeRoutine());
            }
            else
            {
                // === 모드 2: 서서히 사라지기 (Fade) ===
                yield return StartCoroutine(FadeModeRoutine());
            }
        }
    }

    // =================================================================
    // 모드 1: Blink (기존 로직)
    // =================================================================
    IEnumerator BlinkModeRoutine()
    {
        // [ON] 켜기
        EnablePlatform(true);
        SetAlpha(1f); // 알파값 확실하게 복구
        yield return new WaitForSeconds(activeTime - blinkDuration);

        // [WARNING] 깜빡임
        float timer = 0f;
        while (timer < blinkDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }
        spriteRenderer.enabled = true; // 루프 끝나면 확실히 켜둠

        // [OFF] 끄기
        EnablePlatform(false);
        yield return new WaitForSeconds(inactiveTime);
    }

    // =================================================================
    // 모드 2: Fade (신규 로직)
    // =================================================================
    IEnumerator FadeModeRoutine()
    {
        // 1. [ON 유지] 완전히 켜진 상태로 대기
        EnablePlatform(true);
        SetAlpha(1f);
        yield return new WaitForSeconds(activeTime);

        // 2. [Fade Out] 서서히 사라짐
        // ★ 중요: 사라지는 동안에는 아직 밟을 수 있어야 함 (콜리더 ON 유지)
        yield return StartCoroutine(FadeRoutine(1f, 0f));

        // 3. [OFF 상태 진입] 완전히 투명해졌으면 콜리더 끔
        boxCollider.enabled = false; 
        yield return new WaitForSeconds(inactiveTime);

        // 4. [Fade In] 서서히 나타남
        // ★ 중요: 나타나기 시작하는 순간부터 밟을 수 있어야 함
        boxCollider.enabled = true; // 콜리더 먼저 켬
        yield return StartCoroutine(FadeRoutine(0f, 1f));
        
        // (루프가 돌아가면 1번 단계에서 activeTime만큼 대기함)
    }

    // 알파값을 부드럽게 변경하는 코루틴 (Fade In/Out 공용)
    IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration; // 0~1 사이 진행률
            SetAlpha(Mathf.Lerp(startAlpha, endAlpha, t)); // 선형 보간으로 알파값 변경
            yield return null; // 다음 프레임까지 대기
        }
        SetAlpha(endAlpha); // 끝나면 목표값으로 확실하게 고정
    }


    // =================================================================
    // 유틸리티 함수들
    // =================================================================

    // 발판 켜고 끄기 (렌더러 + 콜리더)
    void EnablePlatform(bool isActive)
    {
        spriteRenderer.enabled = isActive;
        boxCollider.enabled = isActive;
    }

    // 알파값(투명도)만 변경하는 함수
    void SetAlpha(float alpha)
    {
        Color newColor = originalColor;
        newColor.a = alpha;
        spriteRenderer.color = newColor;
    }
}