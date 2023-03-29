using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Spine.Unity;
using UnityEngine.UI;

public class BowJoystickController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bl_Joystick attackJoystick;
    public bool isJoystickPressed = false;
    public Player playerScript;
    public GameObject joystick;
    public LineRenderer arrowTrajectory;
    public GameObject arrowPrefab;
    public Transform mainCharacter;
    public float maxDistance = 100.0f;
    public float minDistance = 10.0f;
    public float arrowSpeed = 10.0f;
    private Vector2 _joystickCenter;
    private Vector2 _startPos;
    private Vector2 _endPos;
    private bool _canShoot = true;
    private SkeletonAnimation _mainCharacterAnimation;
    private SkeletonAnimation _skeletonAnimation;
    private bl_Joystick _joystick;
    public bool CanMove { get; private set; } = true;
    private Transform _center;
    public Transform hand;


    private void Start()
    {
        _center = transform.Find("Center");
        _center.gameObject.SetActive(false);
        _joystickCenter = joystick.GetComponent<RectTransform>().anchoredPosition;

        _mainCharacterAnimation = mainCharacter.GetComponent<SkeletonAnimation>();
        _joystick = joystick.GetComponent<bl_Joystick>();
        _skeletonAnimation = mainCharacter.GetComponent<SkeletonAnimation>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _startPos = eventData.position;
        isJoystickPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _endPos = eventData.position;
        isJoystickPressed = false;

        float distance = Vector2.Distance(_startPos, _endPos);

        if (distance >= minDistance)
        {
            Vector2 direction = (_startPos - _endPos).normalized;
            arrowTrajectory.SetPosition(0, Vector3.zero);
            arrowTrajectory.SetPosition(1, Vector3.zero);

            StartCoroutine(ShootArrow(direction));
        }
    }

    private IEnumerator ShootArrow(Vector2 direction)
    {
        if (_canShoot)
        {
            _canShoot = false; // 발사 중에는 false로 설정합니다.
            playerScript.isShooting = true;

            float animationSpeed = 2.0f;
            playerScript.SetAttackAnimationEnabled(false);
            _skeletonAnimation.Skeleton.ScaleX = Mathf.Sign(direction.x);

            _mainCharacterAnimation.AnimationState.SetAnimation(0, "arrow", false).TimeScale = animationSpeed;
            StartCoroutine(shotthearrow(direction));
            yield return new WaitForSeconds(_mainCharacterAnimation.AnimationState.GetCurrent(0).Animation.Duration / animationSpeed);

            _skeletonAnimation.AnimationState.SetAnimation(0, "arrow run", true);
            playerScript.SetAttackAnimationEnabled(true);
            
            

            playerScript.isShooting = false;
             yield return new WaitForSeconds(0.5f); // 발사가 끝난 후 1초 동안 대기합니다.
            _canShoot = true; // 1초가 지난 후 _canShoot 변수를 true로 변경하여 다시 발사할 수 있도록 합니다.
        }
    }

    private IEnumerator shotthearrow(Vector2 direction)
    {


        yield return new WaitForSeconds(0.55f);

        // Instantiate and shoot the arrow
        GameObject arrowInstance = Instantiate(arrowPrefab, hand.position, Quaternion.identity);
        arrowInstance.transform.right = direction;
        arrowInstance.GetComponent<Rigidbody2D>().velocity = direction * arrowSpeed;

        // Disable the Rigidbody2D component on the main character to prevent it from being affected by the arrow's force
        Rigidbody2D mainCharacterRigidbody = mainCharacter.GetComponent<Rigidbody2D>();
        mainCharacterRigidbody.simulated = false;
        yield return new WaitForSeconds(0.1f);
        mainCharacterRigidbody.simulated = true;

       

    }

    void Update()
    {
        if (isJoystickPressed)
        {
            Vector2 inputPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(joystick.GetComponent<RectTransform>(), Input.mousePosition, null, out inputPos);
            UpdateArrowTrajectory(inputPos);
        }
        else if (_canShoot)
        {
            Vector2 attackDirection = new Vector2(attackJoystick.Horizontal, attackJoystick.Vertical).normalized;

            if (attackDirection.magnitude > 0.1f)
            {
                arrowTrajectory.SetPosition(0, hand.position);

                // Calculate arrow direction
                Vector2 direction = attackDirection;

                // Draw arrow trajectory
                arrowTrajectory.enabled = true;
                arrowTrajectory.SetPosition(1, hand.position + (Vector3)(direction * maxDistance));
            }
            else
            {
                arrowTrajectory.enabled = false;
            }
        }
    }

    private IEnumerator PauseAttackAnimation(float waitTime)
    {
        playerScript.SetAttackAnimationEnabled(false);
        yield return new WaitForSeconds(waitTime);
        playerScript.SetAttackAnimationEnabled(true);
    }
    private void UpdateArrowTrajectory(Vector2 inputPos)
    {
        Vector2 joystickPos = joystick.GetComponent<RectTransform>().anchoredPosition;
        Vector2 direction = (joystickPos - inputPos).normalized; // 중심 위치를 기준으로 계산
        float distance = Vector2.Distance(inputPos, joystickPos);

        // Draw arrow trajectory
        arrowTrajectory.enabled = true;
        arrowTrajectory.SetPosition(0, hand.position);

        // Set the arrow trajectory's end position to match the attack direction
        Vector3 attackDirection = new Vector3(attackJoystick.Horizontal, attackJoystick.Vertical, 0);
        if (attackDirection.magnitude > 0.1f)
        {
            attackDirection.Normalize();
            arrowTrajectory.SetPosition(1, hand.position - attackDirection * maxDistance);
        }
        else
        {
            arrowTrajectory.SetPosition(1, hand.position - (Vector3)direction * distance);
        }
    }



}
