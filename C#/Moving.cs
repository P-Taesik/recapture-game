using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moving : MonoBehaviour
{
    public float speed = 5f; // 플레이어 이동 속도

    // Updaqte is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // 좌우 이동 입력값
        float verticalInput = Input.GetAxis("Vertical"); // 상하 이동 입력값

        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f); // 이동 벡터 생성
        transform.position += movement * speed * Time.deltaTime; // 이동 벡터에 이동 속도와 시간을 곱하여 이동
    }
}

