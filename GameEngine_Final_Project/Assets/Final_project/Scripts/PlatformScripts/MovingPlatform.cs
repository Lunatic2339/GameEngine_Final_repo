using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("트리거 설정 (비워두면 자동 시작)")]
    public GameObject connectedEnemy; // ★ 여기에 몬스터를 드래그해서 넣으세요
    private bool isLocked = false;    // 내부적으로 사용하는 잠금 변수

    [Header("이동 설정")]
    public Vector3 moveDistance = new Vector3(3f, 0f, 0f);
    public float moveSpeed = 2f;
    public float waitTime = 1f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isMovingToTarget = true;
    private float waitTimer = 0f;

    // 발판의 실제 속도를 계산하기 위한 변수
    private Vector3 lastPos;
    private Vector2 currentVelocity;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + moveDistance;
        lastPos = transform.position;

        // ★ [핵심 로직 1] 시작할 때 적이 연결되어 있는지 확인
        if (connectedEnemy != null)
        {
            isLocked = true; // 적이 있으면 일단 잠금 (움직이지 마!)
        }
        else
        {
            isLocked = false; // 적이 없으면 바로 움직임
        }
    }

    void Update()
    {
        // ★ [핵심 로직 2] 잠겨있다면, 적이 죽었는지 확인
        if (isLocked)
        {
            // 연결된 적이 게임에서 사라졌다면 (죽었다면) -> null이 됨
            if (connectedEnemy == null)
            {
                isLocked = false; // 잠금 해제! 이제 움직여라
            }
            else
            {
                // 적이 아직 살아있으면 움직이지 않고 리턴
                currentVelocity = Vector2.zero;
                return; 
            }
        }

        // --- 아래는 기존 이동 로직과 동일 ---

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

        // 이번 프레임에 얼마나 움직였는지 계산해서 속도(Velocity) 구하기
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
    // 충돌 로직 (그대로 유지)
    // ---------------------------------------------------------

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
                    player.platformVelocity = currentVelocity;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
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