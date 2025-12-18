using UnityEngine;
using System.Collections;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("목숨 설정")]
    public int maxLives = 5;      // 최대 핏방울 개수
    public int currentLives;

    // ★ UI에게 "체력 변했어!"라고 알려주는 신호탄
    public Action onHealthChanged; 

    public int dashDamage = 1; // 대쉬 공격 데미지

    [Header("넉백 설정")]
    public Vector2 knockbackForce = new Vector2(10f, 5f); // X: 밀리는 힘, Y: 뜨는 힘
    public float knockbackDuration = 0.2f; // 넉백 시간


    [Header("무적 설정")]
    public float iframeDuration = 1.5f;
    public int numberOfFlashes = 4;
    private bool isInvincible = false;

    [Header("참조")]
    public SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerMovement playerMovement;
    // public PlayerMovement playerMovement; // 나중에 맞으면 넉백(밀려남) 구현할 때 필요

    void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        if (PlayerPrefs.HasKey("SaveHealth"))
        {
            currentLives = PlayerPrefs.GetInt("SaveHealth");
        }
        else
        {
            currentLives = maxLives;
        }
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
            if(playerMovement.isDashing)
            {
                // 1. 적의 스크립트를 가져옴
                EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();
                
                // 2. 적 스크립트가 있으면 데미지 줌
                if (enemy != null)
                {
                    enemy.TakeDamage(dashDamage); // 데미지 1
                    
                    // (선택사항) 때리는 맛을 위해 반동 주기?
                    // Rigidbody2D rb = GetComponent<Rigidbody2D>();
                    // if(rb != null) rb.linearVelocity = new Vector2(-transform.localScale.x * 5f, 5f);
                }
                
                // 3. 대쉬 중엔 나는 안 아파야 하니까 여기서 끝냄
                return;
            }
            TakeDamage(1, collision.transform);
             // 적과 부딪히면 10 데미지
        }
    }


    public void TakeDamage(int damage, Transform damageSource)
    {
        // 무적 상태라면 데미지 무시
        if (isInvincible) return;

        currentLives -= damage;
        Debug.Log("플레이어 체력: " + currentLives);

        // ★ UI에게 신호 보내기! (UI야, 다시 그려라!)
        onHealthChanged?.Invoke();

        // 1. 사망 체크
        if (currentLives <= 0)
        {
            Die();
            return;
        }

        // 2. 넉백 실행 (살아있을 때만)
        if (damageSource != null)
        {
            // 방향 계산: (내 위치 - 적 위치) = 밀려날 방향
            // Mathf.Sign을 써서 왼쪽(-1)인지 오른쪽(1)인지만 명확히 가져옴
            float knockbackDir = Mathf.Sign(transform.position.x - damageSource.position.x);
            
            // 힘 벡터 만들기 (X축은 방향대로, Y축은 살짝 위로)
            Vector2 force = new Vector2(knockbackDir * knockbackForce.x, knockbackForce.y);
            
            // Movement에게 명령
            playerMovement.ApplyKnockback(force, knockbackDuration);
        }

        animator.SetTrigger("Hurt");

    

        // 2. 무적 타임 시작 (코루틴)
        StartCoroutine(InvincibilityRoutine());
    }

    public void Die()
    {
        Debug.Log("Game Over!");
        // 여기서 게임 오버 화면을 띄우거나 씬을 재시작하면 됩니다.
        
        // 일단 플레이어를 눕히거나 비활성화
        gameObject.SetActive(false); 
        // Time.timeScale = 0; // 게임 정지 (선택사항)
        // ★ [수정됨] GameManager에게 게임오버 알리기
        if (GameManager.instance != null)
        {
            GameManager.instance.GameOver();
        }
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