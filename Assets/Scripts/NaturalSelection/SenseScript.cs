using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SenseScript : MonoBehaviour
{
    public AgentBehaviour agent; 
   
    void OnTriggerStay2D(Collider2D collider)
    {
        if(collider.CompareTag("Food"))
        {
            if(!agent.IfChasingFood())
                agent.SetPositionToMoveTowards(collider.transform.localPosition);
        }   
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.CompareTag("Player"))
        {
            float diff = collider.GetComponentInParent<AgentBehaviour>().GetChromosomes().size - agent.GetChromosomes().size;
            if(diff>0.2f)
            {
                Vector2 newPosition = 2f * agent.transform.localPosition - collider.transform.localPosition;
                float width = EcoSystemController.instance.regionWidth;
                newPosition.x = Mathf.Clamp(newPosition.x,-width,width);
                newPosition.y = Mathf.Clamp(newPosition.y,-width,width);
                agent.SetPositionToMoveTowards(newPosition);
            }
        }           
    }
}
