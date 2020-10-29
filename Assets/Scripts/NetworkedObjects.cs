using DarkRift;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkriftSerializationExtensions;

public class Inputs : IDarkRiftSerializable
{
    public bool up;
    public bool down;
    public bool left;
    public bool right;
    public bool jump;
    public float hRot;
    public float vRot;

    public Inputs() {}

    public Inputs(bool up, bool down, bool left, bool right, bool jump, float hRot, float vRot)
    {
        this.up = up;
        this.down = down;
        this.left = left;
        this.right = right;
        this.jump = jump;
        this.hRot = hRot;
        this.vRot = vRot;
    }

    public void Deserialize(DeserializeEvent e)
    {
        up = e.Reader.ReadBoolean();
        down = e.Reader.ReadBoolean();
        left = e.Reader.ReadBoolean();
        right = e.Reader.ReadBoolean();
        jump = e.Reader.ReadBoolean();
        hRot = e.Reader.ReadSingle();
        vRot = e.Reader.ReadSingle();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(up);
        e.Writer.Write(down);
        e.Writer.Write(left);
        e.Writer.Write(right);
        e.Writer.Write(jump);
        e.Writer.Write(hRot);
        e.Writer.Write(vRot);
    }
}

public class InputMessage : IDarkRiftSerializable
{
    public Inputs inputs;
    public uint tickNumber;

    public InputMessage() {}

    public InputMessage(Inputs inputs, uint tickNumber)
    {
        this.inputs = inputs;
        this.tickNumber = tickNumber;
    }

    public void Deserialize(DeserializeEvent e)
    {
        inputs = e.Reader.ReadSerializable<Inputs>();
        tickNumber = e.Reader.ReadUInt32();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(inputs);
        e.Writer.Write(tickNumber);
    }
}

public class StateMessage : IDarkRiftSerializable
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    public uint tickNumber;

    public StateMessage() {}

    public StateMessage(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, uint tickNumber)
    {
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
        this.angularVelocity = angularVelocity;
        this.tickNumber = tickNumber;
    }

    public void Deserialize(DeserializeEvent e)
    {
        position = e.Reader.ReadVector3();
        rotation = e.Reader.ReadQuaternion();
        velocity = e.Reader.ReadVector3();
        angularVelocity = e.Reader.ReadVector3();
        tickNumber = e.Reader.ReadUInt32();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.WriteVector3(position);
        e.Writer.WriteQuaternion(rotation);
        e.Writer.WriteVector3(velocity);
        e.Writer.WriteVector3(angularVelocity);
        e.Writer.Write(tickNumber);
    }
}

public class MovementMessage : IDarkRiftSerializable
{
    public ushort ID;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    public uint tickNumber;

    public MovementMessage() { }

    public MovementMessage(ushort ID, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, uint tickNumber)
    {
        this.ID = ID;
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
        this.angularVelocity = angularVelocity;
        this.tickNumber = tickNumber;
    }

    public void Deserialize(DeserializeEvent e)
    {
        ID = e.Reader.ReadUInt16();
        position = e.Reader.ReadVector3();
        rotation = e.Reader.ReadQuaternion();
        velocity = e.Reader.ReadVector3();
        angularVelocity = e.Reader.ReadVector3();
        tickNumber = e.Reader.ReadUInt32();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ID);
        e.Writer.WriteVector3(position);
        e.Writer.WriteQuaternion(rotation);
        e.Writer.WriteVector3(velocity);
        e.Writer.WriteVector3(angularVelocity);
        e.Writer.Write(tickNumber);
    }
}
