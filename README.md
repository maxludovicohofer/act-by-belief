# How to use

Beliefs are a flexible data structure representing perceived info and beliefs on the world.

```c#
// Inputs, updated by external sources
public Belief<Vector3> LastKnownPlayerPosition { get; } = new();

// Outputs, processed by brain
public Belief<Vector3> TargetPosition { get; }
```

The brain is the belief processor of an agent.

```c#
// Initialize an AI with a semi-random personality and a fast reaction time (100ms)
var brain = new Brain(body, personality: .5f)
{
    ReactionTime = .1f
};
```

A brain must define need beliefs, which are beliefs that numerically estimate a need's intensity, based on input beliefs. They also require a motive, that can be survival, love, or achievement, and acts as a weight on the need. These motives are based on Maslow's pyramid of needs.

```c#
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
```

Beliefs communicate through signals, that ensure output beliefs are updated when input beliefs change.

```c#
TargetPosition = new(
    signal =>
        // If the AI wants to kiss the player, should move toward the player
        signal.Get(kissPlayer) > Need.ABSENT
            ? signal.Get(LastKnownPlayerPosition)
            // Else, turn back to its start position
            : body.StartPosition
);
```

The Unity folder contains a demo with a usage example.
