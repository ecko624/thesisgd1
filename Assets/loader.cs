using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class loader : MonoBehaviour
{
    public string nexr;
    // Start is called before the first frame update
    public void Start2()
    {
        Application.LoadLevel(nexr);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
