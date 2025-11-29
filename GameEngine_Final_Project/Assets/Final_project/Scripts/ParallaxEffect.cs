using UnityEngine;

public class InfiniteParallax : MonoBehaviour
{
    private GameObject cam;
    private float length, startpos;
    public float offsetY; 

    [Header("가로 패럴랙스 강도 (0~1)")]
    [SerializeField] public float parallaxEffect; 

    void Start()
    {
        cam = Camera.main.gameObject;
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        
        // 시작 시 카메라와 배경의 Y축 높이 차이를 저장
        offsetY = transform.position.y - cam.transform.position.y;
    }

    void FixedUpdate() 
    {
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        // 1. 배경 이동 (Y축은 카메라 고정)
        transform.position = new Vector3(startpos + dist, cam.transform.position.y, transform.position.z);

        // ★ 핵심 수정: 감지 범위를 1배가 아니라 1.5배로 늘립니다! ★
        // 이유: 점프를 3칸(3 length)씩 하니까, 돌아올 때 감지되지 않으려면 여유 공간이 필요함.
        
        float checkDist = length * 1.5f; // 감지 거리 확장

        if (temp > startpos + length) // 원래 코드대로라면 여기가 겹침 문제의 원인
        {
             // 여기 조건을 조금 더 여유롭게 잡아야 지터링이 안 생기지만, 
             // 가장 확실한 건 아래처럼 조건문 자체를 바꾸는 것입니다.
        }

        // ▼ 지터링 방지용 최종 로직 ▼
        if (temp > startpos + checkDist)
        {
            startpos += length * 3;
        }
        else if (temp < startpos - checkDist)
        {
            startpos -= length * 3;
        }
    }
}