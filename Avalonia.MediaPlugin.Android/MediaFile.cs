using Android.Content;
using Uri = Android.Net.Uri;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.MediaPlugin.Abstractions;

namespace Avalonia.MediaPlugin.Android
{
    /// <summary>
    /// 
    /// </summary>
    public static class MediaFileExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task<MediaFile> GetMediaFileExtraAsync(this Intent self, Context context)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (context == null)
                throw new ArgumentNullException("context");

            var action = self.GetStringExtra("action");
            if (action == null)
                throw new ArgumentException("Intent was not results from MediaPicker", "self");

            Uri uri = (Uri)self.GetParcelableExtra("MediaFile");
            var isPhoto = self.GetBooleanExtra("isPhoto", false);
            var path = (Uri)self.GetParcelableExtra("path");
            var saveToAlbum = false;
            try
            {
                saveToAlbum = (bool)self.GetParcelableExtra("album_save");
            }
            catch { }

            return MediaPickerActivity.GetMediaFileAsync(context, 0, action, isPhoto, ref path, uri, saveToAlbum).ContinueWith(t => t.Result.ToTask()).Unwrap();
        }
    }

}
