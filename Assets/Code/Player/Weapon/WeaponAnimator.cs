using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimator : MonoBehaviour
{

    public Animator weapon;


    public void ShootAnim()
    {
        weapon.SetTrigger("shoot");
    }

    public void ReloadAnim()
    {
        weapon.SetTrigger("reload");
    }

    public void RunAnim()
    {
        weapon.SetBool("run", true);
    }

}
