using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : ProSprite
{
    // Start is called before the first frame update
    void Start()
    {
        Setup(32, 32);
    }

    // Render is called before the camera renders a frame
    override protected void Render()
    {
        Circle[] myCircleArray = new Circle[] { new Circle(center, 64, 0) };
        DrawCircles(myCircleArray);
    }
}
