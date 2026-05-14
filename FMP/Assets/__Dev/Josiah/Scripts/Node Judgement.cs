using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class NodeJudgement : MonoBehaviour
{
    [Header("Node judgementation")]
    public int Node_calculations = 5;
    public bool Node_opinion;
    public bool Node_attitude;
    public bool Idea_judgement;
    public bool Idea_agreement;
    public bool isNode;
    private void Start()
    {
        Node_opinions();
        Node_attitudes();


    }

    private void Update()
    {
        idea_judgement();
        Idea_agreements();
    }
    public void Node_opinions()
    {
        if (Node_opinion == true)
            print("I love you and you influence my opinions a lot");

        else if (Node_opinion == false)
        {
            print("You go against everything i stand for, i hate what you love and love what you hate");
        }

        else
        {
            print(" I dont know who you are and you dont affect me");
        }

        
    }

    

    public void Node_attitudes()
    {
        if (Node_attitude == true)
            print("I really have faith in this idea");
        else if (Node_attitude == false)
        {
            print("I have absolutely zero faith in this idea");
        }
        else
        {
            print(" I don't know if whether to trust this idea or not");
        }
    }


    public void idea_judgement()
    {
        if (Idea_judgement == true)
            print("You are highly respected and we're happy to have you as a memeber of our group");
        else if (Idea_agreement == false)
            print("Your access to our group has been revoked");
        else
        {
            print("I'm too sure if we can trust you just yet");
        }

    }

    public void Idea_agreements()
    {
        if (Idea_agreement == true)
            print("I strongly agree");
        else if (Idea_agreement == false)
            print("You have no relevance in this topic");
        else
        {
            print("I completely disagree");
        }

    }

    public void Node_detector()
    {
        if (isNode == true)
        {
            print("This Node is accepted in the right group");

        }
        else if (isNode == false)
        {
            print("This Node doesn't meet our expectations");
        }
    }





}
