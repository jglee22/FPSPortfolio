using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DamageOverlay : MonoBehaviour
{
    public Image borderImage;       // �׵θ� �̹���
    public float fadeSpeed = 5f;    // ���̵� �ӵ�
    public float damageDuration = 0.5f; // �ǰ� ���� �ð�
    public float maxAlpha = 0.8f;     // �ִ� ����
    public float intensityIncrease = 0.1f; // �ǰ� �� ���� ������


    private float currentAlpha = 0f; // ���� ���İ�
    private float damageTimer = 0f; // �ǰ� ���� Ÿ�̸�
    void Update()
    {
        if (damageTimer > 0)
        {
            damageTimer -= Time.deltaTime;

            // �ǰ� ���� ����
            currentAlpha = Mathf.Clamp(currentAlpha, 0f, maxAlpha);
            borderImage.color = new Color(1, 0, 0, Mathf.Lerp(borderImage.color.a, currentAlpha, Time.deltaTime * fadeSpeed));
        }
        else
        {
            // ������ �����
            borderImage.color = new Color(1, 0, 0, Mathf.Lerp(borderImage.color.a, 0f, Time.deltaTime * fadeSpeed));
            currentAlpha = Mathf.Lerp(currentAlpha, 0f, Time.deltaTime * fadeSpeed); // ���� ����
        }
    }

    public void ShowDamageEffect()
    {
        damageTimer = damageDuration; // Ÿ�̸� �ʱ�ȭ
        currentAlpha += intensityIncrease; // ���� ���� ����
    }
}
