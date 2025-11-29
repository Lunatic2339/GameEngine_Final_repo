using UnityEngine;

public class InfiniteParallax : MonoBehaviour
{
    private GameObject cam;
    private float length, startpos;
    private float offsetY; // 카메라와 배경의 초기 Y축 간격 저장용

    [Header("가로 패럴랙스 강도 (0~1)")]
    [SerializeField] private float parallaxEffect; 
    // 1: 배경이 카메라랑 같이 움직임 (안 움직이는 것처럼 보임 - 먼 배경)
    // 0: 배경이 가만히 있음 (빠르게 지나감 - 가까운 배경)

    void Start()
    {
        cam = Camera.main.gameObject;
        
        // 1. 시작 X 위치 저장
        startpos = transform.position.x;
        
        // 2. 배경 이미지 길이 계산 (SpriteRenderer 필수)
        length = GetComponent<SpriteRenderer>().bounds.size.x;

        // 3. ★핵심★ 시작할 때 카메라와 배경의 높이 차이(Gap)를 기억해둠
        // 이렇게 해야 배경을 조금 위/아래로 배치했을 때 그 간격이 유지됨
        offsetY = transform.position.y - cam.transform.position.y;
    }

    void LateUpdate() // 카메라 이동 후 떨림 방지를 위해 LateUpdate 추천
    {
        // --- X축 (무한 스크롤 로직) ---
        float temp = (cam.transform.position.x * (1 - parallaxEffect)); // 재배치 판정용
        float dist = (cam.transform.position.x * parallaxEffect);       // 실제 이동 거리

        // --- Y축 (카메라 고정 로직) ---
        // 카메라의 Y 위치에 아까 구해둔 간격(offsetY)만 더해서 1:1로 따라감
        float lockedY = cam.transform.position.y + offsetY;

        // 최종 위치 적용
        transform.position = new Vector3(startpos + dist, lockedY, transform.position.z);

        // --- 무한 루프 (재배치) ---
        if (temp > startpos + length) startpos += length;
        else if (temp < startpos - length) startpos -= length;
    }
}