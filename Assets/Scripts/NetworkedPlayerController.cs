using DarkRift;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPlayerController : MonoBehaviour
{
    public Rigidbody rb;
    public Transform cam;

    public float mouseSensitivity = 8f;
    public float uplimit = -50;
    public float downLimit = 50;

    public float speed = 10.0f;
    public float gravity = 10.0f;
    public float maxVelocityChange = 10.0f;
    public bool canJump = true;
    public float jumpForce = 20f;

    protected bool grounded = false;
    protected float timer = 0.0f;
    protected uint tickNumber = 0;

    void OnCollisionStay()
    {
        grounded = true;
    }

    protected void sendInputToServer(InputMessage inputMessage)
    {
        using (var inputMsg = Message.Create((ushort)Tags.Input, inputMessage)) 
        {
            ClientManager.Instance.client.SendMessage(inputMsg, SendMode.Unreliable);
        }
    }

    protected void sendMovementToServer(MovementMessage movementMessage) 
    {
        using (var moveMsg = Message.Create((ushort)Tags.Movement, movementMessage)) 
        {
            ClientManager.Instance.client.SendMessage(moveMsg, SendMode.Unreliable);
        }
    }

    public void addForcesToPlayer(Inputs inputs)
    {
        Vector3 velocityChange = getForceForInput(inputs);
        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        if (velocityChange.y > 0) // Jumping
            grounded = false;

        // We apply gravity manually for more tuning control
        rb.AddForce(new Vector3(0, -gravity * GetComponent<Rigidbody>().mass, 0));
    }
    public void Rotate(Inputs inputs)
    {
        transform.Rotate(0, inputs.hRot * mouseSensitivity, 0);
        cam.Rotate(-inputs.vRot * mouseSensitivity, 0, 0);

        Vector3 euler = cam.localEulerAngles;
        if (euler.x > 180) euler.x -= 360; // reset
            euler.x = Mathf.Clamp(euler.x, uplimit, downLimit);
        cam.localRotation = Quaternion.Euler(euler);
    }

    protected Vector3 getForceForInput(Inputs inputs)
    {
        var hAxis = (inputs.left ? 1 : 0) + (inputs.right ? -1 : 0);
        var vAxis = (inputs.up ? 1 : 0) + (inputs.down ? -1 : 0);

        // Calculate how fast we should be moving
        Vector3 targetVelocity = new Vector3(hAxis, 0, vAxis);
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= speed;

        // Apply a force that attempts to reach our target velocity
        Vector3 velocity = rb.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

        if (grounded && canJump && inputs.jump)
            velocityChange.y += jumpForce / rb.mass; // Jump and take the mass into account
        return velocityChange;
    }
}
