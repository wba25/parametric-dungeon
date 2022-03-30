using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour 
{
    void Awake ()
    {
        GameManager.Instance.cam = GetComponent<Camera>();
    }
}