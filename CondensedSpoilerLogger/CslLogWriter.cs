using CondensedSpoilerLogger.Util;
using RandomizerMod.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CondensedSpoilerLogger
{
    internal class CslLogWriter : RandoLogger
    {
        internal static CslLogWriter Instance { get; } = new();

        internal void Hook()
        {
            // To do - set up cancellation for if they quit to menu during computation
        }

        public override void Log(LogArguments args)
        {
            Task.Factory.StartNew(CreateLoggingAction(args));
        }

        private Action CreateLoggingAction(LogArguments args)
        {
            void DoLogging()
            {
                foreach ((string text, string filename) in 
                    CondensedSpoilerLogger.CreateLoggers().SelectMany(log => log.GetLogTexts(args)))
                {
                    LogManager.Write(text, filename);
                }
            }
            
            return DoLogging;
        }

    }
}
