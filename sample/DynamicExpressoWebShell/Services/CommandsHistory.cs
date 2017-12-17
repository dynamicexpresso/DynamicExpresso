using System;
using System.Collections.Generic;
using System.Threading;

namespace DynamicExpressoWebShell.Services
{

    public class CommandEvent
    {
        public CommandEvent(string exp)
        {
            Expression = exp;
            Time = DateTime.UtcNow;
            //UserHostAddress = HttpContext.Current.Request.UserHostAddress;
            //UserAgent = HttpContext.Current.Request.Browser.Browser;
        }

        public string Expression { get; private set; }
        public DateTime Time { get; private set; }
        //public string UserHostAddress { get; private set; }
        //public string UserAgent { get; private set; }
    }

    public class CommandsHistory
    {
        readonly List<CommandEvent> _lastCommands = new List<CommandEvent>();
        long _count = 0;
        readonly object _lock = new object();

        public long Count
        {
            get
            {
                return Interlocked.Read(ref _count);
            }
        }

        public void HandleCommandExecuted(CommandEvent cmd)
        {
            Interlocked.Increment(ref _count);

            lock (_lock)
            {
                if (_lastCommands.Count > 50)
                    _lastCommands.Clear();

                _lastCommands.Add(cmd);
            }
        }

        public CommandEvent[] GetLastCommands()
        {
            CommandEvent[] currentList;
            lock (_lock)
            {
                currentList = _lastCommands.ToArray();
            }

            return currentList;
        }

        public void Clear()
        {
            lock (_lock)
            {
                _lastCommands.Clear();
            }
        }
    }
}