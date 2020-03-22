using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodScript : MonoBehaviour
{
    public AgentBehaviour agent;
   
    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.CompareTag("Food"))
        {
            agent.IncrementFoodCount();
            Destroy(collider.gameObject);
            agent.SetPositionToMoveTowards(null);
        }       
    }

}
