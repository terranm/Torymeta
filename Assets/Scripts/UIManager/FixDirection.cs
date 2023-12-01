using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FixDirection : MonoBehaviour
{
    GameObject target;
    
    Vector3 startScale;
    public float distance = 3;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {
        transform.rotation = target.transform.rotation;
    }
}
