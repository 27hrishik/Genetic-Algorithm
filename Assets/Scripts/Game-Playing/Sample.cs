using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : MonoBehaviour
{
    NeuralNet neural;
    // Start is called before the first frame update
    void Start()
    {
        neural = new NeuralNet();
        neural.CreateNetwork(65,32,32,16,1);
        neural.IntializeWeightsAndbiases(new Vector2(-1f,1f),new Vector2(-1f,1f));
        float[] input = new float[65];
        for(int i=0;i<65;i++)
            input[i] = Random.Range(0f,1f);
        string intialTime = System.DateTime.Now.ToString("HH:mm:ss.ffffff");    
        float[] output = neural.ForwardPropagation(input);
        string finalTime = System.DateTime.Now.ToString("HH:mm:ss.ffffff");
        Debug.Log("Output is:"+output[0]);
        Debug.Log("start :"+intialTime);
        Debug.Log("stop :"+finalTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
