using UnityEngine;
using System.Collections;
using FableLabs.Anim;
using UnityEngine.UI;

public enum Symbol
{

}

public class Card
{
    public Symbol Symbol;
}

public class Game : MonoBehaviour 
{
    public Slider Timer;
    public float TurnTime = 4.0f;

    public void Turn()
    {
        Tween.FromTo<float>(Timer, f => Timer.value = f, 1.0f, 0.0f, 4.0f)
            .Curve(null)
            .OnComplete(EndMove);

    }

    public void EndMove()
    {
        Turn();
    }

    public void Start()
    {
        Turn();
    }
}
