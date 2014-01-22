using System;
using Sqor.Utils.Json;
using Sqor.Utils.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

#if MONOTOUCH 

using Sqor.Utils.Data;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

#endif

namespace Sqor.Utils.Drawing
{
    public enum ImageSource { None, Url, ByteArray, Native, Custom }
    
    [JsonConverter(typeof(ImageJsonConverter))]
    public struct Image
    {
        private readonly ImageSource source;
        private readonly string url;
        private readonly byte[] byteArray;
        private readonly CancellationTokenSource cancellationTokenSource;
        
        #if MONOTOUCH
        private readonly UIImage nativeImage;
        #endif
        
        public Image(
            ImageSource source, 
            string url, 
            byte[] byteArray
#if MONOTOUCH
            , UIImage nativeImage
#else
            , object nativeImage
#endif
        ) : this()
        {
            if (url != null && !url.Contains("://"))
                throw new InvalidOperationException("Invalid Url: " + url);
        
            this.source = source;
            this.url = url;
            this.byteArray = byteArray;
            #if MONOTOUCH
            this.nativeImage = nativeImage;
            cancellationTokenSource = new CancellationTokenSource();
            #endif
        }
        
        public ImageSource Source
        {
            get { return source; }
        }
        
        public string Url
        {
            get { return url; }
        }
        
        public byte[] ByteArray
        {
            get { return byteArray; }
        }

        public CancellationTokenSource CancellationTokenSource
        {
            get { return this.cancellationTokenSource; }
        }

        public Task<byte[]> ToByteArray()
        {
            switch (Source)
            {
                case ImageSource.Url:
                    return Http.To(Url).Get().AsBinary();
                case ImageSource.ByteArray:
                    return Task.FromResult(ByteArray);
#if MONOTOUCH
                case ImageSource.Native:
                    return Task.FromResult(NativeImage.SaveToByteArray());
#endif
                case ImageSource.None:
                    return Task.FromResult((byte[])null);
                default:
                    throw new InvalidOperationException();
            }
        }
        
        public override bool Equals(object obj)
        {
            if (!(obj is Image))
                return false;
                
            return base.Equals((Image)obj);
        }
        
        public bool Equals(Image other)
        {
            if (other.Source != Source)
                return false;
                
            switch (Source)
            {
                case ImageSource.Url:
                    return other.Url == Url;
                case ImageSource.ByteArray:
                    return other.ByteArray.SequenceEqual(ByteArray);
                case ImageSource.None:
                    return true;
#if MONOTOUCH
                case ImageSource.Native:
                    return other.NativeImage == NativeImage;
#endif
                default:
                    throw new InvalidOperationException();
            }
        }
        
        public override int GetHashCode()
        {
            switch (Source)
            {   
                case ImageSource.Url:
                    return Url.GetHashCode();
                case ImageSource.ByteArray:
                    return ByteArray.GetHashCode();
#if MONOTOUCH
                case ImageSource.Native:
                    return NativeImage.GetHashCode();
#endif
                default:
                    return 0;
            }
        }
        
        #if MONOTOUCH
        
        public UIImage NativeImage
        {
            get { return nativeImage; }
        }
        
        #endif
        
        public Image DefaultTo(Image image)
        {
            if (Source == ImageSource.None)
                return image;
            else
                return this;
        }
        
        public static implicit operator Image(string url)
        {
            return !string.IsNullOrEmpty(url) ? new Image(ImageSource.Url, url, null, null) : new Image(ImageSource.None, null, null, null);
        }
        
        public static implicit operator Image(byte[] data)
        {
            return new Image(data != null ? ImageSource.ByteArray : ImageSource.None, null, data, null);
        }
#if MONOTOUCH        
        public static implicit operator Image(NSData data)
        {
            return data.ToByteArray();
        }
#endif
        
        public static bool operator ==(Image image1, Image image2)
        {
            return image1.Equals(image2);
        }
        
        public static bool operator !=(Image image1, Image image2)
        {
            return !image1.Equals(image2);
        }
        
        #if MONOTOUCH
        
        public static implicit operator Image(UIImage image)
        {
            return new Image(ImageSource.Native, null, null, image);
        }
        
        #endif
        
        public override string ToString()
        {
            return string.Format("[Image: Source={0}, Url={1}, ByteArray={2}, NativeImage={3}, CancellationToken={4}]", 
                Source, 
                Url, 
                ByteArray != null ? "byte[" + ByteArray.Length + "]" : "null", 
#if MONOTOUCH
                NativeImage != null ? "{value}" : "null"
#else
                null
#endif
                ,cancellationTokenSource != null ? cancellationTokenSource.IsCancellationRequested.ToString() : "null"
            );
        }

        class ImageJsonConverter : IJsonConverter
        {
            public string TypeDescription
            {
                get { return "url"; }
            }

            public JsonValue ToJson(object o)
            {
                var image = (Image)o;
                if (image.Source == ImageSource.None)
                    return new JsonPrimitive();
                if (image.Source != ImageSource.Url)
                    throw new InvalidOperationException("Cannot convert a non-Url based image into json");
                return ((Image)o).Url;
            }

            public object FromJson(JsonValue json)
            {
                return new Image(ImageSource.Url, json, null, null);
            }
        }
    }
}

