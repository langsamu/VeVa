namespace langsamu.VeVa
{
    using AForge;
    using AForge.Imaging;
    using AForge.Imaging.Filters;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    public static class Vectorization
    {
        private static FiltersSequence contrastFilters;

        private static FiltersSequence selectionFilters;

        static Vectorization()
        {
            Vectorization.contrastFilters = new FiltersSequence(new IFilter[] 
            {
                Grayscale.CommonAlgorithms.Y,
                new Threshold(200),
                new Closing()
            });

            Vectorization.selectionFilters = new FiltersSequence(new IFilter[] 
            {
                new ColorFiltering(new IntRange(255, 255), new IntRange(0, 0), new IntRange(255, 255)),
                Grayscale.CommonAlgorithms.Y,
                new Threshold(72),
                new Invert()
            });
        }

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
                var enhanced = Vectorization.Enhance(selected);
                var traced = Vectorization.Trace(enhanced);

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
            using (var original = AForge.Imaging.Image.FromFile(path))
            {
                using (var selection = Vectorization.HighlightSelection(original))
                {
                    var counter = Vectorization.CreateCounter(original, selection);

                    var blobs = counter.GetObjects(original, false);

                    if (blobs.Any())
                    {
                        foreach (var blob in blobs)
                        {
                            yield return Vectorization.ExtractSelection(blob, counter, selection);
                        }
                    }
                    else
                    {
                        // No blobs means no selection, no cutting required, process whole image
                        yield return path;
                    }
                }
            }
        }

        private static BlobCounter CreateCounter(Bitmap original, Bitmap selection)
        {
            var counter = new BlobCounter();

            counter.BlobsFilter = new ExcludeOriginalFilter(original);
            counter.FilterBlobs = true;
            counter.ObjectsOrder = ObjectsOrder.Area;

            counter.ProcessImage(selection);

            return counter;
        }

        private static string ExtractSelection(Blob blob, BlobCounter counter, Bitmap selectionsImage)
        {
            var selected = blob.Image;

            counter.ExtractBlobsImage(selectionsImage, blob, false);

            var mask = AForge.Imaging.Image.Clone(blob.Image.ToManagedImage(), selected.PixelFormat);

            using (var combined = Vectorization.Combine(selected, mask))
            {
                var destination = Path.GetTempFileName();

                combined.Save(destination);

                return destination;
            }
        }

        private static string Trace(string path)
        {
            var vectorPath = Path.GetTempFileName();

            var arguments = new string[] { "--svg", "--flat", "--tight", "--output", vectorPath, path };
            var delimited = string.Join(" ", arguments);

            var startInfo = new ProcessStartInfo("lib/potrace-1.11.win64/potrace.exe", delimited)
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

        private static string Enhance(string path)
        {
            var result = Path.GetTempFileName();

            using (var original = AForge.Imaging.Image.FromFile(path))
            {
                using (var filtered = Vectorization.contrastFilters.Apply(original))
                {
                    filtered.Save(result, ImageFormat.Bmp);
                }
            }

            return result;
        }

        private static Bitmap HighlightSelection(Bitmap original)
        {
            var clone = AForge.Imaging.Image.Clone(original, PixelFormat.Format32bppArgb);

            return Vectorization.selectionFilters.Apply(clone);
        }

        private static Bitmap Combine(UnmanagedImage selected, Bitmap mask)
        {
            var filters = new FiltersSequence(new IFilter[] 
            {
                new Invert(),
                new Add(selected)
            });

            return filters.Apply(mask);
        }
    }
}