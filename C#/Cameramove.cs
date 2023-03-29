using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cameramove : MonoBehaviour
{
    public Transform target; // 캐릭터(타겟)의 Transform 컴포넌트
    public float smoothSpeed = 0.125f; // 카메라 이동의 부드러움 정도
    public Vector3 offset; // 타겟과 카메라 간의 오프셋

    public Vector2 minPosition; // 카메라가 이동할 수 있는 최소 위치
    public Vector2 maxPosition; // 카메라가 이동할 수 있는 최대 위치

    public float shakeDuration = 0.01f; // 화면 흔들림 효과의 지속시간
    public float shakeMagnitude = 0.1f; // 화면 흔들림 효과의 세기

    private Vector3 originalPosition; // 카메라 원래 위치 저장 변수

    void LateUpdate()
    {
        // 타겟의 위치와 오프셋을 더한 원하는 카메라 위치 계산 (Z 좌표는 그대로 유지)
        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);

        // 카메라 위치를 원하는 위치로 부드럽게 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 카메라 이동 범위를 제한
        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minPosition.x, maxPosition.x);
        smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minPosition.y, maxPosition.y);

        // 실제 카메라 위치 갱신
        transform.position = smoothedPosition;
    }

    public void Shake()
    {
        originalPosition = transform.position;
        StartCoroutine(DoShake());
    }

    IEnumerator DoShake()
    {
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            Vector3 shakeOffset = Random.insideUnitCircle * shakeMagnitude;
            transform.position = originalPosition + shakeOffset;

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.position = originalPosition;
    }
}
