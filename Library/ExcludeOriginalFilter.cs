namespace langsamu.VeVa
{
    using AForge.Imaging;
    using System.Drawing;

    public class ExcludeOriginalFilter : IBlobsFilter
    {
        private Rectangle original;

        public ExcludeOriginalFilter(Bitmap original)
        {
            this.original = new Rectangle(0, 0, original.Width, original.Height);
        }

        public bool Check(Blob blob)
        {
            return !(blob.Rectangle == this.original);
        }
    }
}