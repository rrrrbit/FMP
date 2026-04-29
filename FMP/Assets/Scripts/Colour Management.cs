using UnityEngine;
using System.Collections;

public class ColourManagement : MonoBehaviour
{
    public enum Node_colours
    {
        Brown,
        Blue,
        Red,
        Green,
        Black,
        purple,
        yellow,
        Magenta



    }

    public Node_colours colour;
    public bool isDefault;
    public bool colour_detector;

    void Update()
    {
        switch (colour)
        {   case Node_colours.Brown:
            isDefault = true;
             break;
                case Node_colours.Blue:
                isDefault = false;
                break;
                case Node_colours.Red:
                isDefault = false;
                break;
                case Node_colours.Green:
                isDefault = true;
                    break;
                case Node_colours.Black:
                isDefault = false; 
                break;
            case Node_colours.purple:
                isDefault = false;
                break;
                case Node_colours.yellow:
                isDefault = false;
                break;
            case Node_colours.Magenta:
                isDefault = false;
                break;



           
        }
    }
    void Colour_Verifier()
    {
        if (colour_detector == true)
        {
            print("Colour is brown");

        }
        else if (colour_detector == false)
        {
            print("Colour is blue");

        }
        else if (colour_detector == false)
        {
            print("Colour is Red");
        }
        else if (colour_detector == true)
        {
            print("Colour is Green");
        }
        else if (colour_detector == false)
        {
            print("Colour is black");
        }
        else if (colour_detector == false)
        {
            print("Colour is purple");
        }
    }


}
