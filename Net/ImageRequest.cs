using System;
using Sqor.Utils.Drawing;
using System.Threading;

namespace Sqor.Utils.Net
{
    public class ImageRequest
    {
        private Image image;
        public readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public Image Image {
            get {
                return image;
            }
            set {
                image = value;
            }
        }


        public ImageRequest (Image image)
        {
            this.image = image;      
        }

        public override bool Equals (object obj)
        {
            if (!(obj is ImageRequest))
                return false;

            var img = (ImageRequest)obj;
            return img.Image == this.Image;
        }

        public override int GetHashCode()
        {
            return Image.GetHashCode ();
        }

        public void Cancel()
        {
            this.CancellationTokenSource.Cancel ();
        }


    }
}

