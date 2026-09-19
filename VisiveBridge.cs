using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Visive;

namespace Trakto;

public static class VisiveBridge
{
    public static void UpdateBitmap(ref WriteableBitmap? bitmap, FrameObject frameObj)
    {
        if (bitmap == null || bitmap.PixelSize.Width != frameObj.width || bitmap.PixelSize.Height != frameObj.height)
        {
            bitmap?.Dispose(); // Dispose old unmanaged memory
            bitmap = new WriteableBitmap(
                new PixelSize(frameObj.width, frameObj.height),
                new Vector(96, 96),
                PixelFormat.Rgba8888,
                AlphaFormat.Unpremul);
        }

        using (var frameBuffer = bitmap.Lock())
        {
            frameObj.WriteToBuffer(frameBuffer.Address, frameObj.width * frameObj.height * 4);
        }
    }
}
