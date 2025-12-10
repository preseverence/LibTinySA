using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace LibTinySA.Windows
{
  public static class ImageHelper
  {
    /// <summary>
    /// Creates GDI Bitmap from data array provided by LibTinySA and updates it.
    /// </summary>
    /// <param name="img">Pixels array.</param>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <returns>GDI Bitmap generated.</returns>
    /// <seealso cref="TinySAControl.ImageAcquired"/>
    public static Bitmap BuildGDIImage(ushort[,] img, int width, int height)
    {
      Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
      UpdateGDIImage(bmp, img);
      return bmp;
    }

    /// <summary>
    /// Updates GDI Bitmap from data array provided by LibTinySA 
    /// </summary>
    /// <param name="bmp">Existing bitmap created by <see cref="BuildGDIImage"/>.</param>
    /// <param name="img">Pixels array.</param>
    public static unsafe void UpdateGDIImage(Bitmap bmp, ushort[,] img)
    {
      int width = bmp.Width;
      int height = bmp.Height;
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
    }

    /// <summary>
    /// Creates WPF Bitmap from data array provided by LibTinySA and updates it.
    /// </summary>
    /// <param name="img">Pixels array.</param>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <returns>WPF Bitmap generated.</returns>
    /// <seealso cref="TinySAControl.ImageAcquired"/>
    public static WriteableBitmap BuildWPFImage(ushort[,] img, int width, int height)
    {
      WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr565, null);
      UpateWPFImage(bitmap, img);
      return bitmap;
    }

    /// <summary>
    /// Updates WPF Bitmap from data array provided by LibTinySA 
    /// </summary>
    /// <param name="bmp">Existing bitmap created by <see cref="BuildWPFImage"/>.</param>
    /// <param name="img">Pixels array.</param>
    public static void UpateWPFImage(WriteableBitmap bmp, ushort[,] img)
    {
      int width = bmp.PixelWidth;
      int height = bmp.PixelHeight;

      bmp.Lock();
      bmp.WritePixels(new System.Windows.Int32Rect(0, 0, width, height), img, width * 2, 0);
      bmp.Unlock();
    }
  }
}
