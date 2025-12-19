using UnityEngine;

public class BossWall : MonoBehaviour
{
    [Header("설정")]
    public float moveSpeed = 10f;  // 문이 내려오는 속도
    public float openHeight = 5f;  // 위로 얼마나 올라가 있다가 내려올지 (거리)

    private Vector3 closedPosition; // 문이 닫혀야 할 원래 위치 (바닥)
    private Vector3 targetPosition;

    // 오브젝트가 SetActive(true) 될 때마다 실행됨
    void OnEnable()
    {
        // 1. 현재 위치(닫힌 상태의 위치)를 기억해둠
        closedPosition = transform.position;

        // 2. 문을 위쪽으로 순간이동 시킴 (열린 상태)
        transform.position = closedPosition + (Vector3.up * openHeight);

        // 3. 목표는 다시 원래 위치(closedPosition)로 가는 것
        targetPosition = closedPosition;
    }

    void Update()
    {
        // 현재 위치가 목표 위치와 다르면 계속 이동
        if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            // MoveTowards: 현재위치에서 목표위치로 일정 속도로 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
        else
        {
            // 거의 다 왔으면 정확하게 위치 고정 (떨림 방지)
            transform.position = targetPosition;
        }
    }
}