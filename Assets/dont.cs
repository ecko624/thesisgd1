using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dont : MonoBehaviour
{
    public AudioSource audioSource; // assign your AudioSource here

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnMouseDown()
    {
        // Only trigger on left mouse button
        if (Input.GetMouseButtonDown(0))
        {
            if (audioSource != null)
            {
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("AudioSource not assigned to " + gameObject.name);
            }
        }
    }
}
