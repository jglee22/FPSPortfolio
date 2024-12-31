using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DamageOverlay : MonoBehaviour
{
    public Image borderImage;       // 테두리 이미지
    public float fadeSpeed = 5f;    // 페이드 속도
    public float damageDuration = 0.5f; // 피격 유지 시간
    public float maxAlpha = 0.8f;     // 최대 강도
    public float intensityIncrease = 0.1f; // 피격 시 강도 증가량


    private float currentAlpha = 0f; // 현재 알파값
    private float damageTimer = 0f; // 피격 유지 타이머
    void Update()
    {
        if (damageTimer > 0)
        {
            damageTimer -= Time.deltaTime;

            // 피격 강도 유지
            currentAlpha = Mathf.Clamp(currentAlpha, 0f, maxAlpha);
            borderImage.color = new Color(1, 0, 0, Mathf.Lerp(borderImage.color.a, currentAlpha, Time.deltaTime * fadeSpeed));
        }
        else
        {
            // 서서히 사라짐
            borderImage.color = new Color(1, 0, 0, Mathf.Lerp(borderImage.color.a, 0f, Time.deltaTime * fadeSpeed));
            currentAlpha = Mathf.Lerp(currentAlpha, 0f, Time.deltaTime * fadeSpeed); // 강도 감소
        }
    }

    public void ShowDamageEffect()
    {
        damageTimer = damageDuration; // 타이머 초기화
        currentAlpha += intensityIncrease; // 강도 누적 증가
    }
}
