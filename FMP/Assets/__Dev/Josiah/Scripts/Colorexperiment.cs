using UnityEngine;

public class Colorexperiment : MonoBehaviour
{
    public enum Colorchoice
    {
        Yellow,
        Magenta,
        Red,
        Green,
        Blue,
        Purple

    }

    public Colorchoice theColor;
    public bool isPrimary;

    // Update is called once per frame
    void Update()
    {
        switch (theColor)
        {
            case Colorchoice.Yellow:
                isPrimary = true;
                break;
                case Colorchoice.Magenta:  
                isPrimary = false;
                break;
                case Colorchoice.Red:
                isPrimary = true;
                break;
                case Colorchoice.Green:
                isPrimary = false;
                break;
                case Colorchoice.Blue:
                    isPrimary = true;
                break;
                case Colorchoice .Purple:
                isPrimary = false;
                    break;



        }
    }
}
