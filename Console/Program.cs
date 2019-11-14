namespace langsamu.VeVa
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Any())
            {
                var unique = args.Distinct();

                var converted = Vectorization.Convert(unique);

                foreach (var item in converted)
                {
                    Console.WriteLine(item);
                }
            }
            else
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("{0} file-1.ext file-2.ext [...] file-n.ext", Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().ManifestModule.Name));
            }
        }
    }
}
