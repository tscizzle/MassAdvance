using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Material damagedMaterial;

    public static float blockHeight = 0.5f;

    public string blockType;
    public bool isDamaged = false;

    void Start()
    {

    }

    void Update()
    {
        
    }

    /* PUBLIC API */

    public void damageBlock()
    {
        isDamaged = true;
        GetComponent<Renderer>().material = damagedMaterial;
    }
}
