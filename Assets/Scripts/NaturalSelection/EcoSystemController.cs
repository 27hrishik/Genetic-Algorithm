using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public sealed class EcoSystemController : MonoBehaviour
{
    public static EcoSystemController instance {get;set;} // for making a Singleton
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
    [Header("Food")]
    public Transform foodParent;
    public GameObject foodPrefab;
    public enum FoodGenType {uniform,depleting,populationBased};
    public FoodGenType foodGenType;
    public float foodCount; // No of food to be generated in the instance
    public float minFoodCount;
    public float depletionRate;
    public float foodPerAgent;
    List<GameObject> foodCollection;
    
    [Header("Region")]
    public float regionWidth; // width of the working plane

    [Header("AgentProperties")] //min and max values of respective genes
    public int agentCount;
    public Transform agentParent;
    public GameObject agentPrefab;
    public Vector2 speed;
    public Vector2 size;
    public Vector2 sense;
    public enum GeneType {speed,size,sense};
    List<GameObject> agentCollection;
    List<GameObject> agentToKill,agentToSurvive,agentToCrossover;

    public enum IntialPopulationType {uniform,uniformWithMutation,randomised};
    [Header("Intialization")]
    public IntialPopulationType populationType;
    public Vector3 intialConfiguration;
    [Header("Selection")]
    public int survivalRequirement;
    public int crossoverRequirement;

    [Range(0f,1f),Header("Mutation")]
    public float mutationFactor = 0.1f;

    //Internal Variables
    int genCount;
    int activeAgentCount;
    public int ActiveAgentCount{
        get{
            return activeAgentCount;
        }
        set{
            activeAgentCount = value;
            if(activeAgentCount==0)
                StartNextGeneration();
        }
    }
    public enum AgentReport {kill,pass,crossover};

    //Graphing
    public readonly string path = "./ProjectGraphs/";
    string population = "population.csv";
    string chromosomes = "chromosomes.csv";

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
        if(!Directory.Exists(path))
            Directory.CreateDirectory(path);
        FileStream f = new FileStream(path+population,FileMode.OpenOrCreate);
        f.Close();
        f = new FileStream(path+chromosomes,FileMode.OpenOrCreate);
        f.Close();
        StreamWriter fs = new StreamWriter(path+population);
        fs.Write("Killed,Survived,Crossover\n");
        fs.Close();
        fs = new StreamWriter(path+chromosomes);
        fs.Write("Avg. Speed,Avg. Size,Avg. Sense\n");
        fs.Close();
    }
    // Start is called before the first frame update
    void Start()
    {
        foodCollection = new List<GameObject>();
        agentCollection = new List<GameObject>();
        agentToKill = new List<GameObject>();
        agentToSurvive = new List<GameObject>();
        agentToCrossover = new List<GameObject>();
        genCount = 1;
        UIController.instance.genText.text = "Gen : " + genCount;
        GenerateFood();
        GenerateIntialPopulation();
    }

    void GenerateIntialPopulation()
    {
        Vector2 tempPosition;
        GameObject temp;
        float tempX,tempY,tempZ;
        for(int i=0;i<agentCount;i++)
        {
            switch(populationType)
            { 
                case IntialPopulationType.uniformWithMutation:
                    float mutation = mutationFactor;
                    tempX = Mathf.Clamp01(intialConfiguration.x + Random.Range(-mutation,mutation));
                    tempY = Mathf.Clamp01(intialConfiguration.y + Random.Range(-mutation,mutation));
                    tempZ = Mathf.Clamp01(intialConfiguration.z + Random.Range(-mutation,mutation));
                break;
                case IntialPopulationType.randomised:
                    tempX = Mathf.Clamp01(Random.value);
                    tempY = Mathf.Clamp01(Random.value);
                    tempZ = Mathf.Clamp01(Random.value); 
                break;
                default:
                    tempX = Mathf.Clamp01(intialConfiguration.x);
                    tempY = Mathf.Clamp01(intialConfiguration.y);
                    tempZ = Mathf.Clamp01(intialConfiguration.z);
                break;          
            }
            tempPosition = GetAgentInstantiationPosition();
            temp = Instantiate(agentPrefab,tempPosition,Quaternion.identity);
            temp.transform.parent = agentParent;
            temp.GetComponent<AgentBehaviour>().IntializeAgent(tempX,tempY,tempZ);
            agentCollection.Add(temp); 
        }
        ActiveAgentCount = agentCollection.Count;
    }

    (float speed,float size,float sense) GetCrossoverChromosomes(AgentBehaviour p1,AgentBehaviour p2)
    {
        (float speed1,float size1,float sense1) = p1.GetChromosomes();
        (float speed2,float size2,float sense2) = p2.GetChromosomes();
        float speed = Random.Range(0,1)==0?speed1:speed2;
        float size = Random.Range(0,1)==0?size1:size2;
        float sense = Random.Range(0,1)==0?sense1:sense2;
        speed = Mathf.Clamp01(speed + Random.Range(-mutationFactor,mutationFactor));
        size = Mathf.Clamp01(size + Random.Range(-mutationFactor,mutationFactor));
        sense = Mathf.Clamp01(sense + Random.Range(-mutationFactor,mutationFactor));
        return (speed,size,sense);
    }

    Vector2 GetAgentInstantiationPosition()
    {
        Vector2 start,end;
        switch(Random.Range(0,4))
        {
            case 0: 
                start = new Vector2(-regionWidth,regionWidth);
                end = new Vector2(regionWidth,regionWidth);
            break;
            case 1:
                start = new Vector2(regionWidth,regionWidth);
                end = new Vector2(regionWidth,-regionWidth);
            break;
            case 2:
                start = new Vector2(regionWidth,-regionWidth);
                end = new Vector2(-regionWidth,-regionWidth);
            break;
            default:
                start = new Vector2(-regionWidth,-regionWidth);
                end = new Vector2(-regionWidth,regionWidth);
            break;   
        }
        return Vector2.Lerp(start,end,Random.value);
    }
    // Update is called once per frame
    void Update()
    {
        TimeScale = timeScale;
    }

    void StartNextGeneration()
    {
        UIController.instance.genText.text = "Gen : " + genCount;
        UpdateChromosomesData();
        ClassifyAgent();
        UpdatePopulationData();
        KillAgent(); 
        SurviveAgent();
        CrossoverAgent();
        ActiveAgentCount = agentCollection.Count;
        genCount++;
        GenerateFood();     
    }

    void UpdatePopulationData()
    {
        StreamWriter fs = new StreamWriter(path+population,true);
        fs.Write(agentToKill.Count+","+agentToSurvive.Count+","+agentToCrossover.Count+"\n");
        fs.Close();       
    }

    void UpdateChromosomesData()
    {
        float avgSpeed = 0f,avgSize = 0f,avgSense = 0f;
        foreach(GameObject go in agentCollection)
        {
            if(go!=null)
            {
                (float speed,float size,float sense) = go.GetComponent<AgentBehaviour>().GetChromosomes();
                avgSpeed += speed;
                avgSize += size;
                avgSense += sense;
            }
        }
        avgSense=avgSense/agentCollection.Count;
        avgSize=avgSize/agentCollection.Count;
        avgSpeed=avgSpeed/agentCollection.Count;
        StreamWriter fs = new StreamWriter(path+chromosomes,true);
        fs.Write(string.Format("{0:0.0000},{1:0.0000},{2:0.0000}\n",avgSpeed,avgSize,avgSense));
        fs.Close();
    }
    void ClassifyAgent()
    {
        foreach(GameObject go in agentCollection)
        {
            if(go!=null)
            {
                switch(GetAgentReport(go.GetComponent<AgentBehaviour>().GetFoodCount()))
                {
                    case AgentReport.kill:
                        agentToKill.Add(go);
                    break;
                    case AgentReport.pass:
                        agentToSurvive.Add(go);
                    break;
                    case AgentReport.crossover:
                        agentToCrossover.Add(go);
                    break;            
                }
            }
        }
        agentCollection.Clear();
    }

    void KillAgent()
    {
        foreach(GameObject go in agentToKill)
        {
            if(go!=null)
            {
                Destroy(go);
            }
        }
        agentToKill.Clear();
    }

    void SurviveAgent()
    {
        foreach(GameObject go in agentToSurvive)
        {
            if(go!=null)
            {
                (float speed,float size,float sense) = go.GetComponent<AgentBehaviour>().GetChromosomes();
                go.transform.localPosition = GetAgentInstantiationPosition();
                go.GetComponent<AgentBehaviour>().IntializeAgent(speed,size,sense);
                agentCollection.Add(go);
            }
        }
        agentToSurvive.Clear();   
    }

    void CrossoverAgent()
    {
        if(agentToCrossover.Count>0)
        {
            int count = agentToCrossover.Count;
            int[] randomSequence = new int[count];
            for(int i=0;i<count;i++)
                randomSequence[i] = i;
            int index = 0,swap,temp;
            while(count - index>1)
            {
                swap = Random.Range(index,count);
                temp = randomSequence[index];
                randomSequence[index] = randomSequence[swap];
                randomSequence[swap] = temp;
                index++;
            }
            for(int i=0;i<count;i+=2)
            {
                if(i+1<count)
                {
                    int index1 = randomSequence[i];
                    int index2 = randomSequence[i+1];
                    GameObject p1 = agentToCrossover[index1];
                    GameObject p2 = agentToCrossover[index2];
                    p1.transform.localPosition = GetAgentInstantiationPosition();
                    p2.transform.localPosition = GetAgentInstantiationPosition();
                    AgentBehaviour a1 = p1.GetComponent<AgentBehaviour>();
                    AgentBehaviour a2 = p2.GetComponent<AgentBehaviour>();
                    (float speed1,float size1,float sense1) = a1.GetChromosomes();
                    (float speed2,float size2,float sense2) = a2.GetChromosomes();
                    Vector2 tempPosition = GetAgentInstantiationPosition();
                    GameObject child = Instantiate(agentPrefab,tempPosition,Quaternion.identity);
                    child.transform.parent = agentParent;
                    (float speed,float size,float sense) = GetCrossoverChromosomes(a1,a2);
                    child.GetComponent<AgentBehaviour>().IntializeAgent(speed,size,sense);
                    a1.IntializeAgent(speed1,size1,sense1);
                    a2.IntializeAgent(speed2,size2,sense2);
                    agentCollection.Add(p1);
                    agentCollection.Add(p2);
                    agentCollection.Add(child);
                }
                else
                {
                    GameObject go = agentToCrossover[randomSequence[i]];
                    (float speed,float size,float sense) = go.GetComponent<AgentBehaviour>().GetChromosomes();
                    go.transform.localPosition = GetAgentInstantiationPosition();
                    go.GetComponent<AgentBehaviour>().IntializeAgent(speed,size,sense);
                    agentCollection.Add(go);    
                }

            }     
        }
        agentToCrossover.Clear();
    }
    
    AgentReport GetAgentReport(int value)
    {
        if(value<survivalRequirement)
            return AgentReport.kill;
        else if(value<crossoverRequirement)
            return AgentReport.pass;
        else
            return AgentReport.crossover;        
    }

    void GenerateFood()
    {
        if(foodCollection.Count>0)
        {
            foreach(GameObject go in foodCollection)
            {
                if(go!=null)
                {
                    Destroy(go);
                }
            }
        }
        foodCollection.Clear();
       
        switch(foodGenType)
        {
            case FoodGenType.depleting:
                foodCount = Mathf.Max(minFoodCount,foodCount - depletionRate);
            break;
            case FoodGenType.populationBased:
                foodCount = Mathf.Max(minFoodCount,agentCollection.Count * foodPerAgent);
            break;        
        }
        int count = (int)foodCount;
        for(int i=0;i<count;i++)
        {
            Vector2 position = GetRandomPositionOnPlane();
            GameObject temp = Instantiate(foodPrefab,position,Quaternion.identity);
            temp.transform.parent = foodParent;
            foodCollection.Add(temp);   
        }
    }

    public Vector2 GetRandomPositionOnPlane()
    {
        return new Vector2(Random.Range(-regionWidth,regionWidth),Random.Range(-regionWidth,regionWidth));
    }

    public float GetNormalizedValue(GeneType type,float value)
    {
        switch(type)
        {
            case GeneType.speed:
                return (value - speed.x)/(speed.y-speed.x);
            case GeneType.size:
                return (value - size.x)/(size.y-size.x);
            case GeneType.sense:
                return (value - sense.x)/(sense.y-sense.x);        
        }
        return 0f;
    }

    public float GetScaledValue(GeneType type,float value)
    {switch(type)
        {
            case GeneType.speed:
                return speed.x + value*(speed.y-speed.x);
            case GeneType.size:
                return size.x + value*(size.y-size.x);
            case GeneType.sense:
                return sense.x + value*(sense.y-sense.x);        
        }
        return 0f;
    }
}
