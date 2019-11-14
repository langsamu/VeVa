namespace langsamu.VeVa
{
    using AForge.Imaging;
    using AForge.Imaging.Filters;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing.Imaging;
    using System.IO;

    public static class p1
    {
        private static FiltersSequence filters;

        private static Func<string, string> filterFile;

        static p1()
        {
            p1.filters = new FiltersSequence(new IFilter[] 
            {
                Grayscale.CommonAlgorithms.Y,
                new Threshold(200),
                new Closing()
            });

            p1.filterFile = new Func<string, string>(p1.FilterFile).Memoize();
        }

        private static string VectorizeFile(string path)
        {
            var vectorPath = Path.GetTempFileName();

            var p = new string[] { "--svg", "--flat", "--tight", "--output", vectorPath, path };
            var delimited = string.Join(" ", p);

            var startInfo = new ProcessStartInfo("lib/potrace/potrace.exe", delimited)
            {
                UseShellExecute = false
            };

            var process = new Process()
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();

            return vectorPath;
        }

        public static IEnumerable<string> Do(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                var filtered = p1.filterFile(path);
                var vectorized = p1.VectorizeFile(filtered);
                
                yield return vectorized;
            }
        }

        private static string FilterFile(string path)
        {
            var filteredPath = Path.GetTempFileName();

            using (var original = Image.FromFile(path))
            {
                using (var filtered = p1.filters.Apply(original))
                {
                    filtered.Save(filteredPath, ImageFormat.Bmp);
                }
            }

            return filteredPath;
        }
    }
}

