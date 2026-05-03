using System.Collections;
using UnityEngine;

public class DoubleDoorController : BaseDoorController
{
    public Transform leftPivot;
    public Transform rightPivot;

    public float leftOpenAngle  = -90f;
    public float rightOpenAngle =  90f;

    private Quaternion _leftClosed,  _leftOpen;
    private Quaternion _rightClosed, _rightOpen;
    private bool _isOpen = false;

    protected override void Init()
    {
        _leftClosed  = leftPivot.rotation;
        _leftOpen    = Quaternion.Euler(leftPivot.eulerAngles  + new Vector3(0, leftOpenAngle,  0));

        _rightClosed = rightPivot.rotation;
        _rightOpen   = Quaternion.Euler(rightPivot.eulerAngles + new Vector3(0, rightOpenAngle, 0));
    }

    protected override IEnumerator Toggle()
    {
        Quaternion leftTarget  = _isOpen ? _leftClosed  : _leftOpen;
        Quaternion rightTarget = _isOpen ? _rightClosed : _rightOpen;
        _isOpen = !_isOpen;

        while (Quaternion.Angle(leftPivot.rotation,  leftTarget)  > 0.01f ||
               Quaternion.Angle(rightPivot.rotation, rightTarget) > 0.01f)
        {
            leftPivot.rotation  = Quaternion.Lerp(leftPivot.rotation,  leftTarget,  Time.deltaTime * openSpeed);
            rightPivot.rotation = Quaternion.Lerp(rightPivot.rotation, rightTarget, Time.deltaTime * openSpeed);
            yield return null;
        }

        leftPivot.rotation  = leftTarget;
        rightPivot.rotation = rightTarget;
    }
}
