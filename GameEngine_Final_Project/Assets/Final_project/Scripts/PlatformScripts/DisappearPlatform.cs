using UnityEngine;
using UnityEngine.Tilemaps; // ★ 타일맵 기능을 쓰기 위해 필수!
using System.Collections;
public class DisappearingPlatformTilemap : MonoBehaviour
{
    public enum PlatformMode { Blink, Fade }

    [Header("모드 설정")]
    public PlatformMode platformMode = PlatformMode.Blink;

    [Header("작동 방식")]
    public bool activateOnTouch = false;

    [Header("공통 시간 설정")]
    public float activeTime = 0.5f;
    public float inactiveTime = 2f;
    public float startDelay = 0f;

    [Header("Blink 모드 전용 설정")]
    public float blinkDuration = 0.5f;
    public float blinkInterval = 0.1f;

    [Header("Fade 모드 전용 설정")]
    public float fadeDuration = 1.0f;

    // ★ 변경점: SpriteRenderer -> Tilemap 관련 컴포넌트로 변경
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    private Collider2D col; // BoxCollider2D, TilemapCollider2D 모두 호환되도록 부모 클래스 사용
    
    private Color originalColor;
    private bool isRunning = false;

    void Start()
    {
        // ★ 컴포넌트 가져오는 부분 변경
        tilemap = GetComponent<Tilemap>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        col = GetComponent<Collider2D>(); // TilemapCollider2D도 이걸로 가져와집니다.

        // 안전장치: 타일맵이 없으면 에러 로그 출력
        if (tilemap == null || tilemapRenderer == null)
        {
            Debug.LogError("이 스크립트는 Tilemap과 TilemapRenderer가 있는 오브젝트에 붙여야 합니다!", gameObject);
            return;
        }

        originalColor = tilemap.color;

        if (!activateOnTouch)
        {
            StartCoroutine(AutoCycleRoutine());
        }
        else
        {
            EnablePlatform(true);
            SetAlpha(1f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (activateOnTouch && !isRunning && collision.gameObject.CompareTag("Player"))
        {
            // 타일맵 콜라이더는 모양이 복잡할 수 있어서 윗면 체크를 단순화하거나 
            // 필요하다면 유지합니다. (여기선 유지)
            if (collision.GetContact(0).normal.y < -0.5f)
            {
                StartCoroutine(OneShotRoutine());
            }
        }
    }

    // --- 루틴 로직 (기존과 동일하지만 제어 대상만 다름) ---

    IEnumerator AutoCycleRoutine()
    {
        if (startDelay > 0) yield return new WaitForSeconds(startDelay);

        while (true)
        {
            if (platformMode == PlatformMode.Blink)
                yield return StartCoroutine(BlinkModeRoutine());
            else
                yield return StartCoroutine(FadeModeRoutine());

            if (platformMode == PlatformMode.Blink)
            {
                EnablePlatform(true);
                SetAlpha(1f);
            }
        }
    }

    IEnumerator OneShotRoutine()
    {
        isRunning = true;

        if (platformMode == PlatformMode.Blink)
        {
            yield return StartCoroutine(BlinkModeRoutine());
            EnablePlatform(true);
            SetAlpha(1f);
        }
        else
        {
            yield return StartCoroutine(FadeModeRoutine());
        }

        isRunning = false;
    }

    IEnumerator BlinkModeRoutine()
    {
        float waitTime = activeTime - blinkDuration;
        if (waitTime > 0) yield return new WaitForSeconds(waitTime);

        float timer = 0f;
        while (timer < blinkDuration)
        {
            // ★ SpriteRenderer 대신 TilemapRenderer를 껐다 켰다 함
            tilemapRenderer.enabled = !tilemapRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }
        tilemapRenderer.enabled = true;

        EnablePlatform(false);
        yield return new WaitForSeconds(inactiveTime);
    }

    IEnumerator FadeModeRoutine()
    {
        EnablePlatform(true);
        SetAlpha(1f);
        yield return new WaitForSeconds(activeTime);

        yield return StartCoroutine(FadeRoutine(1f, 0f));

        col.enabled = false;
        yield return new WaitForSeconds(inactiveTime);

        col.enabled = true;
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

    // ★ 타일맵 제어 함수로 변경
    void EnablePlatform(bool isActive)
    {
        tilemapRenderer.enabled = isActive; // 눈에 보이는 것 끄기
        col.enabled = isActive;             // 밟는 것 끄기
    }

    // ★ 타일맵 색상 제어 함수로 변경
    void SetAlpha(float alpha)
    {
        Color newColor = originalColor;
        newColor.a = alpha;
        tilemap.color = newColor; // SpriteRenderer.color 대신 Tilemap.color 사용
    }
}