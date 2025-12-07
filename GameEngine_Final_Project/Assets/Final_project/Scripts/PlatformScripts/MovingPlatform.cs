using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("이동 설정")]
    public Vector3 moveDistance = new Vector3(3f, 0f, 0f); // 이동할 거리 (X, Y, Z)
    public float moveSpeed = 2f;      // 이동 속도
    public float waitTime = 1f;       // 끝에 도달했을 때 대기 시간

    [Header("상태 (자동 계산)")]
    private Vector3 startPos;         // 시작 위치
    private Vector3 targetPos;        // 목표 위치
    private bool isMovingToTarget = true; // 정방향 이동 중인지?
    private float waitTimer = 0f;     // 대기 시간 타이머

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + moveDistance;
    }

    void Update()
    {
        // 1. 대기 시간 체크
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        // 2. 목표 지점 설정 (왔다 갔다)
        Vector3 destination = isMovingToTarget ? targetPos : startPos;

        // 3. 이동 (MoveTowards 사용)
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        // 4. 도착 확인
        if (Vector3.Distance(transform.position, destination) < 0.01f)
        {
            // 방향 뒤집기
            isMovingToTarget = !isMovingToTarget;
            // 대기 시간 설정
            waitTimer = waitTime;
        }
    }

    // ---------------------------------------------------------
    // ★ 핵심 기능: 플레이어가 탔을 때 같이 움직이게 하기
    // ---------------------------------------------------------
    
    // 플레이어가 발판 위에 닿으면 -> 플레이어를 발판의 자식으로 만듦
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 플레이어의 발 밑에서 닿았을 때만 태우기 (옆에서 닿으면 밀려나야 함)
        if (collision.gameObject.CompareTag("Player"))
        {
            // 0.7f 이상이면 확실한 윗면 (플레이어 이동 코드와 기준 통일)
            if (collision.GetContact(0).normal.y < -0.7f) 
            {
                collision.transform.SetParent(transform);
            }
        }
    }

    // 플레이어가 발판에서 떨어지면 -> 자식 해제 (독립)
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
            
            // 주의: DontDestroyOnLoad가 적용된 씬 구조라면 
            // null 대신 원래 부모나 씬 루트로 보내야 할 수도 있음.
            // 보통은 null로 하면 최상위로 가서 해결됨.
        }
    }

    // 에디터에서 이동 경로 미리보기 (초록색 선)
    void OnDrawGizmos()
    {
        // 플레이 모드가 아닐 때는 현재 위치 기준으로 계산
        Vector3 from = Application.isPlaying ? startPos : transform.position;
        Vector3 to = from + moveDistance;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawWireSphere(from, 0.2f);
        Gizmos.DrawWireSphere(to, 0.2f);
    }
}