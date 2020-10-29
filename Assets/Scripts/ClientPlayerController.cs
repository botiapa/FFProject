using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPlayerController : NetworkedPlayerController
{
	const uint BUFFERSIZE = 1024;

	public Queue<StateMessage> stateMessages = new Queue<StateMessage>();

	private ClientState[] client_state_buffer = new ClientState[BUFFERSIZE];
	private Inputs[] client_input_buffer = new Inputs[BUFFERSIZE];

	void _Update()
	{
		timer += Time.deltaTime;
		while (timer >= Time.fixedDeltaTime) {
			timer -= Time.fixedDeltaTime;

			bool up = Input.GetKey(KeyCode.W);
			bool down = Input.GetKey(KeyCode.S);
			bool left = Input.GetKey(KeyCode.A);
			bool right = Input.GetKey(KeyCode.D);
			var inputs = new Inputs(up, down, right, left, Input.GetButtonDown("Jump"), Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

			var inputMessage = new InputMessage(inputs, tickNumber);
			sendInputToServer(inputMessage);

			uint buffer_slot = tickNumber % BUFFERSIZE;
			client_input_buffer[buffer_slot] = inputs;
			client_state_buffer[buffer_slot] = new ClientState(rb.position, rb.rotation);

			Rotate(inputs);
			addForcesToPlayer(inputs);

			Physics.Simulate(Time.fixedDeltaTime);

			++tickNumber;
		}

		if(stateMessages.Count > 0) 
		{
			var state_msg = stateMessages.Dequeue();
			while (stateMessages.Count > 0) {
				state_msg = stateMessages.Dequeue();
			}

			uint buffer_slot = state_msg.tickNumber % BUFFERSIZE;
			Vector3 pos_error = state_msg.position - client_state_buffer[buffer_slot].position;
			float rot_error = Quaternion.Angle(state_msg.rotation, client_state_buffer[buffer_slot].rotation);

			if (pos_error.sqrMagnitude > 0.0000001f || rot_error > 1f)
			{
				Debug.Log($"msg: {state_msg.rotation} actual: {client_state_buffer[buffer_slot].rotation}");
				Debug.Log($"Correcting for error at tick {state_msg.tickNumber} (rewinding {tickNumber - state_msg.tickNumber} ticks) Magnitude: pos:{pos_error} && rot:{rot_error}");
				// Rewind & Replay
				rb.position = state_msg.position;
				rb.rotation = state_msg.rotation;
				rb.velocity = state_msg.velocity;
				rb.angularVelocity = state_msg.angularVelocity;

				uint rewind_tick_number = state_msg.tickNumber;
				while (rewind_tick_number < tickNumber) 
				{
					buffer_slot = rewind_tick_number % BUFFERSIZE;
					client_state_buffer[buffer_slot].position = rb.position;
					client_state_buffer[buffer_slot].rotation = rb.rotation;

					Rotate(client_input_buffer[buffer_slot]);
					addForcesToPlayer(client_input_buffer[buffer_slot]);
					Physics.Simulate(Time.fixedDeltaTime);
					++rewind_tick_number;
				}
			}
		}
	}

    private void Update()
    {
		timer += Time.deltaTime;
		while (timer >= Time.fixedDeltaTime)
		{
			timer -= Time.fixedDeltaTime;
			bool up = Input.GetKey(KeyCode.W);
			bool down = Input.GetKey(KeyCode.S);
			bool left = Input.GetKey(KeyCode.A);
			bool right = Input.GetKey(KeyCode.D);
			var inputs = new Inputs(up, down, right, left, Input.GetButtonDown("Jump"), Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

			Rotate(inputs);
			addForcesToPlayer(inputs);
			sendMovementToServer(new MovementMessage(ClientManager.Instance.client.ID, rb.position, rb.rotation, rb.velocity, rb.angularVelocity, tickNumber));
			Physics.Simulate(Time.fixedDeltaTime);

			++tickNumber;
		}
		
	}
}

class ClientState
{
	public Vector3 position;
	public Quaternion rotation;

    public ClientState(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}
