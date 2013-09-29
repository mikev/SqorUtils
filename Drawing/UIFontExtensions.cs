#if MONOTOUCH

using System;
using MonoTouch.UIKit;

namespace Sqor.Utils.Drawing
{
    /// <summary>
    /// Provides extension methods that allow you derive new fonts from existing
    /// fonts by tweaking their size of face (bold).
    /// </summary>
    public static class UIFontExtensions
    {
        public static UIFont Derive(this UIFont font, float size) 
        {
            return UIFont.FromName(font.Name, size);
        }

        public static UIFont DeriveBold(this UIFont font, float? size = null) 
        {
            var name = font.Name.Split('-')[0];
            return UIFont.FromName(name + "-Bold", size ?? font.PointSize);
        }
        
        public static UIFont DeriveMedium(this UIFont font, float? size = null)
        {
            var name = font.Name.Split('-')[0];
            return UIFont.FromName(name + "-Medium", size ?? font.PointSize);            
        }
        
        public static UIFont DeriveLight(this UIFont font, float? size = null)
        {
            var name = font.Name.Split('-')[0];
            return UIFont.FromName(name + "-Light", size ?? font.PointSize);            
        }
        
        public static string ToHtmlFont(this UIFont font)
        {
            return font.PointSize * 3 + "pt " + font.FamilyName;
        }
    }
}

#endif