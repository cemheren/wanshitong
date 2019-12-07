using System.Drawing;
using System.Drawing.Drawing2D;

public static class ImageHelpers
{
    public static Bitmap Crop(this Bitmap source, CropImageModel cropModel)
    {
        Bitmap nb = new Bitmap((int)cropModel.Width, (int)cropModel.Height);

        nb.SetResolution(source.HorizontalResolution, source.VerticalResolution);

        Graphics gfx = Graphics.FromImage(nb);
        gfx.SmoothingMode = SmoothingMode.AntiAlias;
        gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
        gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
        gfx.DrawImage(source, 
            new Rectangle(0, 0, (int)cropModel.Width, (int)cropModel.Height), 
                (int)cropModel.Left, (int)cropModel.Top, (int)cropModel.Width, (int)cropModel.Height, GraphicsUnit.Pixel);

        // Graphics g = Graphics.FromImage(nb);
        // g.DrawImage(source, -(int)cropModel.Left, -(int)cropModel.Top);
        return nb;
    }
}