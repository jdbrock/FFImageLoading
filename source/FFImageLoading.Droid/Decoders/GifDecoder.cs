﻿using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using FFImageLoading.Work;
using FFImageLoading.Helpers;
using FFImageLoading.Extensions;
using FFImageLoading.Config;

namespace FFImageLoading.Decoders
{
    public class GifDecoder : IDecoder<Bitmap>
    {
        public async Task<IDecodedImage<Bitmap>> DecodeAsync(Stream stream, string path, ImageSource source, ImageInformation imageInformation, TaskParameter parameters)
        {
            var result = new DecodedImage<Bitmap>();
            var helper = new PlatformGifHelper();

            await helper.ReadGifAsync(stream, path, parameters);
            result.IsAnimated = helper.Frames.Count > 1;

            if (result.IsAnimated && Configuration.AnimateGifs)
            {
                result.AnimatedImages = new AnimatedImage<Bitmap>[helper.Frames.Count];

                for (int i = 0; i < helper.Frames.Count; i++)
                {
                    var animatedImage = new AnimatedImage<Bitmap>
                    {
                        Delay = helper.Frames[i].Delay,
                        Image = helper.Frames[i].Image
                    };
                    result.AnimatedImages[i] = animatedImage;
                }
            }
            else
            {
                result.IsAnimated = false;
                result.Image = helper.Frames[0].Image;
            }

            if (result.Image != null)
            {
                imageInformation.SetOriginalSize(result.Image.Width, result.Image.Height);
                imageInformation.SetCurrentSize(result.Image.Width, result.Image.Height);
            }
            else if (result.AnimatedImages != null)
            {
                if (result.AnimatedImages.Length > 0)
                {
                    if (result.AnimatedImages[0].Image != null)
                    {
                        imageInformation.SetOriginalSize(result.AnimatedImages[0].Image.Width, result.AnimatedImages[0].Image.Height);
                        imageInformation.SetCurrentSize(result.AnimatedImages[0].Image.Width, result.AnimatedImages[0].Image.Height);
                    }
                }
            }

            return result;
        }

        public Configuration Configuration => ImageService.Instance.Config;

        public IMiniLogger Logger => ImageService.Instance.Config.Logger;

        public class PlatformGifHelper : GifHelperBase<Bitmap>
        {
            protected override int DipToPixels(int dips)
            {
                return dips.DpToPixels();
            }

            protected override Task<Bitmap> ToBitmapAsync(int[] data, int width, int height, int downsampleWidth, int downsampleHeight)
            {
                Bitmap bitmap;
                bitmap = Bitmap.CreateBitmap(data, width, height, Bitmap.Config.Argb8888);
				bitmap.HasAlpha = false;

                if (downsampleWidth != 0 && downsampleHeight != 0)
                {
                    var old = bitmap;

                    bitmap = Bitmap.CreateScaledBitmap(old, downsampleWidth, downsampleHeight, false);

                    old.Recycle();
                    old.TryDispose();
                }

                return Task.FromResult(bitmap);
            }
        }
    }
}
