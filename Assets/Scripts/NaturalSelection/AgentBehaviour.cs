using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBehaviour : MonoBehaviour
{
    public Transform agent; //sprite for showing size change
    public CircleCollider2D senseTrigger; // the sense trigger through which agent detects food
    //All the Genes
    [SerializeField]
    float speed = 5f;
    [SerializeField]
    float size = 1.5f;
    [SerializeField]
    float sense = 5f;
    //Internal Variables
    bool ifChasingFood;
    int foodCollected; // to count the no of food consumed;
    Vector2? moveTowardsPosition; //Create a Optional Vector2 means can also have null value 
    float energyCapacity,energyDepletionRate;
    bool ifEnergyLeft,ifReturned;
    Vector2 intialPosition;

    public void IntializeAgent(float speed,float size,float sense)
    {
        GetComponentInChildren<SpriteRenderer>().color = new Color(size,speed,sense);
        this.speed = EcoSystemController.instance.GetScaledValue(EcoSystemController.GeneType.speed, speed);
        this.size = EcoSystemController.instance.GetScaledValue(EcoSystemController.GeneType.size, size);
        agent.localScale = Vector3.one * this.size;
        this.sense = senseTrigger.radius = EcoSystemController.instance.GetScaledValue(EcoSystemController.GeneType.sense, sense);
        foodCollected = 0;
        moveTowardsPosition = null; 
        energyDepletionRate = Mathf.Pow(this.size,3f) * Mathf.Pow(this.speed,2f) + this.sense;
        energyCapacity = 1000f;
        intialPosition = transform.localPosition;
        ifEnergyLeft = true;
        ifReturned = false;

    }
    // Update is called once per frame
    void Update()
    {
        if(ifEnergyLeft && !ifReturned)
            MoveTowardsPosition();
        else if(!ifReturned)
            ReturnToPlace();          
    }

    public bool IfChasingFood()
    {
        return ifChasingFood;
    }

    public void SetPositionToMoveTowards(Vector2? position,bool type = true)
    {
        if(position.HasValue)
        { 
            moveTowardsPosition = position.Value;
            ifChasingFood = type;
        }
        else
        {
            moveTowardsPosition = null;  
            ifChasingFood = false;
        }   
    }

    void MoveTowardsPosition()
    {
        energyCapacity -= energyDepletionRate * Time.deltaTime;
        if(energyCapacity<0f)
            ifEnergyLeft = false;
        if(moveTowardsPosition.HasValue)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition,moveTowardsPosition.Value,speed * Time.deltaTime);
            if(Vector3.Distance(transform.localPosition,moveTowardsPosition.Value)<0.5f)
                moveTowardsPosition = null;
        }
        else
        {
            moveTowardsPosition = EcoSystemController.instance.GetRandomPositionOnPlane();
            ifChasingFood = false;
        }
    }

    void ReturnToPlace()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition,intialPosition,speed * Time.deltaTime);
            if(Vector3.Distance(transform.localPosition,intialPosition)<0.1f)
                {  
                    ifReturned = true;
                    EcoSystemController.instance.ActiveAgentCount--;
                }
    }

    public void IncrementFoodCount()
    {
        foodCollected++;
    }

    public int GetFoodCount()
    {
        return foodCollected;
    }

    public (float speed,float size,float sense) GetChromosomes()
    {
        float retSpeed = EcoSystemController.instance.GetNormalizedValue(EcoSystemController.GeneType.speed, speed);
        float retSize = EcoSystemController.instance.GetNormalizedValue(EcoSystemController.GeneType.size, size);
        float retSense = EcoSystemController.instance.GetNormalizedValue(EcoSystemController.GeneType.sense,sense);
        return (retSpeed,retSize,retSense);       
    }
}
