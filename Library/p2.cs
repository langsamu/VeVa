// TODO: Handle pixel formats correctly
// TODO: Move to UnmanagedImage?

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

    ///// <remarks>See docs folder for sample images</remarks>
    //public class Process2 : IEnumerable<string>
    //{
    //    private IEnumerable<string> paths;

    //    public Process2(IEnumerable<string> paths)
    //    {
    //        this.paths = paths.Distinct();
    //    }

    //    private static Bitmap DetectSelections(Bitmap original)
    //    {
    //        var filters = new FiltersSequence(new IFilter[] 
    //        {
    //            new ColorFiltering(new IntRange(255, 255), new IntRange(0, 0), new IntRange(255, 255)),
    //            Grayscale.CommonAlgorithms.Y,
    //            new Threshold(72),
    //            new Invert()
    //        });

    //        return filters.Apply(original);
    //    }

    //    private static Bitmap Combine(UnmanagedImage selected, Bitmap mask)
    //    {
    //        var filters = new FiltersSequence(new IFilter[] 
    //        {
    //            new Invert(),
    //            new Add(selected)
    //        });

    //        return filters.Apply(mask);
    //    }

    //    public IEnumerator<string> GetEnumerator()
    //    {
    //        foreach (var path in this.paths)
    //        {
    //            foreach (var item in x(path))
    //            {
    //                yield return item;
    //            }
    //        }
    //    }

    //    private static IEnumerable<string> x(string path)
    //    {
    //        using (var original = AForge.Imaging.Image.FromFile(path))
    //        {
    //            using (var selectionsImage = Process2.DetectSelections(original))
    //            {
    //                var counter = new BlobCounter();
    //                counter.BlobsFilter = new ExcludeOriginalFilter(original);
    //                counter.FilterBlobs = true;
    //                counter.ObjectsOrder = ObjectsOrder.Area;
    //                counter.ProcessImage(selectionsImage);

    //                var blobs = counter.GetObjects(original, false);

    //                // No blobs means no selection, no cutting required
    //                if (!blobs.Any())
    //                {
    //                    yield return path;
    //                }

    //                foreach (var blob in blobs)
    //                {
    //                    var selected = blob.Image;

    //                    counter.ExtractBlobsImage(selectionsImage, blob, false);

    //                    var mask = AForge.Imaging.Image.Clone(blob.Image.ToManagedImage(), selected.PixelFormat);

    //                    using (var combined = Process2.Combine(selected, mask))
    //                    {
    //                        var destination = Path.GetTempFileName();

    //                        combined.Save(destination);

    //                        yield return destination;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return this.GetEnumerator();
    //    }
    //}

    /// <remarks>See docs folder for sample images</remarks>
    public class p2
    {
        public static IEnumerable<string> Do(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                foreach (var selectionPath in CropSelections(path))
                {
                    yield return selectionPath;
                }
            }
        }

        private static IEnumerable<string> CropSelections(string path)
        {
            using (var original = AForge.Imaging.Image.FromFile(path))
            {
                using (var selectionsImage = DetectSelections(original))
                {
                    var counter = new BlobCounter();
                    counter.BlobsFilter = new ExcludeOriginalFilter(original);
                    counter.FilterBlobs = true;
                    counter.ObjectsOrder = ObjectsOrder.Area;

                    counter.ProcessImage(selectionsImage);

                    var blobs = counter.GetObjects(original, false);

                    // No blobs means no selection, no cutting required, process whole image
                    if (!blobs.Any())
                    {
                        yield return path;
                    }

                    foreach (var blob in blobs)
                    {
                        var selected = blob.Image;

                        counter.ExtractBlobsImage(selectionsImage, blob, false);

                        var mask = AForge.Imaging.Image.Clone(blob.Image.ToManagedImage(), selected.PixelFormat);

                        using (var combined = Combine(selected, mask))
                        {
                            var destination = Path.GetTempFileName();

                            combined.Save(destination);

                            yield return destination;
                        }
                    }
                }
            }
        }

        private static Bitmap DetectSelections(Bitmap original)
        {
            var clone = AForge.Imaging.Image.Clone(original, PixelFormat.Format32bppArgb);

            var filters = new FiltersSequence(new IFilter[] 
            {
                new ColorFiltering(new IntRange(255, 255), new IntRange(0, 0), new IntRange(255, 255)),
                Grayscale.CommonAlgorithms.Y,
                new Threshold(72),
                new Invert()
            });

            return filters.Apply(clone);
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
    }
}
