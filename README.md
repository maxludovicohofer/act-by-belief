# Act by belief

A decision tree AI workflow for natural thought inspired by the [Maslow hierarchy of needs](https://wikipedia.org/wiki/Maslow%27s_hierarchy_of_needs).

## How to install

Clone the repository as a git submodule and use it. If not using git, just download the code.

## How to use

Beliefs are a flexible data structure representing perceived info and beliefs on the world.

```csharp
// Inputs, updated by external sources
public Belief<Vector3> LastKnownPlayerPosition { get; } = new();

// Outputs, processed by brain
public Belief<Vector3> TargetPosition { get; }
```

The brain is the belief processor of an agent.

```csharp
// Initialize an AI with a fast reaction time (100ms)
var brain = new Brain(body) { ReactionTime = .1f };
```

A brain must define need beliefs, which are beliefs that numerically estimate a need's intensity, based on input beliefs. They also require a motive, that can be survival, love, or achievement, and acts as a weight on the need. These motives are based on Maslow's pyramid of needs.

```csharp
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

A brain can define a personality by changing the motives' default values. For example, an agent with Love at 0.9 processes beliefs differently than one with Love at 0.1. The Brain constructor also includes settings to offset all motives by a random amount, limited by the given value.

```csharp
// Initialize an AI with a semi-random personality (motives are offset by a max of +-0.5)
var brain = new Brain(body, personality: .5f);
```

Beliefs communicate through signals, that ensure output beliefs are updated when input beliefs change.

```csharp
TargetPosition = new(
    signal =>
        // If the AI wants to kiss the player, should move toward the player
        signal.Get(kissPlayer) > Need.ABSENT
            ? signal.Get(LastKnownPlayerPosition)
            // Else, turn back to its start position
            : body.StartPosition
);
```

The `Unity` folder contains a demo with a usage example.
