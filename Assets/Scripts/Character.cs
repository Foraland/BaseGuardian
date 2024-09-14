using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public bool isInAtk = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            if (isInAtk)
            {
                other.gameObject.GetComponent<Enemy>().BeHurt();
            }
        }
    }
}
