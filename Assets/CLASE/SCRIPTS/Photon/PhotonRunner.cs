using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonRunner : MonoBehaviour
{
    public bool bool1;
    public bool bool2;

    // Start is called before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bool1 = true;
        coroutine = StartCoroutine(Corrutina());
        bool2 = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    Coroutine coroutine;

    IEnumerator Corrutina()
    {
        yield return new WaitForSeconds(10f);
        Debug.Log("Hola");
    }

    void Metodo()
    {
        // isConnected = true
        // error
    }
}