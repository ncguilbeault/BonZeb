namespace BonZeb
{
    public class RawImageData
    {
        // Structure used for creating a raw image data type.
        public byte[] ImageData { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int WidthStep { get; set; }
        public RawImageData(byte[] imageData, int width, int height, int widthStep)
        {
            ImageData = imageData;
            Width = width;
            Height = height;
            WidthStep = widthStep;
        }
    }
}
