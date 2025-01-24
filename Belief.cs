using System;
using System.Collections.Generic;

namespace MaxHofer.ActByBelief
{
    /// <summary>
    /// The change event used internally to communicate among <see cref="Belief"/>s.
    /// </summary>
    public delegate void SignalChangeEvent();

    /// <summary>
    /// Basic <see cref="Signal"/> interface.
    /// </summary>
    public interface ISignal
    {
        /// <summary>
        /// The change event used internally to communicate among <see cref="Belief"/>s.
        /// </summary>
        event SignalChangeEvent OnChange;

        /// <summary>
        /// Used internally to retrieve the value of the <see cref="Signal"/>'s <see cref="belief"/>.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Used internally to remove an influence <see cref="Signal"/> and updates the <see cref="Signal"/>'s <see cref="Belief"/>.
        /// </summary>
        /// <param name="signal">The influence <see cref="Signal"/> to remove.</param>
        void Kill(ISignal signal);
    }

#if DEBUG
    /// <summary>
    /// The change event used internally to communicate with debuggers.
    /// </summary>
    /// <param name="value">The changed value.</param>
    public delegate void DebugChangeEvent(object value);
#endif

    /// <summary>
    /// Basic <see cref="Belief"/> interface.
    /// </summary>
    internal interface IBelief
    {
#if DEBUG
        /// <summary>
        /// The change event used internally to communicate with debuggers.
        /// </summary>
        event DebugChangeEvent OnDebugChange;
#endif

        /// <summary>
        /// Used internally to kill the <see cref="Belief"/> and its <see cref="Signal"/>, thus disconnecting all its influences.
        /// </summary>
        void Die();
    }

    /// <summary>
    /// A <see cref="Belief"/> is a value, <see cref="Interpret"/>ed based on <see cref="Sense"/> or other <see cref="Belief"/>s acting as influences.
    /// </summary>
    /// <typeparam name="T">The <see cref="Belief"/>'s <see cref="Value"/> type.</typeparam>
    /// <typeparam name="S">The <see cref="Belief"/>'s <see cref="Sense"/> parameter type.</typeparam>
    public class Belief<T, S> : IBelief
    {
        /// <summary>
        /// The <see cref="Interpret"/> function, transforming a <see cref="Signal"/> in a <see cref="Value"/>, called when an influence changed value or <see cref="Sense"/> is called.
        /// </summary>
        /// <param name="signal">The incoming <see cref="Signal"/>.</param>
        /// <returns>The interpreted <see cref="Value"/>.</returns>
        public delegate T InterpretFunction(Signal signal);

        /// <summary>
        /// The <see cref="OnChange"/> event, triggered when <see cref="Value"/> is changed.
        /// </summary>
        /// <param name="value">The changed <see cref="Value"/>.</param>
        public delegate void ChangeEvent(T value);

        /// <summary>
        /// The <see cref="TriggerChange"/> function, used to override when <see cref="OnChange"/> will be called.
        /// E.g. could be programmed to add a reaction time to the <see cref="Belief"/>, or custom conditions to trigger the event.
        /// </summary>
        /// <param name="triggerChange">Function that triggers <see cref="OnChange"/>.</param>
        /// <param name="value">The changed <see cref="Value"/>.</param>
        internal delegate void TriggerChangeFunction(Action triggerChange, T value);

        /// <summary>
        /// The <see cref="Reinterpret"/> function, used to reinterpret the interpreted <see cref="Value"/>.
        /// </summary>
        /// <param name="value">The changed <see cref="Value"/>.</param>
        /// <returns>The reinterpreted <see cref="Value"/>.</returns>
        internal delegate T ReinterpretFunction(T value);

        /// <summary>
        /// The <see cref="OnChange"/> event, triggered when <see cref="Value"/> is changed.
        /// </summary>
        public event ChangeEvent OnChange;

#if DEBUG
        /// <summary>
        /// The change event used internally to communicate with debuggers.
        /// </summary>
        public event DebugChangeEvent OnDebugChange;
#endif

        /// <summary>
        /// The value representing the state of the <see cref="Belief"/>.
        /// </summary>
        public T Value
        {
            get => value;
            private set
            {
                //? Check if values are equal
                if (EqualityComparer<T>.Default.Equals(this.value, value))
                    return;

                this.value = value;

                if (TriggerChange != null)
                    TriggerChange(InvokeChange, this.value);
                else
                    InvokeChange();
            }
        }

        /// <summary>
        /// The <see cref="TriggerChange"/> function, used to override when <see cref="OnChange"/> will be called.
        /// E.g. could be programmed to add a reaction time to the <see cref="Belief"/>, or custom conditions to trigger the event.
        /// </summary>
        internal TriggerChangeFunction TriggerChange { private get; set; }

        /// <summary>
        /// The <see cref="Reinterpret"/> function, used to reinterpret the interpreted <see cref="Value"/>.
        /// </summary>
        internal ReinterpretFunction Reinterpret
        {
            private get => reinterpret;
            set
            {
                reinterpret = value;
                React();
            }
        }

        /// <summary>
        /// The <see cref="Interpret"/> function, transforming a <see cref="Signal"/> in a <see cref="Value"/>, called when an influence changed value or <see cref="Sense"/> is called.
        /// </summary>
        readonly InterpretFunction interpret;

        /// <summary>
        /// The <see cref="Signal"/> of this <see cref="Belief"/>, used to communicate with other beliefs.
        /// </summary>
        readonly Signal signal;

        /// <summary>
        /// The value representing the state of the <see cref="Belief"/>.
        /// </summary>
        T value;

        /// <summary>
        /// The <see cref="Reinterpret"/> function, used to reinterpret the interpreted <see cref="Value"/>.
        /// </summary>
        ReinterpretFunction reinterpret;

        /// <summary>
        /// Creates a new <see cref="Belief"/> with the provided <see cref="Interpret"/> function.
        /// </summary>
        /// <param name="interpret">Transforms a <see cref="Signal"/> in a <see cref="Value"/>.</param>
        public Belief(InterpretFunction interpret = null)
        {
            signal = new(this);
            OnChange += _ => signal.InvokeChange();
#if DEBUG
            OnChange += value => OnDebugChange?.Invoke(value);
#endif

            this.interpret =
                interpret ?? (signal => signal.Sensed is T interpreted ? interpreted : value);

            // Initialize
            Sense(default);
        }

        /// <summary>
        /// Senses data to be interpreted by the <see cref="Interpret"/> function.
        /// </summary>
        /// <param name="data">The data to interpret.</param>
        public void Sense(S data)
        {
            signal.Sensed = data;

            React();
        }

        /// <summary>
        /// Adds a belief as an influence to this <see cref="Belief"/>.
        /// </summary>
        /// <typeparam name="BeliefT">The T of the influence.</typeparam>
        /// <typeparam name="BeliefS">The S of the influence.</typeparam>
        /// <param name="belief">The belief acting as influence.</param>
        public void AddInfluence<BeliefT, BeliefS>(Belief<BeliefT, BeliefS> belief)
        {
            signal.AddInfluence(belief.signal);

            React();
        }

        /// <summary>
        /// Removes a belief that is an influence to this <see cref="Belief"/>.
        /// </summary>
        /// <typeparam name="BeliefT">The T of the influence.</typeparam>
        /// <typeparam name="BeliefS">The S of the influence.</typeparam>
        /// <param name="belief">The belief acting as influence.</param>
        public void RemoveInfluence<BeliefT, BeliefS>(Belief<BeliefT, BeliefS> belief)
        {
            signal.RemoveInfluence(belief.signal);

            React();
        }

        /// <summary>
        /// Used internally to kill the <see cref="Belief"/> and its <see cref="Signal"/>, thus disconnecting all its influences.
        /// </summary>
        public void Die() => signal.Die();

        /// <summary>
        /// Interprets changes and sets <see cref="Value"/>.
        /// </summary>
        void React()
        {
            var interpreted = interpret(signal);
            Value = Reinterpret != null ? Reinterpret(interpreted) : interpreted;
        }

        /// <summary>
        /// Triggers <see cref="OnChange"/>.
        /// </summary>
        void InvokeChange() => OnChange.Invoke(Value);

        /// <summary>
        /// Manages the <see cref="Belief"/>'s relationships to other beliefs.
        /// </summary>
        public sealed class Signal : ISignal
        {
            /// <summary>
            /// The change event used internally to communicate among <see cref="Belief"/>s.
            /// </summary>
            public event SignalChangeEvent OnChange;

            /// <summary>
            /// The data received by the <see cref="Sense"/> function.
            /// </summary>
            public S Sensed { get; set; }

            /// <summary>
            /// The <see cref="List"/> of <see cref="Signal"/>s acting as influences on this <see cref="Signal"/>.
            /// </summary>
            readonly List<ISignal> influences = new();

            /// <summary>
            /// This <see cref="Signal"/>'s <see cref="Belief"/>.
            /// </summary>
            readonly Belief<T, S> belief;

            /// <summary>
            /// Used internally to retrieve the value of the <see cref="Signal"/>'s <see cref="belief"/>.
            /// </summary>
            public object Value => belief.Value;

            /// <summary>
            /// Creates a <see cref="Signal"/> by providing its <see cref="Belief"/>.
            /// </summary>
            /// <param name="belief">The <see cref="Belief"/> to reference.</param>
            internal Signal(Belief<T, S> belief) => this.belief = belief;

            /// <summary>
            /// Retrieves the value of a belief and adds it as an influence to this belief.
            /// </summary>
            /// <typeparam name="BeliefT">The T of the belief.</typeparam>
            /// <typeparam name="BeliefS">The S of the belief.</typeparam>
            /// <param name="belief">The belief to get.</param>
            /// <returns>The belief's value.</returns>
            public BeliefT Get<BeliefT, BeliefS>(Belief<BeliefT, BeliefS> belief)
            {
                AddInfluence(belief.signal);

                return belief.Value;
            }

            /// <summary>
            /// Retrieves the values of all influences of this <see cref="Signal"/>.
            /// </summary>
            /// <typeparam name="BeliefT">The type of the value of the single influence.</typeparam>
            /// <returns>An array containing the values of the influences.</returns>
            public BeliefT[] GetInfluences<BeliefT>()
            {
                var values = new BeliefT[influences.Count];

                for (int i = 0; i < values.Length; i++)
                    values[i] = (BeliefT)influences[i].Value;

                return values;
            }

            /// <summary>
            /// Used internally to remove an influence <see cref="Signal"/> and updates the <see cref="Signal"/>'s <see cref="Belief"/>.
            /// </summary>
            /// <param name="signal">The influence <see cref="Signal"/> to remove.</param>
            public void Kill(ISignal signal)
            {
                RemoveInfluence(signal);
                React();
            }

            /// <summary>
            /// Adds a signal as an influence to this <see cref="Signal"/>.
            /// </summary>
            /// <param name="signal">The signal acting as influence.</param>
            internal void AddInfluence(ISignal signal)
            {
                if (signal.Equals(this) || influences.Contains(signal))
                    return;

                influences.Add(signal);
                signal.OnChange += React;
            }

            /// <summary>
            /// Removes a signal that is an influence to this <see cref="Signal"/>.
            /// </summary>
            /// <param name="signal">The signal acting as influence.</param>
            internal void RemoveInfluence(ISignal signal)
            {
                if (influences.Remove(signal))
                    signal.OnChange -= React;
            }

            /// <summary>
            /// Disconnects the <see cref="Signal"/>'s influences.
            /// </summary>
            internal void Die()
            {
                foreach (var signal in influences.ToArray())
                    RemoveInfluence(signal);

                if (OnChange == null)
                    return;

                // Remove influence on other signals
                foreach (var reactFunction in OnChange.GetInvocationList())
                    ((ISignal)reactFunction.Target).Kill(belief.signal);
            }

            /// <summary>
            /// Triggers <see cref="OnChange"/>.
            /// </summary>
            internal void InvokeChange() => OnChange?.Invoke();

            /// <summary>
            /// Triggers the <see cref="Belief"/>'s react.
            /// </summary>
            void React() => belief.React();
        }
    }

    /// <summary>
    /// A <see cref="Belief"/> is a value, <see cref="Interpret"/>ed based on <see cref="Sense"/> or other <see cref="Belief"/>s acting as influences.
    /// </summary>
    /// <typeparam name="T">The <see cref="Belief"/>'s <see cref="Value"/> type.</typeparam>
    public class Belief<T> : Belief<T, T>
    {
        /// <summary>
        /// Creates a new <see cref="Belief"/> with the provided <see cref="Interpret"/> function.
        /// </summary>
        /// <param name="interpret">Transforms a <see cref="Signal"/> in a <see cref="Value"/>.</param>
        public Belief(InterpretFunction interpret = null)
            : base(interpret ?? (signal => signal.Sensed)) { }
    }

    /// <summary>
    /// A <see cref="Belief"/> is a value, <see cref="Interpret"/>ed based on <see cref="Sense"/> or other <see cref="Belief"/>s acting as influences.
    /// </summary>
    public class Belief : Belief<object>
    {
        /// <summary>
        /// Creates a new <see cref="Belief"/> with the provided <see cref="Interpret"/> function.
        /// </summary>
        /// <param name="interpret">Transforms a <see cref="Signal"/> in a <see cref="Value"/>.</param>
        public Belief(InterpretFunction interpret = null)
            : base(interpret) { }
    }
}
