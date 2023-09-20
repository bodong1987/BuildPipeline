namespace BuildPipeline.Core.Framework
{
    /// <summary>
    /// Class GlobalEventRouter.
    /// </summary>    
    public static class GlobalEventRouter
    {
        private static readonly Dictionary<object, Delegate> EventDict = new Dictionary<object, Delegate>();

        #region Register Events
        /// <summary>
        /// Registers the event.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>        
        public static void RegisterEvent(object eventType, Action eventHandler)
        {
            if (BeforeRegisterEvent(eventType, eventHandler)) //-V3022
            {
                EventDict[eventType] = (Action)EventDict[eventType] + eventHandler;
            }
        }

        /// <summary>
        /// Registers the event.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>
        public static void RegisterEvent<T0>(object eventType, Action<T0> eventHandler)
        {
            if (BeforeRegisterEvent(eventType, eventHandler)) //-V3022
            {
                EventDict[eventType] = (Action<T0>)EventDict[eventType] + eventHandler;
            }
        }

        /// <summary>
        /// Registers the event.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>
        public static void RegisterEvent<T0, T1>(object eventType, Action<T0, T1> eventHandler)
        {
            if (BeforeRegisterEvent(eventType, eventHandler)) //-V3022
            {
                EventDict[eventType] = (Action<T0, T1>)EventDict[eventType] + eventHandler;
            }
        }

        /// <summary>
        /// Registers the event.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <typeparam name="T2">The type of the t2.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>
        public static void RegisterEvent<T0, T1, T2>(object eventType, Action<T0, T1, T2> eventHandler)
        {
            if (BeforeRegisterEvent(eventType, eventHandler)) //-V3022
            {
                EventDict[eventType] = (Action<T0, T1, T2>)EventDict[eventType] + eventHandler;
            }
        }

        /// <summary>
        /// Registers the event.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <typeparam name="T2">The type of the t2.</typeparam>
        /// <typeparam name="T3">The type of the t3.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>
        public static void RegisterEvent<T0, T1, T2, T3>(object eventType, Action<T0, T1, T2, T3> eventHandler)
        {
            if (BeforeRegisterEvent(eventType, eventHandler)) //-V3022
            {
                EventDict[eventType] = (Action<T0, T1, T2, T3>)EventDict[eventType] + eventHandler;
            }
        }

        /// <summary>
        /// Registers the event.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <typeparam name="T2">The type of the t2.</typeparam>
        /// <typeparam name="T3">The type of the t3.</typeparam>
        /// <typeparam name="T4">The type of the t4.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>
        public static void RegisterEvent<T0, T1, T2, T3, T4>(object eventType, Action<T0, T1, T2, T3, T4> eventHandler)
        {
            if (BeforeRegisterEvent(eventType, eventHandler)) //-V3022
            {
                EventDict[eventType] = (Action<T0, T1, T2, T3, T4>)EventDict[eventType] + eventHandler;
            }
        }

        /// <summary>
        /// Befores the register event.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// eventType
        /// or
        /// eventHandler
        /// </exception>
        /// <exception cref="ArgumentException"></exception>
        private static bool BeforeRegisterEvent(object eventType, Delegate eventHandler)
        {
            if (eventType == null)
            {
                throw new ArgumentNullException("eventType");
            }

            if (eventHandler == null)
            {
                throw new ArgumentNullException("eventHandler");
            }

            Delegate value = null;
            if (!EventDict.TryGetValue(eventType, out value))
            {
                EventDict.Add(eventType, null);
            }
            else
            {
                if (value != null && value.GetType() != eventHandler.GetType())
                {
                    throw new ArgumentException(string.Format("Delegate type miss match! please check the code!!!"));
                }
            }

            return true;
        }
        #endregion

        #region UnRegister Events
        /// <summary>
        /// Uns the register event.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>
        public static void UnRegisterEvent(object eventType, Action eventHandler)
        {
            if (BeforeUnRegisterEvent(eventType, eventHandler))
            {
                EventDict[eventType] = (Action)EventDict[eventType] - eventHandler;
            }
        }

        /// <summary>
        /// Uns the register event.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>
        public static void UnRegisterEvent<T0>(object eventType, Action<T0> eventHandler)
        {
            if (BeforeUnRegisterEvent(eventType, eventHandler))
            {
                EventDict[eventType] = (Action<T0>)EventDict[eventType] - eventHandler;
            }
        }

        /// <summary>
        /// Uns the register event.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>
        public static void UnRegisterEvent<T0, T1>(object eventType, Action<T0, T1> eventHandler)
        {
            if (BeforeUnRegisterEvent(eventType, eventHandler))
            {
                EventDict[eventType] = (Action<T0, T1>)EventDict[eventType] - eventHandler;
            }
        }

        /// <summary>
        /// Uns the register event.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <typeparam name="T2">The type of the t2.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>
        public static void UnRegisterEvent<T0, T1, T2>(object eventType, Action<T0, T1, T2> eventHandler)
        {
            if (BeforeUnRegisterEvent(eventType, eventHandler))
            {
                EventDict[eventType] = (Action<T0, T1, T2>)EventDict[eventType] - eventHandler;
            }
        }

        /// <summary>
        /// Uns the register event.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <typeparam name="T2">The type of the t2.</typeparam>
        /// <typeparam name="T3">The type of the t3.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>
        public static void UnRegisterEvent<T0, T1, T2, T3>(object eventType, Action<T0, T1, T2, T3> eventHandler)
        {
            if (BeforeUnRegisterEvent(eventType, eventHandler))
            {
                EventDict[eventType] = (Action<T0, T1, T2, T3>)EventDict[eventType] - eventHandler;
            }
        }

        /// <summary>
        /// Uns the register event.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <typeparam name="T2">The type of the t2.</typeparam>
        /// <typeparam name="T3">The type of the t3.</typeparam>
        /// <typeparam name="T4">The type of the t4.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>
        public static void UnRegisterEvent<T0, T1, T2, T3, T4>(object eventType, Action<T0, T1, T2, T3, T4> eventHandler)
        {
            if (BeforeUnRegisterEvent(eventType, eventHandler))
            {
                EventDict[eventType] = (Action<T0, T1, T2, T3, T4>)EventDict[eventType] - eventHandler;
            }
        }
        /// <summary>
        /// Befores the un register event.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventHandler">The event handler.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// eventType
        /// or
        /// eventHandler
        /// </exception>
        /// <exception cref="ArgumentException"></exception>
        private static bool BeforeUnRegisterEvent(object eventType, Delegate eventHandler)
        {
            if (eventType == null)
            {
                throw new ArgumentNullException("eventType");
            }

            if (eventHandler == null)
            {
                throw new ArgumentNullException("eventHandler");
            }

            Delegate value = null;
            if (!EventDict.TryGetValue(eventType, out value))
            {
                return false;
            }
            else
            {
                if (value != null && value.GetType() != eventHandler.GetType())
                {
                    throw new ArgumentException(string.Format("Delegate type miss match! please check the code!!!"));
                }
            }

            return true;
        }
        #endregion

        #region Broadcast Events
        /// <summary>
        /// Broadcasts the specified event type.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        public static void Broadcast(object eventType)
        {
            Delegate value;
            if (EventDict.TryGetValue(eventType, out value))
            {
                var callback = value as Action;

                callback?.Invoke();
            }
        }

        /// <summary>
        /// Broadcasts the specified event type.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="arg0">The arg0.</param>
        public static void Broadcast<T0>(object eventType, T0 arg0)
        {
            Delegate value;
            if (EventDict.TryGetValue(eventType, out value))
            {
                var callback = value as Action<T0>;

                callback?.Invoke(arg0);
            }
        }

        /// <summary>
        /// Broadcasts the specified event type.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        public static void Broadcast<T0, T1>(object eventType, T0 arg0, T1 arg1)
        {
            Delegate value;
            if (EventDict.TryGetValue(eventType, out value))
            {
                var callback = value as Action<T0, T1>;

                callback?.Invoke(arg0, arg1);
            }
        }

        /// <summary>
        /// Broadcasts the specified event type.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <typeparam name="T2">The type of the t2.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        public static void Broadcast<T0, T1, T2>(object eventType, T0 arg0, T1 arg1, T2 arg2)
        {
            Delegate value;
            if (EventDict.TryGetValue(eventType, out value))
            {
                var callback = value as Action<T0, T1, T2>;

                callback?.Invoke(arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Broadcasts the specified event type.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <typeparam name="T2">The type of the t2.</typeparam>
        /// <typeparam name="T3">The type of the t3.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="arg3">The arg3.</param>
        public static void Broadcast<T0, T1, T2, T3>(object eventType, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            Delegate value;
            if (EventDict.TryGetValue(eventType, out value))
            {
                var callback = value as Action<T0, T1, T2, T3>;

                callback?.Invoke(arg0, arg1, arg2, arg3);
            }
        }

        /// <summary>
        /// Broadcasts the specified event type.
        /// </summary>
        /// <typeparam name="T0">The type of the t0.</typeparam>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <typeparam name="T2">The type of the t2.</typeparam>
        /// <typeparam name="T3">The type of the t3.</typeparam>
        /// <typeparam name="T4">The type of the t4.</typeparam>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="arg0">The arg0.</param>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="arg3">The arg3.</param>
        /// <param name="arg4">The arg4.</param>
        public static void Broadcast<T0, T1, T2, T3, T4>(object eventType, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Delegate value;
            if (EventDict.TryGetValue(eventType, out value))
            {
                var callback = value as Action<T0, T1, T2, T3, T4>;

                callback?.Invoke(arg0, arg1, arg2, arg3, arg4);
            }
        }
        #endregion

        #region Shutdown
        /// <summary>
        /// Clears this instance.
        /// </summary>
        public static void Clear()
        {
            EventDict.Clear();
        }
        #endregion
    }
}
