using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeartSystem : MonoBehaviour
{
    [Header("참조")]
    public PlayerHealth playerHealth; // 인스펙터 연결도 유지하되, 코드로도 찾음

    [Header("UI 설정")]
    public GameObject heartPrefab;
    public Transform heartContainer;
    
    [Header("스프라이트")]
    public Sprite fullHeart;
    public Sprite emptyHeart;

    private List<Image> hearts = new List<Image>();

    void Start()
    {
        // ★ [핵심 수정] 게임이 재시작되면 플레이어도 새로 태어납니다.
        // 그러므로 스크립트가 시작될 때 "지금 씬에 있는 플레이어"를 다시 찾아야 합니다.
        if (playerHealth == null)
        {
            // 방법 1: PlayerHealth 타입을 가진 오브젝트 찾기
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            
            // 방법 2: 태그로 찾기 (방법 1이 안 되면 주석 풀고 이거 쓰세요)
            // playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        }

        // --- 기존 로직 ---

        // 1. 하트 아이콘 생성 (기존 하트가 있다면 지우고 다시 생성하는 게 안전)
        // (하트 컨테이너에 이미 자식들이 있다면 싹 지우기 - 재시작 버그 방지)
        foreach (Transform child in heartContainer)
        {
            Destroy(child.gameObject);
        }
        hearts.Clear(); // 리스트 초기화

        // 새로 생성
        for (int i = 0; i < playerHealth.maxLives; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartContainer);
            hearts.Add(newHeart.GetComponent<Image>());
        }

        // 2. 이벤트 구독 (새로운 플레이어에게 연결)
        playerHealth.onHealthChanged += UpdateHearts;

        // 3. ★ 강제 업데이트! (시작하자마자 현재 체력으로 그림 그리기)
        UpdateHearts();
    }

    void UpdateHearts()
    {
        // ... (나머지 코드는 기존과 동일) ...
        int currentLives = playerHealth.currentLives;

        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < currentLives)
            {
                hearts[i].sprite = fullHeart;
                hearts[i].enabled = true;
            }
            else
            {
                if (emptyHeart != null) hearts[i].sprite = emptyHeart;
                else hearts[i].enabled = false;
            }
        }
    }
}