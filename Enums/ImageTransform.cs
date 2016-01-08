using System;

namespace Sqor.Utils.Enums
{
    /// <summary>
    /// Describes transformations to the images.
    /// </summary>
    [Flags]
    public enum ImageTransform
    {
        /// <summary>
        /// No transformations.
        /// </summary>
        None = 0,

        /// <summary>
        /// Apply Exif rotation.
        /// </summary>
        RotateExif = 1,

        /// <summary>
        /// Scale the height and width so that one dimension matches the specified size and the other dimension is larger than the specified size.
        /// </summary>
        ScaleContainBoth = 2,
        /// <summary>
        /// Scale the height and width so that one dimension matches the specified size and the other dimension is smaller than the specified size.
        /// </summary>
        ScaleFitBoth = 4,

        /// <summary>
        /// Scale the width of the image to match the specified width, let the height change to maintain aspect ratio.
        /// </summary>
        ScaleFitWidth = 8,

        /// <summary>
        /// Scale the height of the image to match the specified height, let the width change to maintain aspect ratio.
        /// </summary>
        ScaleFitHeight = 16,

        /// <summary>
        /// Crop the image
        /// </summary>
        CropFocusObjects = 32,

        /// <summary>
        /// Crop the image, focus on the top center. (Removing sides and lower section when necessary).
        /// </summary>
        CropFocusTopCenter = 64,

        /// <summary>
        /// Crop the image, focus on the top left. (Removing the right and bottom section when necessary).
        /// </summary>
        CropFocusTopLeft = 128,

        /// <summary>
        /// Crop the image, focus on the bottom center. (Removing the top and sides when necessary).
        /// </summary>
        CropFocusBottomCenter = 256,

        /// <summary>
        /// Crop the image, focus on the bottom left. (Removing the top and right when necessary).
        /// </summary>
        CropFocusBottomLeft = 512,

        /// <summary>
        /// Crop the image focus on the center 1/6 of the distance from the top. (Removing from the bottom, sides, and top when necessary).
        /// </summary>
        CropFocusSixthDownCenter = 1024,

        /// <summary>
        /// Crop the image focus on the center of the image, removing from all sides when necessary.
        /// </summary>
        CropFocusCenter = 2048,

        /// <summary>
        /// If the format is png, convert it to a jpg.
        /// </summary>
        //[Obsolete("Use ConvertToJpg instead.")]
        PngToJpg = 4096,

        /// <summary>
        /// Convert image to jpeg, may be ignored for animated gifs in the future.
        /// </summary>
        ConvertToJpg = 4096,

        AddPlayButton = 8192,
        CrosspostPresentation = 16384,
        CropToCircle = 32768,
        FacebookPortrait = 65536

    }
}