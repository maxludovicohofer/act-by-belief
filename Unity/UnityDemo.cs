using UnityEngine;
using MaxHofer.ActByBelief;

public class UnityDemo
{
    // Inputs
    public Belief<Vector3> LastKnownPlayerPosition { get; } = new(); //! Receives data from external sources

    // Outputs
    public Belief<Vector3> TargetPosition { get; }

    public UnityDemo(UnityDemoBody body)
    {
        // Initialize an AI with a semi-random personality and a fast reaction time (100ms)
        var brain = new Brain(body, personality: .5f) { ReactionTime = .1f };

        // The AI needs to kiss the player
        var kissPlayer = brain.Need(
            new(
                signal =>
                    // The need increases as the player gets closer
                    Need.Near(
                        signal.Get(LastKnownPlayerPosition),
                        signal.Get(body.Position),
                        maxDistance: 200
                    )
            ),
            // The AI wants to kiss the player out of love
            Motive.Love
        );

        TargetPosition = new(
            signal =>
                // If the AI wants to kiss the player, should move toward the player
                signal.Get(kissPlayer) > Need.ABSENT
                    ? signal.Get(LastKnownPlayerPosition)
                    // Else, turn back to its start position
                    : body.StartPosition
        );
    }
}

public class UnityDemoBody : MonoBehaviour
{
    public Vector3 StartPosition { get; } = new();

    // Beliefs that always represent reality can be placed in the body
    public Belief<Vector3> Position { get; } = new();

    [SerializeField]
    CharacterController controller;

    [SerializeField]
    float movementSpeed = 1;

    UnityDemo brain;

    // Add brain reference
    void Awake() => brain = new UnityDemo(this);

    void FixedUpdate()
    {
        var pathToTarget = brain.TargetPosition.Value - Position.Value;

        // Check if target reached
        if (pathToTarget.magnitude < 1)
            return;

        // Move to target
        controller.Move(movementSpeed * pathToTarget.normalized);

        // Sense updated position
        Position.Sense(transform.position);
    }
}
