using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public Button attackButton; // 칼 공격 버튼

    private bool isAttacking = false; // 칼 공격 여부
    public int hp = 10; //hp
    public float attackRange = 4.0f;

    public int attackDamage = 100;
    public bl_Joystick moveJoystick;//조이스틱 할당
    private bool _isAttackAnimationEnabled = true;//공격 애니메이션 확인
    public bl_Joystick js;//공격 조이스틱
    public float speed;//이동속도
    private SkeletonAnimation skeletonAnimation;//스파인 애니메이션
    public Vector2 minPosition;//이동범위 (최소)
    public Vector2 maxPosition;//이동범위 (최대)
    public bool isShooting = false;//플레이어가 활 공격중인지 확인
    public Button weaponChangeButton; // 무기 변경 버튼

    private void Start()
    {
        attackButton.onClick.AddListener(OnAttackButtonClicked);
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        weaponChangeButton = GameObject.Find("ChangeWeapon").GetComponent<Button>();

    }

    public void SetAttackAnimationEnabled(bool isEnabled)
    {
        _isAttackAnimationEnabled = isEnabled;
        SetWeaponChangeButtonEnabled(isEnabled);
    }


    void OnAttackButtonClicked()
    {
        // 칼 공격 버튼이 클릭되면 isAttacking 변수를 true로 설정
        isAttacking = true;
    }
    public void Attack()
    {
        Vector2 origin = transform.position;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin, attackRange);

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.GetComponent<Knight>() != null)
            {
                Debug.Log("Boss found");
                hitCollider.GetComponent<Knight>().TakeDamage(attackDamage);
            }
        }


        if (_isAttackAnimationEnabled)
        {
            if (WeaponAttackController.weaponType == WeaponAttackController.WeaponType.Sword)
            {
                // Sword Attack 애니메이션 재생
                skeletonAnimation.AnimationState.SetAnimation(0, "attack", false).TimeScale = 1.5f;
                _isAttackAnimationEnabled = false;

                // 무기 변경 버튼 비활성화
                SetWeaponChangeButtonEnabled(false);
            }
        }

    }


    public void OnAttackAnimationEnd()
    {
        if (WeaponAttackController.weaponType == WeaponAttackController.WeaponType.Sword)
        {
            _isAttackAnimationEnabled = true;

            // 무기 변경 버튼 활성화
            SetWeaponChangeButtonEnabled(true);

            if (js.Horizontal == 0 && js.Vertical == 0)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, "victory", true);
            }
            else
            {
                skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        float angleStep = 10f;
        float startAngle = -90f;
        Gizmos.color = Color.red;

        Vector3 previousPoint = origin;
        for (float angle = startAngle; angle <= startAngle + 180f; angle += angleStep)
        {
            Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * attackRange;
            Vector3 currentPoint = origin + direction;
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }
    public void SetWeaponChangeButtonEnabled(bool isEnabled)
    {
        weaponChangeButton.interactable = isEnabled;
    }

    // -----------------------------
    // 보스몹 충돌처리
    // -----------------------------
    #region 보스몹 충돌 처리
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("AttackArea") | collision.gameObject.CompareTag("Boss1")) // 원형 범위와 충돌한 경우
        {
            TakeDamage(10);
        }
        if (collision.gameObject.CompareTag("Boss1") | collision.gameObject.CompareTag("Boss1")) // 원형 범위와 충돌한 경우
        {
            TakeDamage(10);
        }
    }

    #endregion
public void TakeDamage(int damage)
{
    hp -= damage;
    if (hp <= 0)
    {
        Debug.Log("Player died.");
        // 죽는 애니메이션(die)을 재생하고, 다른 동작과 애니메이션을 중지합니다.
        skeletonAnimation.AnimationState.SetAnimation(0, "die", false);
        skeletonAnimation.timeScale = 1.0f; // 애니메이션 재생 속도를 원래대로 설정합니다.
        _isAttackAnimationEnabled = false; // 공격 애니메이션 재생 여부를 false로 설정합니다.
        moveJoystick.gameObject.SetActive(false); // 이동 조이스틱을 비활성화합니다.
        js.gameObject.SetActive(false); // 공격 조이스틱을 비활성화합니다.
        isShooting = false;
        js.gameObject.SetActive(false); // 활 조이스틱 비활성화

        SetWeaponAndAttackButtonEnabled(false); // 무기 변경 버튼과 칼 공격 버튼을 둘 다 비활성화합니다.

        // 3초 후에 Main 씬으로 이동합니다.
        StartCoroutine(GoToMainSceneCoroutine());
    }
}

IEnumerator GoToMainSceneCoroutine()
{
    yield return new WaitForSeconds(3.0f);
    SceneManager.LoadScene("Main");
}



public void SetAttackButtonEnabled(bool isEnabled)
{
    attackButton.interactable = isEnabled;
}

public void SetWeaponAndAttackButtonEnabled(bool isEnabled)
{
    SetWeaponChangeButtonEnabled(isEnabled);
    SetAttackButtonEnabled(isEnabled);
}



    // -----------------------------
    // 이동 및 애니메이션
    // -----------------------------
    #region 이동 및 애니메이션
    void Update()
    {
        Vector3 moveDirection = new Vector3(moveJoystick.Horizontal, moveJoystick.Vertical, 0);
        Vector3 dir = new Vector3(js.Horizontal, js.Vertical, 0);
        dir.Normalize();

        if (!isShooting)
        {
            transform.position += dir * speed * Time.deltaTime;
        }

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);
        transform.position = pos;

        var state = skeletonAnimation.AnimationState.GetCurrent(0);

        if (_isAttackAnimationEnabled)
        {
            if (WeaponAttackController.weaponType == WeaponAttackController.WeaponType.Bow)
            {
                if (dir.magnitude == 0f)
                {
                    if (state == null || state.Animation.Name != "victory")
                    {
                        // idle 애니메이션 재생
                        skeletonAnimation.AnimationState.SetAnimation(0, "victory", true);
                    }
                }
                else
                {
                    if (state == null || state.Animation.Name != "arrow run")
                    {
                        // Arrow run 애니메이션 재생
                        skeletonAnimation.AnimationState.SetAnimation(0, "arrow run", true);
                    }
                }
            }
            else // Sword 인 경우
            {
                if (dir.magnitude == 0f)
                {
                    if (state == null || state.Animation.Name != "victory")
                    {
                        skeletonAnimation.AnimationState.SetAnimation(0, "victory", true);
                    }
                }
                else
                {
                    if (state == null || state.Animation.Name != "run")
                    {
                        skeletonAnimation.AnimationState.SetAnimation(0, "run", true);
                    }
                }

                if (isAttacking)
                {
                    Attack();
                    isAttacking = false;
                }
            }
        }
        else
        {
            if (state != null && (state.Animation.Name == "attack" || state.Animation.Name == "arrow run") && state.IsComplete)
            {
                OnAttackAnimationEnd();
            }
        }

        if (dir.magnitude > 0f && !isShooting)
        {
            skeletonAnimation.Skeleton.ScaleX = Mathf.Sign(js.Horizontal);
        }
    }

    #endregion
}
