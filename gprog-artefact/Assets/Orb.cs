using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    [SerializeField] private Podium podium;
    void Start()
    {
        if (podium != null) podium.SetItem(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
