using JetBrains.Annotations;
using UnityEngine;

public class Toten : MonoBehaviour
{
    public float Duration;
    public float Delay;
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;

    private float _age;

    [UsedImplicitly]
    private void Start ()
	{
	    _age = 0;
	}

    [UsedImplicitly]
    private void Update ()
	{
	    if ((Duration <= 0 || _age < (Duration + Delay)) && _age >= Delay)
	    {
            transform.localPosition = transform.localPosition + Position * Time.deltaTime;
            transform.localEulerAngles = transform.localEulerAngles + Rotation * Time.deltaTime;
            transform.localScale = transform.localScale + Scale * Time.deltaTime;
	    }
        _age += Time.deltaTime;
    }
}
