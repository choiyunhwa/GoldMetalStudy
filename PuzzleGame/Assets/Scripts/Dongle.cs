using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public GameManager manager;
    public ParticleSystem effect;

    public int level;
    public bool isDrag = false;
    public bool isMerge;
    public bool isAttach;


    public Rigidbody2D rigid;
    private CircleCollider2D circle;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private float dealTime;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        circle = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        anim.SetInteger("Level", level);
    }

    private void OnDisable() //��Ȱ��ȭ �� �� �ڵ����� ����
    {
        //���� �Ӽ� �ʱ�ȭ
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        //���� Ʈ������ �ʱ�ȭ
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        //���� ���� �ʱ�ȭ
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0f; //ȸ�� �ӵ�
        circle.enabled = true;

    }
    void Update()
    {
        if(isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // X�� ��� ����
            float leftBorder = -4.2f + transform.localScale.x / 2f;
            float rightBorder = 4.2f - transform.localScale.x / 2f;

            if (mousePos.x < leftBorder)
            {
                mousePos.x = leftBorder;
            }
            else if (mousePos.x > rightBorder)
            {
                mousePos.x = rightBorder;
            }
            mousePos.y = 8;
            mousePos.z = 0;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);
        }
        
    }

    public void Drag()
    {
        isDrag = true;
    }

    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(AttachRoutine());
    }

    private IEnumerator AttachRoutine()
    {
        if (isAttach)
            yield break;

        isAttach = true;
        manager.SfxPlay(GameManager.Sfx.Attach);

        yield return new WaitForSeconds(0.2f);

        isAttach = false;
       
    }

    private void OnCollisionStay2D(Collision2D collision) //������ �浹 ���϶� ��� ����Ǵ� �Լ�
    {
        if(collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            if(level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                //���� ��ġ��
                //���� ����� ��ġ ��������

                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                //1. ���� �Ʒ��� ������
                //2. ������ ���� �϶�, ���� �����ʿ� ���� ��

                if(meY < otherY || (meX == otherX && meX > otherX))
                {
                    // ���� �����

                    other.Hide(transform.position);

                    // ������

                    LevelUp();
                }
            }
        }
    }

    public void Hide(Vector3 targetPos)
    {
        isMerge = true;

        rigid.simulated = false;
        circle.enabled = false;

        if(targetPos == Vector3.up * 100)
        {
            EffectPlay();
        }

        StartCoroutine(HideRoutine(targetPos));
    }

    private IEnumerator HideRoutine(Vector3 targetPos)
    {

        int frameCount = 0;

        while(frameCount < 20)
        {
            frameCount++;

            if(targetPos != Vector3.up * 100)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            }
            else if( targetPos == Vector3.up * 100)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }
            yield return null;
        }

        manager.score += (int)Mathf.Pow(2, level);

        isMerge = false;
        gameObject.SetActive(false);
    }

    private void LevelUp()
    {
        isMerge = true;

        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0f;

        StartCoroutine(LevelUpRoutine());
    }

    private IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1);
        EffectPlay();
        manager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f);

        level++;

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        isMerge = false;
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            dealTime += Time.deltaTime;

            if(dealTime > 2)
            {
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }
            if(dealTime > 5)
            {
                manager.GameOver();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            dealTime = 0f;
            spriteRenderer.color = Color.white;
        }
    }

    private void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;

        effect.Play();
    }
}
