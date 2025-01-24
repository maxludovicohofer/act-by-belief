using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaxHofer.ActByBelief
{
    /// <summary>
    /// The body for the <see cref="Brain"/> when working with the Unity engine.
    /// </summary>
    internal class UnityBody
    {
        /// <summary>
        /// The body <see cref="MonoBehaviour"/> object.
        /// </summary>
        internal MonoBehaviour Body { get; }

        /// <summary>
        /// Dictionary containing all debounce <see cref="Coroutine"/>s, in order to effectively debounce them.
        /// </summary>
        static readonly Dictionary<string, Coroutine> debounceCoroutines = new();

        /// <summary>
        /// Creates a <see cref="UnityBody"/> by setting its <see cref="Body"/>.
        /// </summary>
        /// <param name="body">The body <see cref="MonoBehaviour"/> object.</param>
        /// <exception cref="NullReferenceException">Exception thrown if <see cref="body"/> is null.</exception>
        internal UnityBody(MonoBehaviour body)
        {
#if DEBUG
            if (body == null)
                throw new ArgumentNullException(nameof(body));
#endif
            Body = body;
        }

        /// <summary>
        /// Debounces a function by the provided seconds.
        /// </summary>
        /// <param name="globalDebounceID">The globally unique ID to use to debounce the function.</param>
        /// <param name="debounceFunction">The function to debounce.</param>
        /// <param name="debounceSeconds">The seconds to debounce the function for.</param>
        internal void Debounce(
            string globalDebounceID,
            Action debounceFunction,
            float debounceSeconds
        )
        {
            if (!debounceCoroutines.ContainsKey(globalDebounceID))
            {
                debounceCoroutines.Add(
                    globalDebounceID,
                    Body.StartCoroutine(
                        DebounceCoroutine(globalDebounceID, debounceFunction, debounceSeconds)
                    )
                );
            }
        }

        /// <summary>
        /// Removes the <see cref="Brain"/> from the <see cref="UnityDebugger"/>.
        /// </summary>
        internal void Die()
        {
#if DEBUG
            if (debugger == null || brainDebugName == default)
                return;

            debugger.RemoveBrain(brainDebugName);
#endif
        }

        /// <summary>
        /// Waits for the provided seconds, then executes the function.
        /// </summary>
        /// <param name="globalDebounceID">The globally unique ID used to debounce the function.</param>
        /// <param name="debounceFunction">The function to execute after the wait.</param>
        /// <param name="debounceSeconds">The seconds to wait before executing the function.</param>
        /// <returns>The output required by <see cref="StartCoroutine"/>.</returns>
        static IEnumerator DebounceCoroutine(
            string globalDebounceID,
            Action function,
            float debounceSeconds
        )
        {
            yield return new WaitForSeconds(debounceSeconds);

            function();

            debounceCoroutines.Remove(globalDebounceID);
        }

#if DEBUG
        /// <summary>
        /// Shows or hides <see cref="UnityDebugger"/> UI.
        /// </summary>
        internal static bool ShowDebug
        {
            get => debugger != null && debugger.enabled;
            set
            {
                if (debugger == null)
                {
                    debugger = new GameObject(
                        "ActByBeliefDebugger",
                        typeof(UnityDebugger)
                    ).GetComponent<UnityDebugger>();
                }

                debugger.enabled = value;
            }
        }

        /// <summary>
        /// The name of the <see cref="Brain"/> for <see cref="UnityDebugger"/> purposes.
        /// </summary>
        string brainDebugName;

        /// <summary>
        /// An incremental ID counter to automatically assign <see cref="UnityDebugger"/> names to <see cref="Brain"/>s.
        /// </summary>
        static ulong brainDebugID;

        /// <summary>
        /// The <see cref="UnityDebugger"/> component, used to display debug information on the UI.
        /// </summary>
        static UnityDebugger debugger;

        /// <summary>
        /// Adds this <see cref="Brain"/> and its provided <see cref="Belief"/>s to the <see cref="UnityDebugger"/> to display.
        /// </summary>
        /// <param name="beliefs">The <see cref="Belief"/>s of the <see cref="Brain"/> connected to this <see cref="UnityBody"/>.</param>
        internal void Debug(Dictionary<string, IBelief> beliefs)
        {
            if (debugger == null)
                return;

            brainDebugName = (++brainDebugID).ToString();

            foreach (var belief in beliefs)
                debugger.AddBelief(brainDebugName, belief.Key, belief.Value, Body);
        }
#endif
    }
}
