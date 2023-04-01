using UnityEngine;

public class RotatingAI : SomeAI
{
    private int _rotationFramesAmount = 0;
    private int _framesOfRotation = 0;
    private float _initialRotation = 0f;
    private float _desiredRotation = 0f;

    public override void PrepareAction()
    {
        _initialRotation = transform.rotation.eulerAngles.y;
        _desiredRotation = transform.rotation.eulerAngles.y + (Random.Range(0f, 90f) - 45f);
        _rotationFramesAmount = Random.Range(30, 60);
        _framesOfRotation = 0;
    }

    public override void Act()
    {
        if (_framesOfRotation >= _rotationFramesAmount)
        {
            _aiManager.Transition("Idle");
            _framesOfRotation = 0;
        }
        else
        {
            transform.rotation = Quaternion.Euler(Vector3.Lerp(
                new Vector3(0f, _initialRotation, 0f),
                new Vector3(0f, _desiredRotation, 0f),
                (float) _framesOfRotation / _rotationFramesAmount
            ));
            _framesOfRotation++;
        }
    }
}
