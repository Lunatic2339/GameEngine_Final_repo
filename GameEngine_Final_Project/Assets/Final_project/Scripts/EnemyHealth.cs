using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public int maxHealth = 30; // 적의 최대 체력
    private int currentHealth;

    [Header("피격 효과")]
    public SpriteRenderer spriteRenderer; // 색깔 바꿀 렌더러
    public Color hitColor = Color.red;    // 맞았을 때 색
    private Color originalColor;          // 원래 색 저장용

    void Start()
    {
        currentHealth = maxHealth;
        
        // 자동으로 SpriteRenderer 찾기
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        // 원래 색깔 기억해두기 (보통 흰색)
        originalColor = spriteRenderer.color;
    }

    // 외부(총알)에서 이 함수를 호출해서 데미지를 줍니다.
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " 남은 체력: " + currentHealth);

        // 1. 피격 효과 (깜빡임) 실행
        StartCoroutine(FlashRoutine());

        // 2. 체력이 0 이하면 사망
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " 사망!");
        
        // 나중에 폭발 이펙트나 사운드 추가 가능
        // Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject); // 적 오브젝트 삭제
    }

    // 맞았을 때 잠깐 빨개졌다가 돌아오는 코루틴
    IEnumerator FlashRoutine()
    {
        spriteRenderer.color = hitColor;       // 빨간색으로 변신
        yield return new WaitForSeconds(0.3f); // 0.1초 대기
        spriteRenderer.color = originalColor;  // 원래 색으로 복구
    }
}