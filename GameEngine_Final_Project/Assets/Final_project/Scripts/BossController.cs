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
    public Transform[] moveSpots; // 일반 이동 포인트들
    public Transform rainSpot;    // ★ [추가] 공중 폭격 시 이동할 위치 (천장 중앙)

    private Collider2D myCollider; 
    public BossCamSwitcher camSwitcher;
    [Header("클리어 보상")]
    public GameObject endingPlatform;

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
            // --- 패턴 랜덤 선택 (0 ~ 3) ---
            int randPattern = Random.Range(0, 4); 
            
            switch (randPattern)
            {
                case 0: // 조준 연사
                    yield return StartCoroutine(MoveToRandomSpot());
                    yield return StartCoroutine(Pattern_RapidFire()); 
                    break;
                case 1: // 원형 방사
                    yield return StartCoroutine(MoveToRandomSpot());
                    yield return StartCoroutine(Pattern_CircleFire()); 
                    break;
                case 2: // 스파이럴
                    yield return StartCoroutine(MoveToRandomSpot());
                    yield return StartCoroutine(Pattern_SpiralFire()); 
                    break;
                case 3: // ★ [신규 패턴] 공중 폭격
                    // 이 패턴은 랜덤 위치가 아니라 '지정된 공중 위치'로 가서 쏨
                    yield return StartCoroutine(Pattern_RainFire()); 
                    break;
            }

            yield return new WaitForSeconds(patternInterval);
        }
    }

    // 랜덤 이동 도우미 함수
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
        // 이동하는 동안 기다림
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // --------------------------------------------------------
    // ★ [추가된 패턴] 공중 폭격 (Rain Fire)
    // --------------------------------------------------------
    IEnumerator Pattern_RainFire()
    {
        Debug.Log("패턴 4: 공중 폭격 개시!");

        // 1. 지정된 공중 위치(Rain Spot)로 이동 (없으면 그냥 제자리)
        if (rainSpot != null)
        {
            yield return StartCoroutine(MoveToSpot(rainSpot.position));
        }

        // 2. 아래로 난사 (30발)
        for (int i = 0; i < 30; i++)
        {
            // 아래쪽(270도)을 기준으로 좌우 45도 사이 랜덤 각도
            float randomAngle = 270f + Random.Range(-45f, 45f);

            // 각도를 벡터로 변환
            float dirX = Mathf.Cos(randomAngle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(randomAngle * Mathf.Deg2Rad);
            Vector2 dir = new Vector2(dirX, dirY).normalized;

            // 평소보다 조금 빠른 속도(8f)로 발사
            FireBullet(dir, 8f);

            // 0.05초마다 발사 (아주 빠름)
            yield return new WaitForSeconds(0.05f);
        }
    }

    // --- [기존 패턴들] ---

    IEnumerator Pattern_RapidFire()
    {
        Debug.Log("패턴 1: 조준 연사");
        for (int i = 0; i < 10; i++)
        {
            if (player == null) break;
            Vector2 dir = (player.position - firePoint.position).normalized;
            FireBullet(dir, 7f);
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator Pattern_CircleFire()
    {
        Debug.Log("패턴 2: 원형 방사");
        int bulletCount = 12;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            float dirX = Mathf.Cos(angle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector2 dir = new Vector2(dirX, dirY).normalized;
            FireBullet(dir, 5f);
        }
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator Pattern_SpiralFire()
    {
        Debug.Log("패턴 3: 스파이럴");
        float angle = 0f;
        for (int i = 0; i < 20; i++)
        {
            float dirX = Mathf.Cos(angle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector2 dir = new Vector2(dirX, dirY).normalized;
            FireBullet(dir, 6f);
            angle += 15f;
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
        
// ★ [추가] 보스 죽으면 카메라 원상복구
        if (camSwitcher != null)
        {
            camSwitcher.SwitchToNormalCam();
        }

        Debug.Log("보스 클리어!");
        // ★ [추가] 엔딩 발판 활성화
        if (endingPlatform != null)
        {
            endingPlatform.SetActive(true);
            Debug.Log("탈출구 생성!");
        }
        Destroy(gameObject);
    }
}