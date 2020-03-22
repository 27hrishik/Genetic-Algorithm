using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public sealed class UIController : MonoBehaviour
{
    public static UIController instance {get;set;}

    public TextMeshProUGUI genText;
    // Start is called before the first frame update
    void Awake()
    {
       if(!instance)
       {
           instance = this;
       }
       else
       {
           Destroy(this.gameObject);
       }     
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
