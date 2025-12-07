using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    [Header("함정 설정")]
    public Vector3 spikeOffset = new Vector3(0f, 1f, 0f); // 튀어나올 방향과 거리 (Y:1이면 위로 1칸)
    public float extendSpeed = 10f;  // 찌르는 속도 (빠를수록 푹! 찌름)
    public float retractSpeed = 3f;  // 들어가는 속도 (천천히 복귀)
    public int damage = 1;           // 데미지

    [Header("타이밍 설정")]
    public float startDelay = 0f;    // 엇박자용 시작 대기
    public float activeTime = 1f;    // 튀어나와서 머무는 시간
    public float inactiveTime = 2f;  // 들어가서 쉬는 시간

    private Vector3 hiddenPos; // 숨은 위치 (원래 위치)
    private Vector3 targetPos; // 튀어나온 위치

    void Start()
    {
        hiddenPos = transform.position;
        targetPos = hiddenPos + spikeOffset;

        // 함정 루프 시작
        StartCoroutine(TrapRoutine());
    }

    IEnumerator TrapRoutine()
    {
        // 1. 시작 지연 (엇박자)
        if (startDelay > 0)
            yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // === [휴식] 숨어서 대기 ===
            yield return new WaitForSeconds(inactiveTime);

            // === [공격] 푹! 찌르기 (나오는 동작) ===
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, extendSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos; // 위치 보정

            // === [대기] 찌른 상태로 유지 ===
            yield return new WaitForSeconds(activeTime);

            // === [복귀] 스르륵 들어가기 ===
            while (Vector3.Distance(transform.position, hiddenPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, hiddenPos, retractSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = hiddenPos; // 위치 보정
        }
    }

    // ★ 데미지 처리 (플레이어가 닿으면)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null)
            {
                // 데미지 + 넉백(함정 위치 기준)
                player.TakeDamage(damage, transform);
            }
        }
    }
}