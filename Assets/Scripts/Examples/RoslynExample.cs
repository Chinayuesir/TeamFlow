using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RoslynExample : MonoBehaviour
{
    public double output = 0;
    public string code = "4+5";
   

    // Update is called once per frame
    void Update()
    {
        try
        {
           
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }
}
