using UnityEngine;
using System.Collections;

public class WaterDeath : MonoBehaviour
{
    [Header("설정 값")]
    public float sinkSpeed = 1.0f;   // 가라앉는 속도
    public float deathDelay = 2.0f;  // 죽기 전까지 걸리는 시간
    public string waterTag = "Water"; // 물 태그

    private Rigidbody2D rb;
    private PlayerHealth playerHealth; // PlayerHealth 스크립트 가져오기용 변수
    private bool isDying = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 같은 오브젝트에 있는 PlayerHealth 스크립트를 찾아옵니다.
        playerHealth = GetComponent<PlayerHealth>(); 
        
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth 스크립트가 플레이어에 없습니다!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isDying && collision.CompareTag(waterTag))
        {
            StartCoroutine(SinkAndDie());
        }
    }

    IEnumerator SinkAndDie()
    {
        isDying = true;

        // [중요] 플레이어의 이동 스크립트 이름을 'PlayerMovement'라고 가정했습니다.
        // 실제 사용하시는 이동 스크립트 이름으로 바꿔주세요! (예: Movement, PlayerMove 등)
        var controller = GetComponent<PlayerMovement>(); 
        if (controller != null) controller.enabled = false; // 키보드 입력 차단

        // 물리 효과 변경 (천천히 가라앉기)
        rb.linearVelocity = Vector2.zero; 
        rb.gravityScale = 0f;       

        float timer = 0f;
        while (timer < deathDelay)
        {
            // 강제로 아래로 천천히 밈
            rb.linearVelocity = new Vector2(0, -sinkSpeed);
            timer += Time.deltaTime;
            yield return null;
        }

        // 기존 PlayerHealth에 있는 Die 함수 실행
        if (playerHealth != null)
        {
            playerHealth.Die(); 
        }
    }
}