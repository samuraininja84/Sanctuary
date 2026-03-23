using System;
using UnityEngine;

namespace Sanctuary
{
    /// <summary>
    /// Provides utility methods for encoding and decoding <see cref="Texture2D"/> objects to and from base64 strings.
    /// </summary>
    /// <remarks>
    /// This class is designed to facilitate the conversion of <see cref="Texture2D"/> objects to
    /// base64-encoded strings for storage or transmission, and to decode base64 strings back into <see cref="Texture2D"/> objects.
    /// </remarks>
    public static class PreviewUtility
    {
        /// <summary>
        /// Encode a Texture2D object to a base64 string
        /// </summary>
        /// <param name="previewImage"></param>
        /// <returns></returns>
        public static string EncodePreview(Texture2D previewImage) 
        {
            // Encode the preview image to a JPG byte array
            byte[] previewBytes = previewImage.EncodeToJPG();

            // Convert the byte array to a base64 string
            string previewBase64 = Convert.ToBase64String(previewBytes);

            // Return the base64 string
            return previewBase64;
        }

        /// <summary>
        /// Decode a base64 string to a Texture2D object
        /// </summary>
        /// <param name="previewBase64"></param>
        /// <returns></returns>
        public static Texture2D DecodePreview(string previewBase64) 
        {
            // Convert the base64 string to a byte array
            byte[] previewBytes = Convert.FromBase64String(previewBase64);

            // Create a new texture and load the image from the byte array
            Texture2D previewImage = new Texture2D(0, 0);
            if (ImageConversion.LoadImage(previewImage, previewBytes))
            {
                return previewImage;
            }

            // Return null if the image could not be loaded
            return null;
        }
    }
}
