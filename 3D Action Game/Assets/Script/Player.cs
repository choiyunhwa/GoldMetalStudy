using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    /*
     * 1. 플레이어 속도
     * 2. Input System
     * 3. 가속도 계산?*/

    private float hAxis;
    private float vAxis;

    private bool wDown;
    private bool jDown;
    private bool fDown;
    private bool iDown;
    private bool sDown1;
    private bool sDown2;
    private bool sDown3;

    private bool isJump = false;
    private bool isDodge;
    private bool isSwap;

    public float speed;

    public int ammo;
    public int coin;
    public int health;
    

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public int hasGrenades;

    private float fireDelay;
    private bool isFireReady = true;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;
    private Animator anim;

    //-- 트리거 된 아이템을 저장 -
    private GameObject nearObject;
    private Weapon equipWeapon; // 장착하고 있는 무기
    private int equipWeaponIndex = -1;


    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    public void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Dodge();
        Interation();
        Swap();
    }

    public void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        fDown = Input.GetButtonDown("Fire1");
    }

    public void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;
        if( isSwap || !isFireReady)
            moveVec = Vector3.zero;
        
        transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("IsRun", moveVec != Vector3.zero);
        anim.SetBool("IsWalk", wDown);

        
    }

    public void Turn()
    {
        transform.LookAt(transform.position + moveVec); //회전
    }

    public void Jump()
    {
        if(jDown && moveVec == Vector3.zero && !isJump && !isDodge)
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
            anim.SetBool("IsJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    public void Attack()
    {
        if(equipWeapon == null)
        {
            return;
        }

        fireDelay += Time.deltaTime;

        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();

            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    public void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump &&!isDodge && !isSwap)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.4f);
        }
    }

    public void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }
    private void Swap()
    {

        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;


        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if ((sDown1 || sDown2 || sDown3)&& !isJump && !isDodge )
        {
            if(equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");
            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }

    }

    public void SwapOut()
    {
        isSwap = false;
    }
    private void Interation()
    {
        if(iDown && nearObject != null && !isJump && !isDodge)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }

    

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            anim.SetBool("IsJump", false);
            isJump = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
      if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if(ammo > maxAmmo)
                    {
                        ammo = maxAmmo;
                    }
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                    {
                        coin = maxCoin;
                    }
                    break;
                case Item.Type.Grenade: //수류탄
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                    {
                        hasGrenades = maxHasGrenades;
                    }
                    break;

                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                    {
                        health = maxHealth;
                    }
                    break;
            }
            Destroy(other.gameObject);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon")
        {
            nearObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
    }

}
