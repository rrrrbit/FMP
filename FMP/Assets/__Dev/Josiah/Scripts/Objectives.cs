using System;
using UnityEngine;
public class Objectives : MonoBehaviour
{
    [SerializeField] 
    public class  Node_Objective { }

    public Action OnComplete;

    public Action OnValuechange;

    bool ObjectiveIsClear;
    public string EventTrigger { get; }
    public bool IsComplete { get; private set; }
    public int MaxValue { get; }
    public int CurrentValue {  get; private set; }


    void Start()
    {
        if (ObjectiveIsClear == true)
        {
            print("assign tasks");
            while (true)
            {

            }

        }
        else if (ObjectiveIsClear == false)
        {
            print("Make sure objective is clear before you assign tasks");
        }   while (false)
        {

        }
      
        
    }

    public void ObjectiveAdamant()
    {
        do
        {
           


        } while (true);
        // Experimenting with this state and putting in the method to detect if the objectives are actually clear


    }
    

    void Update()
    {
        while (ObjectiveIsClear == true)
        {
            Debug.Log("Make nodes have positive influences");

        }
    }
}
