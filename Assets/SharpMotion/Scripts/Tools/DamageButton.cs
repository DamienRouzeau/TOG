using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageButton : MonoBehaviour
{
    public int damageQuantity;
    ZombieController controller;

    private void Start()
    {
        FindZombie();
    }

    public void FindZombie()
    {
        if (controller == null)
        {
            controller = FindObjectOfType<ZombieController>();
        }
    }

    public void MakeDamage()
    {
        if (controller == null)
        {
            FindZombie();
        }

        if (controller == null)
        {
            Debug.Log("No Zombie in the scene");
        }
        else
        {
            controller.currentHealth -= damageQuantity;
            //Debug.Log("Zombie Health: " + controller.currentHealth);
        }
    }
}
