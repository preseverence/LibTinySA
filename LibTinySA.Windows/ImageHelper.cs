using System.Drawing;
using System.Drawing.Imaging;

namespace LibTinySA.Windows
{
  public static class ImageHelper
  {
    /// <summary>
    /// Creates .NET Bitmap from data array provided by LibTinySA.
    /// </summary>
    /// <param name="img">Pixels array.</param>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <returns>.NET Bitmap generated.</returns>
    /// <seealso cref="TinySAControl.ImageAcquired"/>
    public static unsafe Bitmap BuildImage(ushort[,] img, int width, int height)
    {
      Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);

      BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppRgb565);

      try
      {
        fixed (ushort* imgPtr = img)
        {
          byte* bytePtr = (byte*)imgPtr;
          byte* bmpPtr = (byte*)data.Scan0;

          for (int i = 0; i < width * height; i++)
          {
            byte b = *bytePtr;
            bytePtr++;
            *bmpPtr = *bytePtr;
            bytePtr++;
            bmpPtr++;
            *bmpPtr = b;
            bmpPtr++;
          }
        }
      }
      catch
      {
        // ignore
      }
      finally
      {
        bmp.UnlockBits(data);
      }

      return bmp;
    }
  }
}
