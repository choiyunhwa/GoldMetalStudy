using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    //근접 공격
    //원거리 공격

    public enum Type
    {
        Melee,
        Range,
    };

    public Type type;

    public int damage;
    public float rate;
    public BoxCollider meleeArea;

    public TrailRenderer trailEffect;

    public Transform bulletPos;
    public GameObject byllet;

    public void Use()
    {
        if (type == Type.Melee)
        {
            StopCoroutine(Swing());
            StartCoroutine(Swing());
        }
    }

    IEnumerator Swing()
    {     
        yield return new WaitForSeconds(0.1f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
    }
}
