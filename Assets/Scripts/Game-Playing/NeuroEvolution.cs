using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;


public sealed class NeuroEvolution : MonoBehaviour
{
    public static NeuroEvolution instance {get;set;}
    [Header("Time")]
    public float timeScale;
    public float TimeScale{
        get{
            return Time.timeScale;
        }
        set{
            if(!value.Equals(Time.timeScale))
                Time.timeScale = value;
        }
    }

    [Header("General")]
    public bool ifTraining;
    public int modelGen;
    public int modelIndex;
    [Header("Obstacle Spawner")]
    public float interval;
    public float range;
    public GameObject objectPrefab;
    public GameObject agentPrefab;
    public Transform agentParent;
    public Sprite eliteSprite;
    [HideInInspector]
    public float score,prevScore;
    List<GameObject> obstacleList;

    [Header("Boundary")]
    public float height = 7.5f;
    public float width = 4f;

    [Header("NetworkProperties")]
    public Vector2 weightRange;
    public Vector2 biasRange;
    public int[] neuronsPerLayer;
    [HideInInspector]
    public float nextObstacleHeight,nextObstacleOffset;

    [Header("Genetic Algorithm")]
    public int noOfAgent;
    List<GameObject> agentList;
    int currentAgent;
    int genCount;
    public int CurrentAgent
    {
        get
        {
            return currentAgent;
        }
        set
        {
            currentAgent = value;
            if(currentAgent<=0)
            { 
                ifPlayerAlive = false;
                NextGeneration();
            }
            else
                ifPlayerAlive = true;   
                 
        }
    }
    [Range(0f,0.95f)] 
    public float percentileSelection;
    [Range(0f,1f)]
    public float crossOverProbability;
    public enum MutationType {randomized,drift}
    [Range(0f,1f)]
    public float mutationProbability; 
    public MutationType mutationType;
    [Range(0f,1f)]
    public float mutationDrift;
    public bool elitism;
    // [Range(1f,10f)]
    // public float nextGenFactor;

    bool ifPlayerAlive = true;

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
        agentList = new List<GameObject>();
        obstacleList = new List<GameObject>(); 
        score = 0f;
        genCount = 0;
        if(ifTraining)
            CreateIntialPopulation();
        else
            LoadTrainedModel();
        StartCoroutine(CreateObject());      
    }

    public void LoadTrainedModel()
    {
        GameObject temp = Instantiate(agentPrefab,Vector3.zero + agentParent.position,Quaternion.identity,agentParent);
        NeuralNet net = LoadNeuralNet();
        if(net==null)
            return;
        temp.GetComponent<AgentScript>().Intialize(net.GetWeights(),net.GetBias(),neuronsPerLayer);
        agentList.Add(temp);
        CurrentAgent = agentList.Count;
    }

    NeuralNet LoadNeuralNet()
    {
        NeuralNet temp = null;
        string modelName = null;
        foreach(int i in neuronsPerLayer)
            modelName += i.ToString();
        modelName += "G" + modelGen + "-" + modelIndex;    
        FileStream fs = new FileStream(modelName + ".mdl", FileMode.Open);
        try 
        {
            BinaryFormatter formatter = new BinaryFormatter();

            // Deserialize the hashtable from the file and 
            // assign the reference to the local variable.
            temp = (NeuralNet) formatter.Deserialize(fs);
        }
        catch (SerializationException e) 
        {
            Debug.Log("Failed to deserialize :" + modelName);
            throw;
        }
        finally 
        {
            fs.Close();
        }
        return temp;
    }

    public void SaveNeuralNet()
    {
        int count = 0;
        string modelName = null;
        foreach(int i in neuronsPerLayer)
            modelName += i.ToString();
        modelName += "G" + genCount + "-";
        foreach(GameObject go in agentList)
        {
            if(go.activeInHierarchy)
            {
                FileStream fs = new FileStream(modelName + count+".mdl", FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();
                try 
                {
                    formatter.Serialize(fs, go.GetComponent<AgentScript>().neural);
                }
                catch (SerializationException e) 
                {
                    Debug.Log("Failed to serialize. Reason: " + e.Message);
                // Construct a BinaryFormatter and use it to serialize the data to the stream. throw;
                }
                finally 
                {
                    fs.Close();
                }
                count++;
            }
        }
    }

    void CreateIntialPopulation()
    {
        CurrentAgent = noOfAgent;
        for(int i=0;i<noOfAgent;i++)
        {
            GameObject temp = Instantiate(agentPrefab,Vector3.zero + agentParent.position,Quaternion.identity,agentParent);
            temp.GetComponent<AgentScript>().Intialize(weightRange,biasRange,neuronsPerLayer);
            agentList.Add(temp);
        }
    }

    void NextGeneration()
    {
        UIScript.instance.generation.text = "Gen : " + ++genCount;
        //evalute agent fitness
        float maxScore = GetMaxScore();
        UIScript.instance.prevScoreText.text = "PrevScore : " + maxScore;
        //group them
        //discard the bad agents
        //select the good agents
        List<GameObject> sucessors = GetGoodAgents(maxScore);
        if(sucessors.Count==0)
        {
            UIScript.instance.generation.text = "Gen : Extinct";
            return;
        }
        int goodAgentCount = sucessors.Count;
        int remainingAgent = noOfAgent - goodAgentCount;      
        //fill remaining population crossover
        for(int i=0;i<remainingAgent;i++)
        {
            GameObject temp = Instantiate(agentPrefab,Vector3.zero + agentParent.position,Quaternion.identity,agentParent);
            if(Random.value<crossOverProbability)
            {
                AgentScript a1 = sucessors[Random.Range(0,goodAgentCount)].GetComponent<AgentScript>();
                AgentScript a2 = sucessors[Random.Range(0,goodAgentCount)].GetComponent<AgentScript>();
                float[][,] tempWeights = CrossOverWeights(a1.neural.GetWeights(),a2.neural.GetWeights());
                float[][] tempBias = CrossOverBias(a1.neural.GetBias(),a2.neural.GetBias());
                temp.GetComponent<AgentScript>().Intialize(tempWeights,tempBias,neuronsPerLayer);
            }
            else
            {
                AgentScript a = sucessors[Random.Range(0,goodAgentCount)].GetComponent<AgentScript>();
                float[][,] tempWeights = MutationWeights(a.neural.GetWeights());
                float[][] tempBias = MutationBias(a.neural.GetBias());
                temp.GetComponent<AgentScript>().Intialize(tempWeights,tempBias,neuronsPerLayer);
            }
            //mutate agent
            //add them to agentList
            agentList.Add(temp);
        }
        foreach(GameObject go in sucessors)
        {
            if(go)
            {
                if(elitism)
                {
                    GameObject temp = Instantiate(agentPrefab,Vector3.zero + agentParent.position,Quaternion.identity,agentParent);
                    AgentScript a = go.GetComponent<AgentScript>();
                    float[][,] tempWeights = a.neural.GetWeights();
                    float[][] tempBias = a.neural.GetBias();
                    AgentScript b = temp.GetComponent<AgentScript>();
                    b.Intialize(tempWeights,tempBias,neuronsPerLayer);
                    //Debug.Log("bs : "+a.neural.CompareBias(b.neural.GetBias()));
                    agentList.Add(temp);
                }
                Destroy(go);
            }
        }
        //adding
        //clearobstacle
        ClearObstacle();
        //clear score
        score = 0f;
        //start obstacle spawner
        StartCoroutine(CreateObject());
        CurrentAgent = agentList.Count;
    }

    public float[][,] CrossOverWeights(float[][,] p1,float[][,] p2)
    {
        float[][,] temp = new float[p1.Length][,];
        for(int i=0;i<p1.Length;i++)
        {
            temp[i] = new float[p1[i].GetLength(0),p1[i].GetLength(1)];
            for(int j=0;j<p1[i].GetLength(0);j++)
                for(int k=0;k<p1[i].GetLength(1);k++)
                {
                    if(Random.value < mutationProbability)
                        temp[i][j,k] = MutatedValue(temp[i][j,k],weightRange);
                    else
                        temp[i][j,k] = Random.value<0.5f?p1[i][j,k]:p2[i][j,k];    

                }
        }
        return temp;
    }

    float MutatedValue(float value,Vector2 range)
    {
        switch(mutationType)
        {
            case MutationType.randomized:
                return Random.Range(range.x,range.y);
            default:
                return Mathf.Clamp(value + Random.Range(-mutationDrift,mutationDrift),range.x,range.y);    
        }
    }

    public float[][,] MutationWeights(float[][,] p)
    {
        float[][,] temp = new float[p.Length][,];
        for(int i=0;i<p.Length;i++)
        {
            temp[i] = new float[p[i].GetLength(0),p[i].GetLength(1)];
            for(int j=0;j<p[i].GetLength(0);j++)
                for(int k=0;k<p[i].GetLength(1);k++)
                {
                    if(Random.value < mutationProbability)
                        temp[i][j,k] = MutatedValue(temp[i][j,k],weightRange);
                    else
                        temp[i][j,k] = p[i][j,k];    

                }
        }
        return temp;
    }


    public float[][] CrossOverBias(float[][] p1,float[][] p2)
    {
        float[][] temp = new float[p1.Length][];
        for(int i=0;i<p1.Length;i++)
        {
            temp[i] = new float[p1[i].Length];
            for(int j=0;j<p1[i].Length;j++)
            {
                if(Random.value < mutationProbability)
                    temp[i][j] = MutatedValue(temp[i][j],biasRange);
                else
                    temp[i][j] = Random.value<0.5f?p1[i][j]:p2[i][j];    

            }
        }
        return temp;
    }

    public float[][] MutationBias(float[][] p)
    {
        float[][] temp = new float[p.Length][];
        for(int i=0;i<p.Length;i++)
        {
            temp[i] = new float[p[i].Length];
            for(int j=0;j<p[i].Length;j++)
            {
                if(Random.value < mutationProbability)
                    temp[i][j] = MutatedValue(temp[i][j],biasRange);
                else
                    temp[i][j] = p[i][j];    

            }
        }
        return temp;
    }

    float GetMaxScore()
    {
        float max = 0f;
        foreach(GameObject go in agentList)
        {
            if(go)
            {   
                max = Mathf.Max(max,go.GetComponent<AgentScript>().finalScore);
            }
        }
        return max;
    }

    List<GameObject> GetGoodAgents(float maxScore)
    {
        List<GameObject> goodAgents = new List<GameObject>();
        foreach(GameObject go in agentList)
        {
            if(go)
            {
                if(go.GetComponent<AgentScript>().finalScore/maxScore>percentileSelection)
                    goodAgents.Add(go);
                else
                    Destroy(go);    
            }
        }
        agentList.Clear();
        return goodAgents;

    }

    void ClearObstacle()
    {
        foreach(GameObject go in obstacleList)
        {
            if(go)
                Destroy(go);
        }
        obstacleList.Clear();
    }

    void Update()
    {
        if(ifPlayerAlive)
        {
            score +=Time.deltaTime;
            UIScript.instance.scoreText.text = string.Format("Score : {0:0.00}",score);
            UpdateObstacle();
            foreach(GameObject go in obstacleList)
            {
                if(go && go.transform.position.y>-2.5f)
                {
                    nextObstacleHeight = (go.transform.position.y + 2.5f)/10f;
                    nextObstacleOffset = (go.transform.position.x + width)/(2f * width); 
                    break;
                }
            }
            TimeScale = timeScale;
        }
    }

    IEnumerator CreateObject()
    {
        while(ifPlayerAlive)
        {
            Vector2 pos = (Vector2)transform.position + Vector2.right * Random.Range(-range,range);
            GameObject temp = Instantiate(objectPrefab,pos,Quaternion.identity,transform);
            obstacleList.Add(temp);
            yield return new WaitForSeconds(interval);
        }
    }

    void UpdateObstacle()
    {
        int count = obstacleList.Count;
        for(int i=0;i<count;i++)
        {
            if(obstacleList[i])
            {
                obstacleList[i].transform.Translate(Vector3.down * 5f * Time.deltaTime);
            }
        }
        for(int i = 0;i<count;i++)
        {
            if(obstacleList[i] && obstacleList[i].transform.position.y<-7.5f )
            {
                GameObject temp = obstacleList[i];
                obstacleList.RemoveAt(i);
                Destroy(temp);
                break;   
            }
        }
    }
}
