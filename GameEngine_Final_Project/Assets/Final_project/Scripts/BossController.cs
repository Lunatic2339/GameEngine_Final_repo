using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("기본 설정")]
    public float maxHealth = 50f;
    private float currentHealth;
    public Transform player;      
    public bool isActivated = false; 

    [Header("공격 설정")]
    public GameObject bulletPrefab; 
    public Transform firePoint;     
    public float patternInterval = 2f; 

    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public Transform[] moveSpots; 
    public Transform rainSpot;    

    private Collider2D myCollider; 
    public BossCamSwitcher camSwitcher;
    
    [Header("클리어 보상")]
    public GameObject endingPlatform;
    public MusicManager musicManager;

    void Start()
    {
        currentHealth = maxHealth;
        myCollider = GetComponent<Collider2D>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    public void ActivateBoss()
    {
        if (!isActivated)
        {
            isActivated = true;
            Debug.Log("보스 활성화!");
            StartCoroutine(BossPatternRoutine());
        }
    }

    IEnumerator BossPatternRoutine()
    {
        yield return new WaitForSeconds(1f);

        while (currentHealth > 0)
        {
            int randPattern = Random.Range(0, 4); 
            
            switch (randPattern)
            {
                case 0: // 조준 연사
                    yield return StartCoroutine(MoveToRandomSpot());
                    yield return StartCoroutine(Pattern_RapidFire()); 
                    break;
                case 1: // 원형 방사 (3연속)
                    yield return StartCoroutine(MoveToRandomSpot());
                    yield return StartCoroutine(Pattern_CircleFire()); 
                    break;
                case 2: // 스파이럴 (길게)
                    yield return StartCoroutine(MoveToRandomSpot());
                    yield return StartCoroutine(Pattern_SpiralFire()); 
                    break;
                case 3: // 공중 폭격
                    yield return StartCoroutine(Pattern_RainFire()); 
                    break;
            }

            yield return new WaitForSeconds(patternInterval);
        }
    }

    IEnumerator MoveToRandomSpot()
    {
        if (moveSpots.Length > 0)
        {
            int randSpot = Random.Range(0, moveSpots.Length);
            yield return StartCoroutine(MoveToSpot(moveSpots[randSpot].position));
        }
    }

    IEnumerator MoveToSpot(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // --------------------------------------------------------
    // ★ [수정됨] 패턴 1: 조준 연사 (발 말고 몸통 조준 + 발사 수 증가)
    // --------------------------------------------------------
    IEnumerator Pattern_RapidFire()
    {
        Debug.Log("패턴 1: 조준 연사 강화판");
        
        // 10발 -> 20발로 증가
        for (int i = 0; i < 20; i++) 
        {
            if (player == null) break;

            // ★ [핵심] 플레이어의 위치(발바닥) + Y축 0.8f (가슴/머리) 쪽을 조준
            Vector3 targetPosition = player.position + new Vector3(0, 0.8f, 0);
            
            Vector2 dir = (targetPosition - firePoint.position).normalized;
            
            // 탄속도 7 -> 9로 살짝 빠르게
            FireBullet(dir, 9f); 
            
            // 연사 속도 더 빠르게 (0.1 -> 0.08)
            yield return new WaitForSeconds(0.08f); 
        }
    }

    // --------------------------------------------------------
    // ★ [수정됨] 패턴 2: 원형 방사 (3번 연속 발사)
    // --------------------------------------------------------
    IEnumerator Pattern_CircleFire()
    {
        Debug.Log("패턴 2: 원형 방사 3연격");
        
        // 3번 반복해서 쏨 (팡! ... 팡! ... 팡!)
        for (int wave = 0; wave < 3; wave++)
        {
            int bulletCount = 18; // 한 바퀴당 총알 수 증가 (12 -> 18)
            float angleStep = 360f / bulletCount;
            float startAngle = wave * 10f; // 쏠 때마다 각도를 살짝 비틀어서 쏘기 (피하기 어렵게)

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = startAngle + (i * angleStep);
                float dirX = Mathf.Cos(angle * Mathf.Deg2Rad);
                float dirY = Mathf.Sin(angle * Mathf.Deg2Rad);
                Vector2 dir = new Vector2(dirX, dirY).normalized;
                
                FireBullet(dir, 6f);
            }
            
            // 다음 파동까지 잠깐 대기
            yield return new WaitForSeconds(0.4f);
        }
    }

    // --------------------------------------------------------
    // ★ [수정됨] 패턴 3: 스파이럴 (훨씬 길게 쏘기)
    // --------------------------------------------------------
    IEnumerator Pattern_SpiralFire()
    {
        Debug.Log("패턴 3: 스파이럴 강화");
        float angle = 0f;

        // 20발 -> 60발로 대폭 증가 (오랫동안 회전하며 쏨)
        for (int i = 0; i < 60; i++)
        {
            float dirX = Mathf.Cos(angle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector2 dir = new Vector2(dirX, dirY).normalized;
            
            FireBullet(dir, 7f);
            
            angle += 15f; // 회전 각도
            yield return new WaitForSeconds(0.03f); // 발사 간격 더 촘촘하게
        }
    }

    // --- [패턴 4: 공중 폭격 (기존 유지)] ---
    IEnumerator Pattern_RainFire()
    {
        Debug.Log("패턴 4: 공중 폭격 개시!");

        if (rainSpot != null)
        {
            yield return StartCoroutine(MoveToSpot(rainSpot.position));
        }

        for (int i = 0; i < 30; i++)
        {
            float randomAngle = 270f + Random.Range(-45f, 45f);
            float dirX = Mathf.Cos(randomAngle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(randomAngle * Mathf.Deg2Rad);
            Vector2 dir = new Vector2(dirX, dirY).normalized;

            FireBullet(dir, 8f);
            yield return new WaitForSeconds(0.05f);
        }
    }

    void FireBullet(Vector2 direction, float speed)
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        Collider2D bulletCol = bullet.GetComponent<Collider2D>();
        if (myCollider != null && bulletCol != null)
            Physics2D.IgnoreCollision(myCollider, bulletCol);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = direction * speed;

        float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, rotZ);

        Projectile proj = bullet.GetComponent<Projectile>();
        if (proj != null) proj.isEnemyBullet = true; 
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("보스 체력: " + currentHealth + "/" + maxHealth);
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        StopAllCoroutines();
        
        // 카메라 원상복구
        if (camSwitcher != null) camSwitcher.SwitchToNormalCam();
        
        // 음악 복구
        if (musicManager != null) musicManager.PlayStageMusic();

        Debug.Log("보스 클리어!");
        
        // 엔딩 발판 생성
        if (endingPlatform != null)
        {
            endingPlatform.SetActive(true);
            Debug.Log("탈출구 생성!");
        }

        // 벽이 있다면 없애주기 (BossTrigger에서 넣은 bossWall 변수가 있다면 여기서 꺼주는 게 좋음)
        // (지금 코드엔 없어서 생략했지만, 필요하면 추가하세요)

        Destroy(gameObject);
    }
}