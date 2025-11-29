using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("목숨 설정")]
    public int maxLives = 5;      // 최대 핏방울 개수
    public int currentLives;

    // ★ UI에게 "체력 변했어!"라고 알려주는 신호탄
    public Action onHealthChanged; 

    [Header("무적 설정")]
    public float iframeDuration = 1.5f;
    public int numberOfFlashes = 4;
    private bool isInvincible = false;

    [Header("참조")]
    public SpriteRenderer spriteRenderer;
    // public PlayerMovement playerMovement; // 나중에 맞으면 넉백(밀려남) 구현할 때 필요

    void Start()
    {
        currentLives = maxLives;
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        onHealthChanged?.Invoke();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(10);
             // 적과 부딪히면 10 데미지
        }
    }


    public void TakeDamage(int damage)
    {
        // 무적 상태라면 데미지 무시
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log("플레이어 체력: " + currentHealth);

        // ★ UI에게 신호 보내기! (UI야, 다시 그려라!)
        onHealthChanged?.Invoke();

        // 1. 사망 체크
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // 2. 무적 타임 시작 (코루틴)
        StartCoroutine(InvincibilityRoutine());
    }

    void Die()
    {
        Debug.Log("Game Over!");
        // 여기서 게임 오버 화면을 띄우거나 씬을 재시작하면 됩니다.
        
        // 일단 플레이어를 눕히거나 비활성화
        gameObject.SetActive(false); 
        // Time.timeScale = 0; // 게임 정지 (선택사항)
    }

    // ★ 무적 시간 깜빡임 효과 (핵심)
    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        
        // 레이어를 바꿔서 적과 충돌 안 하게 할 수도 있지만, 일단 깜빡임만 구현
        for (int i = 0; i < numberOfFlashes; i++)
        {
            // 투명하게
            spriteRenderer.color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(iframeDuration / (numberOfFlashes * 2));
            
            // 원래대로
            spriteRenderer.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(iframeDuration / (numberOfFlashes * 2));
        }

        isInvincible = false;
    }
}