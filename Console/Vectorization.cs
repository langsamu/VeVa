namespace langsamu.VeVa
{
    using ImageMagick;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    public static class Vectorization
    {
        public static IEnumerable<string> Convert(IEnumerable<string> paths)
        {
            return paths.SelectMany(Vectorization.Convert);
        }

        public static IEnumerable<string> Convert(string path)
        {
            var file = Path.GetFileNameWithoutExtension(path);
            var directory = Path.GetDirectoryName(path);

            var counter = 1;

            var cropped = Vectorization.Crop(path);

            foreach (var selected in cropped)
            {
                var traced = Vectorization.Trace(selected);

                // TODO: move format to config
                var formatted = string.Format("{0}.{1:D4}.svg", file, counter);

                var combined = Path.Combine(directory, formatted);

                File.Move(traced, combined);

                yield return combined;

                counter++;
            }
        }

        private static IEnumerable<string> Crop(string path)
        {
            using var original = new MagickImage(path);
            using var borders = original.Clone();
            borders.InverseOpaque(MagickColor.FromRgb(255, 0, 255), MagickColors.Black);

            var components = borders.ConnectedComponents(new ConnectedComponentsSettings { MeanColor = true })
                .Where(component => !(component.X == 0 && component.Y == 0 && component.Width == original.Width && component.Height == original.Height))
                .Where(component => component.Color == MagickColors.Black);

            if (components.Any())
            {
                foreach (var component in components)
                {
                    using var mask = borders.Clone();
                    mask.FloodFill(MagickColors.White, component.Centroid, MagickColors.Black);
                    mask.Crop(component.ToGeometry());
                    mask.InverseOpaque(MagickColors.White, MagickColors.Black);

                    using var content = original.Clone(component.ToGeometry());
                    content.Composite(mask, CompositeOperator.Multiply);
                    mask.Negate();
                    content.Composite(mask, CompositeOperator.Plus);

                    var result = Path.GetTempFileName();
                    content.Write(result, MagickFormat.Bmp);

                    yield return result;
                }
            }
            else
            {
                // No blobs means no selection, no cutting required, process whole image
                yield return path;
            }
        }

        private static string Trace(string path)
        {
            var vectorPath = Path.GetTempFileName();

            var arguments = new string[] { "--svg", "--flat", "--tight", "--output", vectorPath, path };
            var delimited = string.Join(" ", arguments);

            var startInfo = new ProcessStartInfo("lib/potrace-1.16.win64/potrace.exe", delimited)
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
    }
}