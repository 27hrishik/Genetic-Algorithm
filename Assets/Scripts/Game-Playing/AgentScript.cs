using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentScript : MonoBehaviour
{
    float speed;
    [HideInInspector]
    public NeuralNet neural;
    public float finalScore;
    float[] inputValues;
    // Start is called before the first frame update
    void Start()
    {
        finalScore = 0f;
        speed = 5f;
        Color color = Random.ColorHSV();
        color.a = 1f;
        GetComponent<SpriteRenderer>().color = color;
        inputValues = new float[3];
    }

    public void Intialize(Vector2 weightRange,Vector2 biasRange,int[] neuronsPerLayer)
    {
        neural = new NeuralNet();
        neural.CreateNetwork(neuronsPerLayer);
        neural.IntializeWeightsAndbiases(weightRange,biasRange);
    }

    public void Intialize(float[][,] weights,float[][] biases,int[] neuronsPerLayer)
    {
        neural = new NeuralNet();
        neural.CreateNetwork(neuronsPerLayer);
        neural.SetWeights(weights);
        neural.SetBias(biases);
    }
    // Update is called once per frame
    void Update()
    {
        inputValues[0] = (transform.position.x + NeuroEvolution.instance.width)/(2f * NeuroEvolution.instance.width);
        inputValues[1] = NeuroEvolution.instance.nextObstacleOffset;
        inputValues[2] = NeuroEvolution.instance.nextObstacleHeight;
        float[] outputValues = neural.ForwardPropagation(inputValues);
        if(outputValues[0]>outputValues[1])
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
        }
        else if(outputValues[0]<outputValues[1])
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.CompareTag("Obstacle"))
        {
            finalScore = NeuroEvolution.instance.score;
            gameObject.SetActive(false);
            NeuroEvolution.instance.CurrentAgent--;
        }
    }

    void OnEnable()
    {
        Start();
    }
}
