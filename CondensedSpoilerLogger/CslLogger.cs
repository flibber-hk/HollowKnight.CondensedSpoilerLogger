using RandomizerMod.Logging;

namespace CondensedSpoilerLogger
{
    public abstract class CslLogger : RandoLogger
    {
        protected void WriteLog(string text, string fileName)
        {
            LogManager.Write(text, fileName);
        }
    }
}
