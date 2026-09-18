using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Visive;

namespace Trakto;

public static class VisiveBridge
{
    public static WriteableBitmap FrameToBitmap(FrameObject frameObj)
    {
        var bitmap = new WriteableBitmap(
            new PixelSize(frameObj.width, frameObj.height),
            new Vector(96, 96),
            PixelFormat.Rgba8888,
            AlphaFormat.Unpremul);

        using (var frameBuffer = bitmap.Lock())
        {
            // Allocate a buffer to receive the 8-bit RGBA pixels from Visive
            byte[] rawBytes = new byte[frameObj.width * frameObj.height * 4];
            
            // This method down-shifts the 10-bit color data and fills the 8-bit array
            frameObj.WriteToBuffer(rawBytes);

            // Fast copy into the Avalonia bitmap backbuffer
            Marshal.Copy(rawBytes, 0, frameBuffer.Address, rawBytes.Length);
        }

        return bitmap;
    }
}
