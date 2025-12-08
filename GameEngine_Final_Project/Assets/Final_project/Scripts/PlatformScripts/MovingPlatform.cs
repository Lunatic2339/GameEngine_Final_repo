using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("이동 설정")]
    public Vector3 moveDistance = new Vector3(3f, 0f, 0f);
    public float moveSpeed = 2f;
    public float waitTime = 1f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isMovingToTarget = true;
    private float waitTimer = 0f;

    // ★ [추가] 발판의 실제 속도를 계산하기 위한 변수
    private Vector3 lastPos;
    private Vector2 currentVelocity;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + moveDistance;
        lastPos = transform.position;
    }

    void Update()
    {
        // 1. 실제 이동 로직 (기존과 동일)
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            currentVelocity = Vector2.zero; // 대기 중엔 속도 0
            return;
        }

        Vector3 destination = isMovingToTarget ? targetPos : startPos;
        
        // 이동하기 전 위치 저장
        Vector3 previousPosition = transform.position;

        // 이동 실행
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        // ★ [핵심] 이번 프레임에 얼마나 움직였는지 계산해서 속도(Velocity) 구하기
        // (이동한 거리 / 시간 = 속도)
        if (Time.deltaTime > 0)
        {
            currentVelocity = (transform.position - previousPosition) / Time.deltaTime;
        }

        // 도착 확인
        if (Vector3.Distance(transform.position, destination) < 0.01f)
        {
            isMovingToTarget = !isMovingToTarget;
            waitTimer = waitTime;
            currentVelocity = Vector2.zero;
        }
    }

    // ---------------------------------------------------------
    // ★ 수정된 충돌 로직 (SetParent 대신 속도 전달)
    // ---------------------------------------------------------

    // 플레이어가 닿아있는 동안 계속 속도를 전달해야 함 (Stay 사용)
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 확실한 윗면인지 체크
            if (collision.GetContact(0).normal.y < -0.7f)
            {
                PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    // ★ 플레이어에게 내 속도를 전달!
                    player.platformVelocity = currentVelocity;
                }
            }
        }
    }

    // 플레이어가 떨어지면 속도 전달 중지
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
                // ★ 발판 속도 제거 (안 그러면 계속 미끄러짐)
                player.platformVelocity = Vector2.zero;
            }
        }
    }
    
    void OnDrawGizmos()
    {
        Vector3 from = Application.isPlaying ? startPos : transform.position;
        Vector3 to = from + moveDistance;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawWireSphere(from, 0.2f);
        Gizmos.DrawWireSphere(to, 0.2f);
    }
}