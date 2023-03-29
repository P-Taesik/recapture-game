using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Knight : MonoBehaviour
{
    public float minDistanceBetweenAreas = 0.5f;
    public float attackAreaRadius = 0.5f;

    public float attackAreaDuration = 0.5f; // 공격범위 지속시간
    public float attackAreaExplosionDuration = 0.5f; // 폭발 이펙트 지속시간
    public GameObject attackAreaExplosionPrefab; // 폭발 이펙트 프리팹
    public float skillInterval = 30f; //스킬1 재사용시간
    private bool usingSkill = false; // 스킬 사용 여부를 추적하는 변수를 추가합니다.
    private bool usingSkill2 = false; // 스킬2 사용 여부를 추적하는 변수를 추가합니다.
    private bool skill2 = false;
    public LayerMask playerLayer; // 감지할 레이어를 설정합니다.
    public float detectionRadius = 2f; // 감지 범위를 설정합니다.
    public AnimationClip attack1AnimationClip;
    private IEnumerator DamageCoroutine;
    private float attackDuration = 0.3f;
    private bool canUseSecondSkill = false; // 두 번째 스킬 사용 가능 여부를 확인하는 변수
    public GameObject deadZone;
    private SkeletonAnimation skeletonAnimation;
    public Transform character;
    public float followSpeed = 5f;
    private bool isDead = false;
    public int bossHp = 100; // 보스 체력을 설정합니다.
    public GameObject attackAreaPrefab; // 원형 범위 스킬 프리팹을 추가합니다.
    public float explosionWaitDuration = 1f; // 폭발 이펙트 대기 시간
    private bool canAttack = true;
    private float attackCooldown = 3f;
    private Coroutine attackCoroutine;
    public Canvas canvas;
    public Image[] images; // 데드존을 구성하는 이미지들
    public float fadeInSpeed = 0.5f; // 이미지가 나타나는 속도
    public Image[] deadZoneImages;

    private bool isFadingIn = false; // 이미지가 나타나는 중인지 여부

    private void Start()
    {
        StartCoroutine(UseSkill());
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        SetAnimation("standing", true);
        deadZone.SetActive(false);
        foreach (Image image in images)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0); // 알파값을 0으로 초기화합니다.
        }
        // Canvas 오브젝트를 찾아서 Canvas 컴포넌트를 가져옵니다.
        canvas = FindObjectOfType<Canvas>();
    }





    // -----------------------------
    // 보스몹 피해처리 및 사망
    // -----------------------------
    #region 보스몹 피해처리 및 사망
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDead && collision.gameObject.CompareTag("arrow")) // 화살과 충돌한 경우
        {
            TakeDamage(10);
            Destroy(collision.gameObject); // 충돌한 화살을 제거합니다.
        }
    }
    void NextScene()
    {
        // 보스가 죽기 전까지는 움직임을 멈춥니다.
        character = null;
        skeletonAnimation.AnimationState.SetAnimation(0, "dead", false);

        // 천천히 화면이 검은색으로 변하는 코루틴을 시작합니다.
        StartCoroutine(FadeToBlack(2f));
    }
    IEnumerator FadeToBlack(float duration)
    {
        // 검은색 배경 이미지를 가지고 있는 패널 오브젝트를 찾습니다.
        GameObject panel = canvas.transform.Find("Panel").gameObject;

        // 패널 오브젝트에 붙어있는 CanvasGroup 컴포넌트를 가져옵니다.
        CanvasGroup panelCanvasGroup = panel.GetComponent<CanvasGroup>();

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / duration);

            // 검은색 배경을 가진 패널 오브젝트의 alpha 값을 조절합니다.
            panelCanvasGroup.alpha = alpha;

            yield return null;
        }

        // 코루틴이 끝난 후 Store 씬으로 이동합니다.
        SceneManager.LoadScene("Store");
    }






    public void TakeDamage(int damage)
    {
        if (!isDead)
        {
            bossHp -= damage;

            if (bossHp <= 30 && !usingSkill2)
            {
                StartCoroutine(UseSkill2()); // 두 번째 스킬 사용 시작
            }
            else if (usingSkill2 && bossHp > 30)
            {
                usingSkill2 = false;
            }
            if (bossHp <= 0)
            {
                Debug.Log("Boss1 died.");
                PlayDeadAnimation();
                isDead = true;

                Collider2D collider = GetComponent<Collider2D>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                // 보스가 죽은 후에는 공격 불가능 상태로 설정합니다.
                canAttack = false;
                // 보스가 죽은 후에는 코루틴을 중지합니다.
                StopAllCoroutines();

                // 보스가 죽었을 때 NextScene 함수를 호출합니다.
                StartCoroutine(DelayedNextScene(1f));
            }
            else
            {
                StartCoroutine(BlinkSkeleton());
            }
        }
    }

    IEnumerator DelayedNextScene(float delayTime)
    {
        yield return new WaitForSeconds(2f);
        NextScene();
    }






    void PlayDeadAnimation()
    {
        skeletonAnimation.AnimationState.SetAnimation(0, "dead", false);
    }
    #endregion



    // -----------------------------
    // 평타
    // -----------------------------
    #region 평타
    private void DoAttack()
    {
        if (usingSkill2)
        {
            return;
        }
        SetAnimation("Attack_1", false);
        StartCoroutine(WaitForAttackArea(1f));
        float attackAnimationLength = GetSpineAnimationLength("Attack_1");
        StartCoroutine(WaitForAnimationEnd(attackAnimationLength));
    }

    private IEnumerator WaitForAnimationEnd(float time)
    {
        yield return new WaitForSeconds(time);
        OnAttackAnimationEnd();
    }
    private float GetSpineAnimationLength(string animationName)
    {
        Spine.Animation animation = skeletonAnimation.Skeleton.Data.FindAnimation(animationName);
        if (animation != null)
        {
            return animation.Duration;
        }
        return 0;
    }


    private IEnumerator WaitForAttackArea(float time)
    {
        yield return new WaitForSeconds(time);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackAreaRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                Vector2 directionToPlayer = (collider.transform.position - transform.position).normalized;
                float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

                if (angleToPlayer >= 0 && angleToPlayer <= 180)
                {
                    Player player = collider.gameObject.GetComponent<Player>();
                    if (player != null)
                    {
                        player.TakeDamage(10);
                    }
                }
            }
        }
    }



    // Attack_1 애니메이션이 끝났을 때 호출되는 이벤트 함수
    public void OnAttackAnimationEnd()
    {
        SetAnimation("walking", true);
        StartCoroutine(AttackCooldown());
    }

    // 평타 쿨다운을 처리하는 코루틴
    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    private Vector2 CalculateArcPoint(float angle, float radius)
    {
        float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        return new Vector2(x, y);
    }

    #endregion


    // -----------------------------
    // 보스 1번 스킬사용
    // -----------------------------
    #region 보스 1번 스킬사용
IEnumerator UseSkill()
{
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(skillInterval);

            // 스킬2 사용 중이 아닐 때만 스킬1을 사용하도록 합니다.
            if (!usingSkill2 && !isDead)
            {
                usingSkill = true;

                SetAnimation("skill", false);
                // 카메라 흔들림 효과
                StartCoroutine(ShakeCameraOnSkillUse());

                yield return new WaitForSeconds(skeletonAnimation.AnimationState.GetCurrent(0).Animation.Duration);

                CreateAttackAreas(10);

                yield return new WaitForSeconds(attackAreaDuration + attackAreaExplosionDuration);

                usingSkill = false;
            }
        }
    }
}

private IEnumerator ShakeCameraOnSkillUse()
{
    yield return new WaitForSeconds(1.1f);
    // 카메라 흔들림 효과
    Camera.main.GetComponent<Cameramove>().Shake();
}




    void CreateAttackAreas(int count)
    {
        List<GameObject> attackAreas = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPosition = GetRandomPosition(-5.95f, 2.39f, -4.94f, 3.3f);
            GameObject attackArea = Instantiate(attackAreaPrefab, randomPosition, Quaternion.identity);
            attackAreas.Add(attackArea);
        }

        StartCoroutine(ExpandAndDestroyAttackAreas(attackAreas));
    }

    Vector3 GetRandomPosition(float minX, float maxX, float minY, float maxY)
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        return new Vector3(randomX, randomY, 0f);
    }

    IEnumerator ExpandAndDestroyAttackAreas(List<GameObject> attackAreas)
    {
        float expandSpeed = 2f;
        float maxScale = 2f;

        float timer = 0f;
        while (timer < attackAreaDuration)
        {
            timer += Time.deltaTime;
            foreach (GameObject attackArea in attackAreas)
            {
                float currentScale = Mathf.Min(maxScale, attackArea.transform.localScale.x + expandSpeed * Time.deltaTime);
                attackArea.transform.localScale = new Vector3(currentScale, currentScale, 1f);
            }
            yield return null;
        }

        // 최대 범위에서 1초 동안 대기
        yield return new WaitForSeconds(1f);

        foreach (GameObject attackArea in attackAreas)
        {
            Destroy(attackArea);
            CreateExplosionEffect(attackArea.transform.position);
        }
    }

    void CreateExplosionEffect(Vector3 position)
    {
        GameObject explosion = Instantiate(attackAreaExplosionPrefab, position, Quaternion.identity);
        StartCoroutine(ExpandAndDestroyExplosionEffect(explosion));
    }

    IEnumerator ExpandAndDestroyExplosionEffect(GameObject explosion)
    {
        float expandSpeed = 3f;
        float maxScale = 0.5f;
        float currentScale = 0.1f;

        explosion.transform.localScale = new Vector3(currentScale, currentScale, 1f);
        while (currentScale < maxScale)
        {
            currentScale += expandSpeed * Time.deltaTime;
            explosion.transform.localScale = new Vector3(currentScale, currentScale, 1f);
            yield return null;
        }
        // 폭발 이미지가 최대크기 상태에서 0.5초간 유지
        yield return new WaitForSeconds(0.2f);

        Destroy(explosion);
    }

    #endregion
    // -----------------------------
    // 보스 2번 스킬사용
    // -----------------------------
    #region 보스 2번 스킬사용

    IEnumerator UseSkill2()
    {
        if (!isDead && !usingSkill2)
        {
            usingSkill2 = true;

            SetAnimation("Attack_2", false);
            yield return new WaitForSeconds(skeletonAnimation.AnimationState.GetCurrent(0).Animation.Duration);

            // 스킬2 사용 후 usingSkill2 변수를 true로 설정하여 더 이상 스킬2를 사용하지 못하도록 합니다.
            usingSkill2 = true;

            deadZone.SetActive(true);

            // 데드존 이미지 점점 나타나게 하기
            StartCoroutine(FadeInImages());

            // 스킬 2 사용이 끝나면 usingSkill2 변수를 다시 false로 설정합니다.
            usingSkill2 = false;
        }
    }





    public void Show()
    {
        isFadingIn = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator FadeInImages()
    {
        float alpha = 0f;
        float fadeSpeed = 1f;

        foreach (Transform child in deadZone.transform)
        {
            if (child.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
            }
        }

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;

            foreach (Transform child in deadZone.transform)
            {
                if (child.TryGetComponent(out SpriteRenderer spriteRenderer))
                {
                    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
                }
            }

            yield return null;
        }
    }


    #endregion

    // -----------------------------
    // 이동 및 애니메이션
    // -----------------------------
    #region 이동 및 애니메이션
    private void Update()
    {

        if (!isDead && !usingSkill && !usingSkill2)
        {
            if (character != null)
            {
                Vector3 targetPosition = new Vector3(character.position.x, character.position.y, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

                Vector2 direction = (targetPosition - transform.position).normalized;
                FlipSprite(direction);

                var state = skeletonAnimation.AnimationState.GetCurrent(0);
                string currentAnimationName = state.Animation.Name;

                // 플레이어와의 거리를 계산합니다.
                float distanceToPlayer = Vector3.Distance(transform.position, character.position);

                // 플레이어가 원형 감지 범위 내에 있을 때만 공격 애니메이션을 실행합니다.
                if (distanceToPlayer <= detectionRadius)
                {
                    if (canAttack && currentAnimationName != "Attack_1" && !usingSkill2)
                    {
                        canAttack = false;
                        DoAttack();
                    }
                }
                else if (currentAnimationName != "walking" && currentAnimationName != "Attack_1")
                {
                    SetAnimation("walking", true);
                }
            }
        }
    }




    private IEnumerator BlinkSkeleton()
    {
        for (int i = 0; i < 3; i++)
        {
            skeletonAnimation.skeleton.SetColor(Color.red);
            yield return new WaitForSeconds(0.1f);
            skeletonAnimation.skeleton.SetColor(Color.white);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void SetAnimation(string animationName, bool loop)
    {
        skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
    }

    private void FlipSprite(Vector2 direction)
    {
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
    #endregion

}