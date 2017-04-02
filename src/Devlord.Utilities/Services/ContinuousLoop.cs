﻿using System;
using System.Threading;

namespace Devlord.Utilities.Services
{
    /// <summary>
    /// This is a loop that always runs. If you use it for long-running operations, it basically makes sure that your
    /// task starts again as soon as it's finished.
    /// </summary>
    /// <remarks>Using fire-and-forget code inside your event handler might cause an overflow, since a new invocation will occur as soon as control
    /// returns from the event.</remarks>
    public class ContinuousLoop : ServiceTimer
    {
        #region Fields

        private int _runningTimers;

        #endregion

        #region Constructors and Destructors
        

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Adding multiple events to the timer will allow the events to run consecutively, though in no particular order.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public override ServiceTimer AddEvent(ServiceTimerEventHandler @event)
        {
            Events += (s, e) =>
            {
                ++_runningTimers;
                try
                {
                    @event.Invoke(this, e);
                }
                catch (Exception error)
                {
                    Logger.Log(error);
                }

                if (--_runningTimers == 0)
                {
                    // Restart the timer immediately.
                    LocalTimer.Change(0, Timeout.Infinite);
                }
            };

            return this;
        }

        public override void Run()
        {
            // New timer with immediate start, manual repeat.
            LocalTimer = new Timer(AllCallbacks, new ServiceTimerState(), 0, Timeout.Infinite);
        }

        #endregion
    }
}