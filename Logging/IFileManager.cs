using System.IO;

namespace Sqor.Utils.Logging
{
    public interface IFileManager
    {
        TextWriter Output { get; }
    }
}