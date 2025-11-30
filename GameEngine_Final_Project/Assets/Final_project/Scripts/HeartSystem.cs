using UnityEngine;
using UnityEngine.UI; // UI 건드릴 때 필수
using System.Collections.Generic;

public class HeartSystem : MonoBehaviour
{
    [Header("참조")]
    public PlayerHealth playerHealth; // 플레이어 연결

    [Header("UI 설정")]
    public GameObject heartPrefab;    // 핏방울 아이콘 프리팹 (Image)
    public Transform heartContainer;  // 아이콘들이 들어갈 부모 (Panel)
    
    [Header("스프라이트")]
    public Sprite fullHeart;  // 꽉 찬 이미지
    public Sprite emptyHeart; // 빈 이미지 (없으면 안 써도 됨)

    private List<Image> hearts = new List<Image>();

    void Start()
    {
        // 1. 최대 목숨 개수만큼 아이콘 생성 (초기 세팅)
        for (int i = 0; i < playerHealth.maxLives; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartContainer);
            hearts.Add(newHeart.GetComponent<Image>());
        }

        // 2. 플레이어 체력이 변할 때마다 UpdateHearts 함수가 실행되도록 구독(연결)
        playerHealth.onHealthChanged += UpdateHearts;

        // 3. 처음에 한 번 그려주기
        UpdateHearts();
    }

    void UpdateHearts()
    {
        // 현재 체력 가져오기
        int currentLives = playerHealth.currentLives;

        for (int i = 0; i < hearts.Count; i++)
        {
            // 인덱스(i)가 현재 체력보다 작으면 -> 꽉 찬 하트
            if (i < currentLives)
            {
                hearts[i].sprite = fullHeart;
                hearts[i].enabled = true; // 보이게
            }
            // 인덱스(i)가 현재 체력보다 크거나 같으면 -> 빈 하트 (또는 안 보이게)
            else
            {
                if (emptyHeart != null)
                {
                    hearts[i].sprite = emptyHeart; // 빈 이미지로 교체
                }
                else
                {
                    hearts[i].enabled = false; // 빈 이미지가 없으면 그냥 끄기
                }
            }
        }
    }
}