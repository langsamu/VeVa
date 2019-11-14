namespace langsamu.VeVa
{
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using sys = System;

    public class Console
    {
        public static void Main(string[] args)
        {
            if (args.Any())
            {
                var unique = args.Distinct();

                var converted = langsamu.VeVa.Vectorization.Convert(unique);

                foreach (var item in converted)
                {
                    sys.Console.WriteLine(item);
                }
            }
            else
            {
                sys.Console.WriteLine("Usage:");
                sys.Console.WriteLine("{0} file-1.ext file-2.ext [...] file-n.ext", Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().ManifestModule.Name));
            }
        }
    }
}
