using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIScript : MonoBehaviour
{
    public static UIScript instance {get;set;}
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI prevScoreText;
    public TextMeshProUGUI generation;
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

}
