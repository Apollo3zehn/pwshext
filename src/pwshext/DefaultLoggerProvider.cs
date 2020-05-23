using Microsoft.Extensions.Logging;

namespace pwshext
{
    public class DefaultLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new DefaultLogger();
        }

        public void Dispose()
        {
            //
        }
    }
}
