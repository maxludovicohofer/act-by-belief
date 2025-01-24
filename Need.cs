using System;
using UnityEngine;

namespace MaxHofer.ActByBelief
{
    using Importance = Single;
    using NeedBelief = Belief<Need>;

    /// <summary>
    /// The motive of a <see cref="Need"/>, based on Maslow's hierarchy of needs (<seealso href="https://en.wikipedia.org/wiki/Maslow's_hierarchy_of_needs"/>). Gets converted to weight through <c>2^-Motive</c>:
    /// <list type="bullet">
    /// <item><description><see cref="Survival"/> -> 1</description></item>
    /// <item><description><see cref="Love"/> -> 0.5</description></item>
    /// <item><description><see cref="Achievement"/> -> 0.25</description></item>
    /// </list>
    /// </summary>
    public enum Motive
    {
        Survival,
        Love,
        Achievement
    }

    /// <summary>
    /// Struct used to work with <see cref="Need"/> <see cref="Belief"/>s.
    /// </summary>
    public readonly struct Need
    {
        #region Instance

        /// <summary>
        /// Intensity of the <see cref="Need"/>, ranges from 0 to 1. Used to scale external values based on this <see cref="Need"/>.
        /// </summary>
        Importance Intensity => importance * intensifier;

        /// <summary>
        /// <see cref="Importance"/> of the <see cref="Need"/>, ranges from 0 to (1 / <see cref="intensifier"/>). used to compare between <see cref="Need"/>s.
        /// </summary>
        readonly Importance importance;

        /// <summary>
        /// 1 / maximum <see cref="Importance"/> that can be reached by <see cref="importance"/>. Used to remap <see cref="importance"/> to <see cref="Intensity"/>.
        /// </summary>
        readonly Importance intensifier;

        /// <summary>
        /// Creates a <see cref="Need"/> with the provided <see cref="Importance"/> and maximum <see cref="Importance"/>.
        /// </summary>
        /// <param name="importance">The <see cref="Importance"/> of the <see cref="Need"/>.</param>
        /// <param name="intensifier">1 / maximum <see cref="Importance"/> of the <see cref="Need"/>.</param>
        internal Need(Importance importance, Importance intensifier = 1)
        {
#if DEBUG
            CheckRange(importance, $"{nameof(importance)} ({nameof(intensifier)}: {intensifier})");
#endif
            this.importance = importance;
            this.intensifier = intensifier;
        }

        /// <summary>
        /// Transforms the <see cref="Need"/> to an appropriate string representation.
        /// </summary>
        /// <returns>A string containing <see cref="importance"/> and <see cref="Intensity"/>.</returns>
        public override string ToString() =>
            $"{importance}{(intensifier == 1 ? "" : $" ({nameof(Intensity)}: {Intensity})")}";

        /// <summary>
        /// Checks if the first <see cref="Need"/> is more important than the second.
        /// </summary>
        /// <param name="first">The first <see cref="Need"/>.</param>
        /// <param name="second">The second <see cref="Need"/>.</param>
        /// <returns>True if the first <see cref="Need"/> has more <see cref="Importance"/> than the second.</returns>
        public static bool operator >(Need first, Need second) =>
            first.importance > second.importance;

        /// <summary>
        /// Checks if the first <see cref="Need"/> is less important than the second.
        /// </summary>
        /// <param name="first">The first <see cref="Need"/>.</param>
        /// <param name="second">The second <see cref="Need"/>.</param>
        /// <returns>True if the first <see cref="Need"/> has less <see cref="Importance"/> than the second.</returns>
        public static bool operator <(Need first, Need second) =>
            first.importance < second.importance;

        /// <summary>
        /// Evaluates the difference in <see cref="Importance"/> between two <see cref="Need"/>s.
        /// </summary>
        /// <param name="first">The first <see cref="Need"/>.</param>
        /// <param name="second">The second <see cref="Need"/>.</param>
        /// <returns>A <see cref="Need"/> with <see cref="Importance"/> equal to first - second.</returns>
        public static Need operator -(Need first, Need second) =>
            new(Math.Max(first.importance - second.importance, 0), first.intensifier);

        /// <summary>
        /// Evaluates the sum of <see cref="Intensity"/> between two <see cref="Need"/>s.
        /// </summary>
        /// <param name="first">The first <see cref="Need"/>.</param>
        /// <param name="second">The second <see cref="Need"/>.</param>
        /// <returns>A number equal to first intensity + second intensity.</returns>
        public static float operator +(Need first, Need second) =>
            first.Intensity + second.Intensity;

        /// <summary>
        /// Evaluates the maximum between two <see cref="Need"/>s.
        /// </summary>
        /// <param name="first">The first <see cref="Need"/>.</param>
        /// <param name="second">The second <see cref="Need"/>.</param>
        /// <returns>The greater of the two <see cref="Need"/>s.</returns>
        public static Need Max(Need first, Need second) => first < second ? second : first;

        /// <summary>
        /// Evaluates the minimum between two <see cref="Need"/>s.
        /// </summary>
        /// <param name="first">The first <see cref="Need"/>.</param>
        /// <param name="second">The second <see cref="Need"/>.</param>
        /// <returns>The lesser of the two <see cref="Need"/>s.</returns>
        public static Need Min(Need first, Need second) => first > second ? second : first;

        /// <summary>
        /// Scales a number by a <see cref="Need"/>'s <see cref="Intensity"/>.
        /// </summary>
        /// <param name="current">The <see cref="Need"/>.</param>
        /// <param name="other">The number to scale.</param>
        /// <returns>The number scaled by the <see cref="Need"/>.</returns>
        public static float operator *(Need current, float other) => current.Intensity * other;

        /// <summary>
        /// Scales a number by a <see cref="Need"/>'s <see cref="Intensity"/>.
        /// </summary>
        /// <param name="other">The number to scale.</param>
        /// <param name="current">The <see cref="Need"/>.</param>
        /// <returns>The number scaled by the <see cref="Need"/>.</returns>
        public static float operator *(float other, Need current) => current * other;

        /// <summary>
        /// Scales a number by a <see cref="Need"/>'s <see cref="Intensity"/>.
        /// </summary>
        /// <param name="current">The <see cref="Need"/>.</param>
        /// <param name="other">The number to scale.</param>
        /// <returns>The number scaled by the <see cref="Need"/>.</returns>
        public static float operator *(Need current, int other) => current.Intensity * other;

        /// <summary>
        /// Scales a number by a <see cref="Need"/>'s <see cref="Intensity"/>.
        /// </summary>
        /// <param name="other">The number to scale.</param>
        /// <param name="current">The <see cref="Need"/>.</param>
        /// <returns>The number scaled by the <see cref="Need"/>.</returns>
        public static float operator *(int other, Need current) => current * other;

        /// <summary>
        /// Scales a <see cref="Vector3"/> by a <see cref="Need"/>'s <see cref="Intensity"/>.
        /// </summary>
        /// <param name="current">The <see cref="Need"/>.</param>
        /// <param name="other">The <see cref="Vector3"/> to scale.</param>
        /// <returns>The <see cref="Vector3"/> scaled by the <see cref="Need"/>.</returns>
        public static Vector3 operator *(Need current, Vector3 other) => current.Intensity * other;

        /// <summary>
        /// Scales a <see cref="Vector3"/> by a <see cref="Need"/>'s <see cref="Intensity"/>.
        /// </summary>
        /// <param name="other">The <see cref="Vector3"/> to scale.</param>
        /// <param name="current">The <see cref="Need"/>.</param>
        /// <returns>The <see cref="Vector3"/> scaled by the <see cref="Need"/>.</returns>
        public static Vector3 operator *(Vector3 other, Need current) => current * other;

        /// <summary>
        /// Evaluates the maximum between a <see cref="Need"/>'s importance and a number.
        /// </summary>
        /// <param name="current">The <see cref="Need"/>.</param>
        /// <param name="other">The number.</param>
        /// <returns>A <see cref="Need"/> with <see cref="Importance"/> equal to the greater of the numbers.</returns>
        static Need Max(Need current, float other) =>
            current.importance < other ? new(other, current.intensifier) : current;

        /// <summary>
        /// Evaluates the minimum between a <see cref="Need"/>'s importance and a number.
        /// </summary>
        /// <param name="current">The <see cref="Need"/>.</param>
        /// <param name="other">The number.</param>
        /// <returns>A <see cref="Need"/> with <see cref="Importance"/> equal to the lesser of the numbers.</returns>
        static Need Min(Need current, float other) =>
            current.importance > other ? new(other, current.intensifier) : current;

        /// <summary>
        /// Averages the <see cref="Importance"/> of the provided <see cref="Need"/>s.
        /// </summary>
        /// <param name="needs">The <see cref="Need"/>s to average.</param>
        /// <returns>A <see cref="Need"/> representing the average of the provided <see cref="Need"/>s</returns>
        static Need Average(Need[] needs)
        {
            if (needs.Length == 0)
                return new(0);

            var assessmentSum = 0f;
            var normalizerSum = 0f;

            foreach (var need in needs)
            {
                assessmentSum += need.importance;
                normalizerSum += need.intensifier;
            }

            var divisor = 1f / needs.Length;
            return new(divisor * assessmentSum, divisor * normalizerSum);
        }

        #endregion

        #region Interpret

        /// <summary>
        /// Need is not felt at all, meaning its <see cref="Need"/> is at its lowest.
        /// </summary>
        public static readonly Need ABSENT = new(0);

        /// <summary>
        /// Need is felt at its maximum, meaning its <see cref="Need"/> is at its highest.
        /// </summary>
        public static readonly Need URGENT = new(1);

        /// <summary>
        /// Need is felt at its average value, meaning its <see cref="Need"/> is at its midpoint.
        /// </summary>
        public static readonly Need NORMAL = new(.5f);

        /// <summary>
        /// Evaluates a <see cref="Need"/> based on a condition.
        /// </summary>
        /// <param name="condition">The condition to verify.</param>
        /// <returns><see cref="URGENT"/> if condition is true, else <see cref="ABSENT"/>.</returns>
        public static Need If(bool condition) => new(Convert.ToSingle(condition));

        /// <summary>
        /// Limits a <see cref="Need"/> to a minimum.
        /// </summary>
        /// <param name="need">The <see cref="Need"/> to limit.</param>
        /// <param name="min">The minimum value to limit the <see cref="Need"/> to.</param>
        /// <returns>Need if need is greater than min, else min.</returns>
        public static Need NeverTooLow(Need need, float min = .1f) => Max(need, min);

        /// <summary>
        /// Limits a <see cref="Need"/> to a maximum.
        /// </summary>
        /// <param name="need">The <see cref="Need"/> to limit.</param>
        /// <param name="min">The maximum value to limit the <see cref="Need"/> to.</param>
        /// <returns>Need if need is lower than max, else max.</returns>
        public static Need NeverTooHigh(Need need, float max = .9f) => Min(need, max);

        /// <summary>
        /// Evaluates a <see cref="Need"/> based on proximity to a target.
        /// </summary>
        /// <param name="target">The target position.</param>
        /// <param name="current">The position to evaluate in respect to the target.</param>
        /// <param name="maxDistance">The distance at which the evaluation will be <see cref="ABSENT"/>.</param>
        /// <returns>A <see cref="Need"/> ranging from <see cref="URGENT"/> at zero distance to <see cref="ABSENT"/> at maximum distance.</returns>
        public static Need Near(Vector3 target, Vector3 current, float maxDistance) =>
            Invert(Far(target, current, maxDistance));

        /// <summary>
        /// Evaluates a <see cref="Need"/> based on distance to a target.
        /// </summary>
        /// <param name="target">The target position.</param>
        /// <param name="current">The position to evaluate in respect to the target.</param>
        /// <param name="maxDistance">The distance at which the evaluation will be <see cref="URGENT"/>.</param>
        /// <returns>A <see cref="Need"/> ranging from <see cref="URGENT"/> at maximum distance to <see cref="ABSENT"/> at zero distance.</returns>
        public static Need Far(Vector3 target, Vector3 current, float maxDistance) =>
            new(Mathf.Min(Vector3.Distance(current, target), maxDistance) / maxDistance);

        /// <summary>
        /// Evaluates a <see cref="Need"/> based on the decrease of a value.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>A <see cref="Need"/> ranging from <see cref="URGENT"/> if value is 1 to <see cref="ABSENT"/> if value is max.</returns>
        public static Need WhenDecreasing(int value, int max) => Invert(WhenIncreasing(value, max));

        /// <summary>
        /// Evaluates a <see cref="Need"/> based on the decrease of a value.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="min">The minimum value.</param>
        /// <returns>A <see cref="Need"/> ranging from <see cref="URGENT"/> if value is min to <see cref="ABSENT"/> if value is max.</returns>
        public static Need WhenDecreasing(float value, float max, float min = .1f) =>
            Invert(WhenIncreasing(value, max, min));

        /// <summary>
        /// Evaluates a <see cref="Need"/> based on the increase of a value.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>A <see cref="Need"/> ranging from <see cref="URGENT"/> if value is max to <see cref="ABSENT"/> if value is 1.</returns>
        public static Need WhenIncreasing(int value, int max) =>
            new(Math.Max((value - 1) / Math.Max(max - 1, 1), 0));

        /// <summary>
        /// Evaluates a <see cref="Need"/> based on the increase of a value.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="min">The minimum value.</param>
        /// <returns>A <see cref="Need"/> ranging from <see cref="URGENT"/> if value is max to <see cref="ABSENT"/> if value is min.</returns>
        public static Need WhenIncreasing(float value, float max, float min = .1f) =>
            new(Math.Max((value - min) / Math.Max(max - min, min), 0));

        /// <summary>
        /// Evaluates a <see cref="Need"/> by averaging the provided <see cref="Need"/>s.
        /// </summary>
        /// <param name="needs">The <see cref="Need"/>s to average.</param>
        /// <returns>A <see cref="Need"/> ranging from <see cref="URGENT"/> if all assessments are <see cref="URGENT"/> to <see cref="ABSENT"/> if all assessments are <see cref="ABSENT"/>.</returns>
        public static Need All(params Need[] needs) => Average(needs);

        /// <summary>
        /// Evaluates a <see cref="Need"/> by inverting a <see cref="Need"/>.
        /// </summary>
        /// <param name="need">The <see cref="Need"/> to invert.</param>
        /// <returns>A <see cref="Need"/> ranging from <see cref="URGENT"/> if the need is <see cref="ABSENT"/> to <see cref="ABSENT"/> if the need is <see cref="URGENT"/>.</returns>
        static Need Invert(Need need) => URGENT - need;

        #endregion

        #region Motives

        /// <summary>
        /// General purpose random number generator.
        /// </summary>
        readonly static System.Random random = new();

        /// <summary>
        /// Generates a random offset based on a chaos factor that acts as a maximum and minimum.
        /// </summary>
        /// <param name="chaos">The chaos factor acting as maximum and minimum.</param>
        /// <returns>A random number ranging from -chaos to chaos.</returns>
        internal static Importance GetRandomOffset(Importance chaos)
        {
#if DEBUG
            CheckRange(chaos, $"{nameof(chaos)}");
#endif
            return chaos * (Importance)(2 * random.NextDouble() - 1);
        }

        /// <summary>
        /// Clamps the provided offsets to avoid <see cref="Motive"/> <see cref="Need"/> going over <see cref="URGENT"/> or under <see cref="ABSENT"/>.
        /// </summary>
        /// <param name="survival">The <see cref="Motive.Survival"/> offset.</param>
        /// <param name="love">The <see cref="Motive.Love"/> offset.</param>
        /// <param name="achievement">The <see cref="Motive.Achievement"/> offset.</param>
        /// <param name="minImportance">The minimum <see cref="Motive"/> <see cref="Need"/>, used to avoid a <see cref="Motive"/> <see cref="Need"/> potentially reaching <see cref="ABSENT"/> and effectively deactivating <see cref="Need"/>s following that <see cref="Motive"/>.</param>
        /// <returns>An array containing the clamped offsets.</returns>
        internal static Importance[] ClampOffsets(
            Importance survival,
            Importance love,
            Importance achievement,
            Importance minImportance = 0
        )
        {
#if DEBUG
            CheckRange(survival, $"{nameof(survival)}", -1);
            CheckRange(love, $"{nameof(love)}", -1);
            CheckRange(achievement, $"{nameof(achievement)}", -1);
            CheckRange(minImportance, $"{nameof(minImportance)}");
#endif
            return new[]
            {
                ClampOffset(survival, Motive.Survival, minImportance),
                ClampOffset(love, Motive.Love, minImportance),
                ClampOffset(achievement, Motive.Achievement, minImportance)
            };
        }

        /// <summary>
        /// Creates a reinterpret function that transforms a <see cref="Need"/> <see cref="Belief"/>'s <see cref="Intensity"/> to its <see cref="Importance"/>, by weighing its value by its <see cref="Motive"/> and offsets.
        /// </summary>
        /// <param name="motive">The <see cref="Motive"/> of the <see cref="Need"/>.</param>
        /// <param name="offsets">The offsets of the <see cref="Brain"/>.</param>
        /// <returns>The <see cref="Need"/> <see cref="Belief"/> reinterpret function.</returns>
        internal static NeedBelief.ReinterpretFunction InterpretIntensityAsImportance(
            Motive motive,
            Importance[] offsets
        ) =>
            need =>
            {
                var brainImportance = MotiveToImportance(motive) + offsets[(byte)motive];

                return new(brainImportance * need, 1 / brainImportance);
            };

        /// <summary>
        /// Gets reaction time based on minimum reaction time and the <see cref="Motive"/> of the <see cref="Need"/>.
        /// </summary>
        /// <param name="minSecondsToReact">The minimum reaction time, used for <see cref="Motive.Survival"/>.</param>
        /// <param name="motive">The <see cref="Motive"/> of the <see cref="Need"/>.</param>
        /// <returns>The seconds to react for the <see cref="Need"/>.</returns>
        internal static float GetReactionTime(float minSecondsToReact, Motive motive) =>
            minSecondsToReact / MotiveToImportance(motive);

        /// <summary>
        /// Clamps the provided offset to avoid <see cref="Motive"/> <see cref="Need"/> going over <see cref="URGENT"/> or under <see cref="ABSENT"/>.
        /// </summary>
        /// <param name="offset">The offset for the <see cref="Motive"/>.</param>
        /// <param name="motive">The <see cref="Motive"/> to reference.</param>
        /// <param name="minImportance">The minimum <see cref="Motive"/> <see cref="Need"/>, used to avoid the <see cref="Motive"/> <see cref="Need"/> potentially reaching <see cref="ABSENT"/> and effectively deactivating <see cref="Need"/>s following that <see cref="Motive"/>.</param>
        /// <returns>The clamped offset.</returns>
        static Importance ClampOffset(Importance offset, Motive motive, Importance minImportance)
        {
            var needImportance = MotiveToImportance(motive);
            return Math.Clamp(needImportance + offset, minImportance, 1) - needImportance;
        }

        /// <summary>
        /// Converts a <see cref="Motive"/> to its <see cref="Need"/> weight.
        /// </summary>
        /// <param name="motive">The <see cref="Motive"/> to convert.</param>
        /// <returns>A number representing the <see cref="Need"/> of the <see cref="Motive"/>.</returns>
        static Importance MotiveToImportance(Motive motive) =>
            (Importance)Math.Pow(2, -(byte)motive);

#if DEBUG
        /// <summary>
        /// Checks if an <see cref="Need"/> is in the correct range.
        /// </summary>
        /// <param name="importance">The <see cref="Need"/> to check.</param>
        /// <param name="importanceName">The name of the <see cref="Need"/> to check.</param>
        /// <param name="min">The minimum value for the <see cref="Need"/>.</param>
        /// <param name="max">The maximum value for the <see cref="Need"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception thrown if the <see cref="Need"/> is out of the correct range.</exception>
        internal static void CheckRange(
            Importance importance,
            string importanceName,
            Importance min = 0,
            Importance max = 1
        )
        {
            if (importance < min || importance > max)
            {
                throw new ArgumentOutOfRangeException(
                    $"{nameof(Brain)}.{importanceName}",
                    $"Value must be between {min} and {max}, is {importance} instead."
                );
            }
        }
#endif
        #endregion
    }
}
