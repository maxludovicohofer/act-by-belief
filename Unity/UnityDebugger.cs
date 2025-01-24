using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using TMPro;
using System;

namespace MaxHofer.ActByBelief
{
    /// <summary>
    /// UI debugger for the Unity engine.
    /// </summary>
    internal class UnityDebugger : MonoBehaviour
    {
        /// <summary>
        /// Activates or deactivates the UI for the <see cref="UnityDebugger"/>.
        /// </summary>
        internal new bool enabled
        {
            get => base.enabled;
            set
            {
                base.enabled = value;
                foreach (var identifier in identifiers)
                    identifier.Value.SetActive(value);
            }
        }

        /// <summary>
        /// Offset from the screen border.
        /// </summary>
        const uint SCREEN_OFFSET = 20;

        /// <summary>
        /// Maximum number of lines of text to display on screen.
        /// </summary>
        const byte MAX_LINES = 50;

        /// <summary>
        /// Performance cost of processing a single line of text.
        /// </summary>
        const float LINE_PROCESS_TIME = .001f;

        /// <summary>
        /// Refresh period for <see cref="UpdateText"/> debounce, in order to not affect game performance.
        /// </summary>
        const float TEXT_UPDATE_PERIOD = MAX_LINES * LINE_PROCESS_TIME;

        /// <summary>
        /// Coroutine saved in order to debounce <see cref="UpdateText"/>.
        /// </summary>
        Coroutine updateText;

        /// <summary>
        /// Current text displayed in the <see cref="UnityDebugger"/> UI.
        /// </summary>
        string text;

        /// <summary>
        /// Debug info for this <see cref="UnityDebugger"/>, saved as <see cref="Brain"/> names mapped to <see cref="Belief"/>s.
        /// </summary>
        static readonly Dictionary<string, Dictionary<string, object>> brains = new();

        /// <summary>
        /// Identifier <see cref="GameObject"/>s, mapped by their relative <see cref="Brain"/> name.
        /// </summary>
        static readonly Dictionary<string, GameObject> identifiers = new();

        /// <summary>
        /// Updates UI.
        /// </summary>
        void OnGUI()
        {
            GUILayout.BeginArea(new(SCREEN_OFFSET, SCREEN_OFFSET, Screen.width, Screen.height));
            GUILayout.Label(text);
            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            if (updateText != null)
                StopCoroutine(updateText);
        }

        /// <summary>
        /// Adds a <see cref="Belief"/> to <see cref="brains"/> and subscribe to its <see cref="IBelief.OnDebugChange"/>. Can also add <see cref="Brain"/> and its identifier, if not present.
        /// </summary>
        /// <param name="brainName">The name of the <see cref="Brain"/> that is referenced by the <see cref="Belief"/>.</param>
        /// <param name="beliefName">The name of the <see cref="Belief"/>.</param>
        /// <param name="belief">The <see cref="Belief"/>.</param>
        /// <param name="body">The body where to attach an identifier text.</param>
        internal void AddBelief(
            string brainName,
            string beliefName,
            IBelief belief,
            MonoBehaviour body
        )
        {
            if (!brains.ContainsKey(brainName))
            {
                // Add brain
                brains.Add(brainName, new());

                // Attach identifier text on top
                var identifier = new GameObject($"{brainName}ActByBeliefID", typeof(TextMeshPro));
                identifier.transform.SetParent(body.transform);
                identifier.transform.SetLocalPositionAndRotation(
                    new(0, 1, 0),
                    Quaternion.Euler(0, 180, 0)
                );

                var textMesh = identifier.GetComponent<TextMeshPro>();
                textMesh.text = brainName;
                textMesh.fontSize = 6;
                textMesh.alignment = TextAlignmentOptions.Center;

                identifiers.Add(brainName, identifier);
            }

            brains[brainName].Add(
                beliefName,
                belief.GetType().GetProperty("Value").GetValue(belief)
            );
            TriggerUpdate();

#if DEBUG
            belief.OnDebugChange += value =>
            {
                brains[brainName][beliefName] = value;
                TriggerUpdate();
            };
#endif
        }

        /// <summary>
        /// Removes a <see cref="Brain"/> from debug.
        /// </summary>
        /// <param name="brainName">The name of the <see cref="Brain"/>.</param>
        internal void RemoveBrain(string brainName)
        {
            if (!brains.ContainsKey(brainName))
                return;

            brains.Remove(brainName);
            TriggerUpdate();

            identifiers.Remove(brainName);
        }

        /// <summary>
        /// Triggers a debounced <see cref="UpdateText"/>.
        /// </summary>
        void TriggerUpdate() => updateText ??= StartCoroutine(UpdateText());

        /// <summary>
        /// Waits for <see cref="TEXT_UPDATE_PERIOD"/> seconds, then updates <see cref="text"/> by converting <see cref="brains"/> to text.
        /// </summary>
        /// <returns>The output required by <see cref="StartCoroutine"/>.</returns>
        IEnumerator UpdateText()
        {
            yield return new WaitForSeconds(TEXT_UPDATE_PERIOD);

            StringBuilder textBuilder = new();

            byte lines = 0;

            foreach (var brain in brains.Keys)
            {
                textBuilder.AppendLine($"{brain}:");
                ++lines;

                foreach (var belief in brains[brain])
                {
                    var value = ObjectToString(belief.Value);

                    if (value == "")
                        continue;

                    textBuilder.AppendLine($"{belief.Key}: {value}");
                    ++lines;

                    if (lines > MAX_LINES)
                        break;
                }

                if (lines > MAX_LINES)
                    break;

                textBuilder.AppendLine();
                ++lines;
            }

            text = textBuilder.ToString();

            updateText = null;
        }

        /// <summary>
        /// Converts a generic object value to a sensible string.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The object as string.</returns>
        static string ObjectToString(object value)
        {
            if (value == null)
                return "";

            if (value is Array arrayValue)
            {
                if (arrayValue.Length == 0)
                    return "";

                var stringArray = new object[arrayValue.Length];

                for (int i = 0; i < stringArray.Length; i++)
                    stringArray[i] = arrayValue.GetValue(i).ToString();

                return string.Join(", ", stringArray);
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
