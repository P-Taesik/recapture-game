using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WeaponAttackController : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float arrowSpeed = 10.0f;

    public static WeaponAttackController weaponInstance;
    private bool isAttacking = false;
    public Sprite bow;
    public Sprite sword;
    public Button changeWeaponButton;
    public Button attackButton;
    public BowJoystickController bowJoystickController;

public static WeaponAttackController GetWeaponInstance()
{
    return weaponInstance;
}    public enum WeaponType
    {
        Sword,
        Bow
    }

    public static WeaponType weaponType = WeaponType.Sword;

    private bool isCooling = false;
    public float coolTime = 1.0f;
    private float remainingCoolTime = 0.0f;
private void Awake()
    {
        weaponInstance = this;
    }

    private void Start()
    {
        bowJoystickController = GameObject.Find("BowController").GetComponent<BowJoystickController>();

        InitializeChangeWeaponButton();
        weaponType = WeaponType.Sword;
        InitializeAttackButton();
        changeWeaponButton.onClick.AddListener(OnChangeWeaponButtonClick);
        bowJoystickController.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        changeWeaponButton.onClick.RemoveListener(OnChangeWeaponButtonClick);
    }

    private void InitializeChangeWeaponButton()
    {
        Image changeWeaponButtonImage = changeWeaponButton.GetComponent<Image>();
        if (changeWeaponButtonImage == null)
        {
            Debug.LogError("무기 변경 버튼의 Image 컴포넌트가 없습니다.");
        }
        else
        {
            changeWeaponButton.image = changeWeaponButtonImage;
        }
    }

    private void InitializeAttackButton()
    {
        GameObject attackButtonObject = GameObject.Find("Attack");

        if (attackButtonObject != null)
        {
            attackButton = attackButtonObject.GetComponent<Button>();
            attackButton.onClick.AddListener(OnAttackButtonClick);
            attackButton.image.sprite = sword;
        }
        else
        {
            Debug.LogError("Attack 오브젝트를 찾을 수 없습니다.");
        }
    }

    public void OnChangeWeaponButtonClick()
    {
        if (weaponType == WeaponType.Sword)
        {
            weaponType = WeaponType.Bow;
            Debug.Log("무기를 활로 변경합니다.");
            attackButton.gameObject.SetActive(false); // 공격 버튼 비활성화
            GameObject bowController = GameObject.Find("BowController");
            if (bowController != null)
            {
                bowController.SetActive(true); // 조이스틱 활성화
            }
        }
        else if (weaponType == WeaponType.Bow)
        {
            weaponType = WeaponType.Sword;
            Debug.Log("무기를 검으로 변경합니다.");
            attackButton.gameObject.SetActive(true); // 공격 버튼 활성화
            GameObject bowController = GameObject.Find("BowController");
            if (bowController != null)
            {
                bowController.SetActive(false); // 조이스틱 비활성화
            }
        }
        UpdateAttackButtonSprite();
        bowJoystickController.gameObject.SetActive(weaponType == WeaponType.Bow); // 무기가 변경될 때 조이스틱 활성화
    }

    private void UpdateAttackButtonSprite()
    {
        if (attackButton != null)
        {
            attackButton.image.sprite = weaponType == WeaponType.Bow ? bow : sword;
        }
    }

    public void OnAttackButtonClick()
    {
        if (isCooling) return;

        if (weaponType == WeaponType.Sword)
        {
            SwordAttack();
        }
        else if (weaponType == WeaponType.Bow)
        {
            BowAttack();
        }

        isCooling = true;
        remainingCoolTime = coolTime;
        StartCoroutine(CoolDown());
    }

    private IEnumerator CoolDown()
    {
        while (remainingCoolTime > 0.0f)
        {
            remainingCoolTime -= Time.deltaTime;
            yield return null;
        }
        isCooling = false;
        attackButton.interactable = true;
    }

    public void SwordAttack()
    {
        Debug.Log("칼 공격 처리");
        attackButton.interactable = false;
    }
    public void BowAttack()
{
    if (bowJoystickController.isJoystickPressed || !bowJoystickController.CanMove) return;
    Debug.Log("활 공격 처리");
    attackButton.interactable = false;
}
}
