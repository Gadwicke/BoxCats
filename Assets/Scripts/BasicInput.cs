using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicInput : MonoBehaviour
{
    public CharacterController Controller;

    public float MaximumSpeed;

    // Update is called once per frame
    void Update()
    {
        var speed = Input.GetKey(KeyCode.LeftShift) ? MaximumSpeed * 3 : MaximumSpeed;

        var xz = (transform.rotation * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"))).normalized * speed;

        Controller.Move(xz + Physics.gravity);
    }
}
