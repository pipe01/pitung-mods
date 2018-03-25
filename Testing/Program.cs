using ShareMod;
using System.IO;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var instream = File.OpenRead("test.zip"))
            {
                Zipper.ExtractZipFile(instream, "./extract");
            }
        }
    }
}
