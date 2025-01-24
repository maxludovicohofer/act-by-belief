using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaxHofer.ActByBelief
{
    using Importance = Single;
    using NeedBelief = Belief<Need>;

    /// <summary>
    /// Manages global AI settings and need <see cref="Belief"/>s.
    /// </summary>
    public class Brain
    {
        /// <summary>
        /// Creates a function that debounces a need's OnChange event, in order to simulate human reaction time.
        /// </summary>
        /// <param name="need">The need to create the function for.</param>
        /// <param name="motive">The need's <see cref="Motive"/>, used to change the debounce time accordingly.</param>
        /// <returns>The trigger change function to supply to the need.</returns>
        delegate NeedBelief.TriggerChangeFunction DebouncedTrigger(NeedBelief need, Motive motive);

        /// <summary>
        /// The minimum number of seconds to wait before reacting to a need's value change.
        /// Seconds to react increment on lower priority <see cref="Motive"/>s.
        /// </summary>
        public float ReactionTime
        {
            get => minSecondsToReact;
            set
            {
                debouncedTrigger =
                    value != 0
                        ? (need, motive) =>
                            (triggerChange, _) =>
                                unityBody.Debounce(
                                    $"{need.GetHashCode()}.{nameof(triggerChange)}",
                                    triggerChange,
                                    ActByBelief.Need.GetReactionTime(value, motive)
                                )
                        : null;

                foreach (var (need, motive) in needs.Values)
                {
                    need.TriggerChange =
                        debouncedTrigger != null ? debouncedTrigger(need, motive) : null;
                }

                minSecondsToReact = value;
            }
        }

        /// <summary>
        /// Activates or deactivates debug UI.
        /// </summary>
        public static bool ShowDebug
        {
#if DEBUG
            get => UnityBody.ShowDebug;
            set => UnityBody.ShowDebug = value;
#else
            get => false;
            set { }
#endif
        }

        /// <summary>
        /// The minimum human reaction time is estimated between 100-200ms (<seealso href="https://en.wikipedia.org/wiki/Mental_chronometry"/>).
        /// </summary>
        const float MIN_HUMAN_REACTION_TIME = .15f;

        /// <summary>
        /// The body to refer to when working with the Unity engine.
        /// </summary>
        readonly UnityBody unityBody;

        /// <summary>
        /// Offsets to the default importance of <see cref="Motive"/>s. These add "personality" to the <see cref="Brain"/>.
        /// </summary>
        readonly Importance[] offsets;

        /// <summary>
        /// Since needs are not always declared as properties of a class, this <see cref="Dictionary"/> stores them to extract them in <see cref="ExtractBeliefs"/>.
        /// </summary>
        readonly Dictionary<string, (NeedBelief need, Motive motive)> needs = new();

        /// <summary>
        /// The classes containing <see cref="Belief"/>s as public properties.
        /// </summary>
        readonly object[] beliefContainers;

        /// <summary>
        /// Creates a function that debounces a need's OnChange event, in order to simulate human reaction time.
        /// </summary>
        DebouncedTrigger debouncedTrigger;

        /// <summary>
        /// The minimum number of seconds to wait before reacting to a need's value change.
        /// Seconds to react increment on lower priority <see cref="Motive"/>s.
        /// </summary>
        float minSecondsToReact;

        /// <summary>
        /// Used to create sequential debug IDs for needs.
        /// </summary>
        ulong needID;

        /// <summary>
        /// Creates a <see cref="Brain"/> by specifying its "personality" explicitly in the form of <see cref="offsets"/>.
        /// </summary>
        /// <param name="body">The body to refer to when working with the Unity engine.</param>
        /// <param name="beliefContainers">The classes containing <see cref="Belief"/>s as public properties.</param>
        /// <param name="survival">The offset for <see cref="Motive.Survival"/>.</param>
        /// <param name="love">The offset for <see cref="Motive.Love"/>.</param>
        /// <param name="achievement">The offset for <see cref="Motive.Achievement"/>.</param>
        /// <param name="minImportance">The minimum allowed importance for <see cref="Motive"/>s. Used to avoid <see cref="Motive"/>s potentially being ignored.</param>
        public Brain(
            MonoBehaviour body,
            object[] beliefContainers,
            Importance survival,
            Importance love,
            Importance achievement,
            Importance minImportance = 0
        )
        {
            unityBody = new(body);
            ReactionTime = MIN_HUMAN_REACTION_TIME;

            this.beliefContainers = beliefContainers ?? new object[0];

            offsets = ActByBelief.Need.ClampOffsets(survival, love, achievement, minImportance);

#if DEBUG
            // Delayed to give some time to create all belief containers
            unityBody.Debounce(
                $"{GetHashCode()}.{nameof(unityBody)}.{nameof(unityBody.Debug)}",
                () => unityBody.Debug(ExtractBeliefs()),
                .1f
            );
#endif
        }

        /// <summary>
        /// Creates a <see cref="Brain"/> by specifying its "personality" explicitly in the form of <see cref="offsets"/>.
        /// </summary>
        /// <param name="body">The body to refer to when working with the Unity engine.</param>
        /// <param name="survival">The offset for <see cref="Motive.Survival"/>.</param>
        /// <param name="love">The offset for <see cref="Motive.Love"/>.</param>
        /// <param name="achievement">The offset for <see cref="Motive.Achievement"/>.</param>
        /// <param name="minImportance">The minimum allowed importance for <see cref="Motive"/>s. Used to avoid <see cref="Motive"/>s potentially being ignored.</param>
        public Brain(
            MonoBehaviour body,
            Importance survival,
            Importance love,
            Importance achievement,
            Importance minImportance = 0
        )
            : this(body, null, survival, love, achievement, minImportance) { }

        /// <summary>
        /// Creates a <see cref="Brain"/> by specifying its "personality" as a maximum offset factor. Individual offsets are then calculated randomly based on this factor.
        /// </summary>
        /// <param name="body">The body to refer to when working with the Unity engine.</param>
        /// <param name="beliefContainers">The classes containing <see cref="Belief"/>s as public properties.</param>
        /// <param name="personality">The maximum offset that can be assigned to a <see cref="Motive"/>.</param>
        /// <param name="minImportance">The minimum allowed importance for <see cref="Motive"/>s. Used to avoid <see cref="Motive"/>s potentially being ignored.</param>
        public Brain(
            MonoBehaviour body,
            object[] beliefContainers,
            Importance personality = 0,
            Importance minImportance = .01f
        )
            : this(
                body,
                beliefContainers,
                ActByBelief.Need.GetRandomOffset(personality),
                ActByBelief.Need.GetRandomOffset(personality),
                ActByBelief.Need.GetRandomOffset(personality),
                minImportance
            ) { }

        /// <summary>
        /// Creates a <see cref="Brain"/> by specifying its "personality" as a maximum offset factor. Individual offsets are then calculated randomly based on this factor.
        /// </summary>
        /// <param name="body">The body to refer to when working with the Unity engine.</param>
        /// <param name="personality">The maximum offset that can be assigned to a <see cref="Motive"/>.</param>
        /// <param name="minImportance">The minimum allowed importance for <see cref="Motive"/>s. Used to avoid <see cref="Motive"/>s potentially being ignored.</param>
        public Brain(
            MonoBehaviour body,
            Importance personality = 0,
            Importance minImportance = .01f
        )
            : this(body, null, personality, minImportance) { }

        /// <summary>
        /// Creates a need <see cref="Belief"/> that follows a <see cref="Motive"/>.
        /// Needs are the core values that decisions of the <see cref="Brain"/> should be based upon, e.g. the need to eat, to reach a target, to have friends.
        /// Needs evaluate their intensity (number from 0 to 1) as the result of their <see cref="Interpret"/> function.
        /// Then, this result is weighed by the need's <see cref="Motive"/>, in order to evaluate its <see cref="Importance"/> for the entire <see cref="Brain"/>.
        /// </summary>
        /// <param name="need">The need <see cref="Belief"/> to create.</param>
        /// <param name="motive">The <see cref="Motive"/> followed by the need.</param>
        /// <returns>The created need <see cref="Belief"/>.</returns>
        public NeedBelief Need(NeedBelief need, Motive motive = Motive.Survival)
        {
            var createdNeed = need ?? new(_ => ActByBelief.Need.URGENT);

            createdNeed.Reinterpret = ActByBelief.Need.InterpretIntensityAsImportance(
                motive,
                offsets
            );

            if (debouncedTrigger != null)
                createdNeed.TriggerChange = debouncedTrigger(createdNeed, motive);

            needs.Add(
                $"{unityBody.Body.GetType().Name}({motive}).{nameof(Need)}{++needID}",
                (createdNeed, motive)
            );

            return createdNeed;
        }

        /// <summary>
        /// Kills all <see cref="Belief"/>s associated with this <see cref="Brain"/>.
        /// </summary>
        public void Die()
        {
            KillContainers();

            unityBody.Die();
        }

        /// <summary>
        /// Kills all <see cref="Belief"/>s that are public properties in the provided <see cref="Belief"/> containers.
        /// </summary>
        /// <param name="beliefContainers">The <see cref="Belief"/> containers where to kill public <see cref="Belief"/>s.</param>
        public void Kill(params object[] beliefContainers)
        {
            if (beliefContainers.Length == 0)
                return;

            KillContainers(beliefContainers);
        }

        /// <summary>
        /// Kills all <see cref="Belief"/>s that are public properties in the provided <see cref="Belief"/> containers.
        /// If no containers are provided, kills all containers associated with this <see cref="Brain"/>.
        /// </summary>
        /// <param name="beliefContainers">The <see cref="Belief"/> containers where to kill public <see cref="Belief"/>s, or empty to kill this <see cref="Brain"/>.</param>
        void KillContainers(params object[] beliefContainers)
        {
            foreach (var belief in ExtractBeliefs(beliefContainers).Values)
                belief.Die();
        }

        /// <summary>
        /// Extracts all <see cref="Belief"/>s that are public properties in the provided containers.
        /// If no containers are provided, will look for <see cref="Belief"/>s in this <see cref="Brain"/>.
        /// </summary>
        /// <param name="beliefContainers">The <see cref="Belief"/> containers where to extract public <see cref="Belief"/>s, or empty to extract from the containers associated with this <see cref="Brain"/>.</param>
        /// <returns>A dictionary of extracted <see cref="Belief"/> names and extracted <see cref="Belief"/>s.</returns>
        Dictionary<string, IBelief> ExtractBeliefs(params object[] beliefContainers)
        {
            var suicide = beliefContainers.Length == 0;

            IEnumerable containers = suicide
                ? new List<object>(this.beliefContainers) { unityBody.Body }
                : beliefContainers;

            Dictionary<string, IBelief> beliefs = new();

            if (suicide)
            {
                foreach (var need in needs)
                    beliefs.Add(need.Key, need.Value.need);
            }

            foreach (var container in containers)
            {
                // Find public beliefs
                foreach (var property in container.GetType().GetProperties())
                {
                    if (property.PropertyType.GetInterface(nameof(IBelief)) != null)
                    {
                        beliefs.Add(
                            $"{container.GetType().Name}.{property.Name}",
                            (IBelief)property.GetValue(container)
                        );
                    }
                }
            }

            return beliefs;
        }
    }
}
