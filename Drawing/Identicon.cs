using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

#if MONOTOUCH

namespace Sqor.Utils.Drawing
{
    /// <summary>
    /// This class generates identicons in a given size using the idea of Don Park, found at:
    /// http://www.docuverse.com/blog/donpark/2007/01/18/visual-security-9-block-ip-identification
    /// Algorithm is explained here
    /// http://www.docuverse.com/blog/donpark/2007/01/19/identicon-updated-and-source-released
    /// This is a new implementation, I didn't even look at the java source code :-\
    /// </summary>
    public class Identicon
    {        
        private static List<GraphicsPath> shapes = new List<GraphicsPath>(16);
        private static List<GraphicsPath> symshapes = new List<GraphicsPath>(4);
        private static bool initialized = false;

        private static void Initialize() 
        {
            for (int i = 0; i < 16; i++)
            {
                GraphicsPath gp = new GraphicsPath();
                shapes.Add(gp);    
            }

            // initializing the 16 shapes

            #region obere Reihe

            PointF[] p0 = new PointF[4];
            p0[0].X = 0; p0[0].Y = 0;
            p0[1].X = 1; p0[1].Y = 0;
            p0[2].X = 0; p0[2].Y = 1;
            p0[3].X = 1; p0[3].Y = 1;
            shapes[0].AddPolygon(p0);

            PointF[] p1 = new PointF[3];
            p1[0].X = 0; p1[0].Y = 0;
            p1[1].X = 1; p1[1].Y = 0;
            p1[2].X = 0; p1[2].Y = 1;
            shapes[1].AddPolygon(p1);

            PointF[] p2 = new PointF[3];
            p2[0].X = 0; p2[0].Y = 1;
            p2[1].X = 0.5F; p2[1].Y = 0;
            p2[2].X = 1; p2[2].Y = 1;
            shapes[2].AddPolygon(p2);

            PointF[] p3 = new PointF[4];
            p3[0].X = 0; p3[0].Y = 0;
            p3[1].X = 0.5F; p3[1].Y = 0;
            p3[2].X = 0.5F; p3[2].Y = 1;
            p3[3].X = 0; p3[3].Y = 1;
            shapes[3].AddPolygon(p3);

            PointF[] p4 = new PointF[4];
            p4[0].X = 0.5F; p4[0].Y = 0;
            p4[1].X = 1; p4[1].Y = 0.5F;
            p4[2].X = 0.5F; p4[2].Y = 1;
            p4[3].X = 0; p4[3].Y = 0.5F;
            shapes[4].AddPolygon(p4);

            PointF[] p5 = new PointF[4];
            p5[0].X = 0; p5[0].Y = 0;
            p5[1].X = 1; p5[1].Y = 0.5F;
            p5[2].X = 1; p5[2].Y = 1;
            p5[3].X = 0.5F; p5[3].Y = 1;
            shapes[5].AddPolygon(p5);

            PointF[] p61 = new PointF[3];
            p61[0].X = 0.5F; p61[0].Y = 0;
            p61[1].X = 0.75F; p61[1].Y = 0.5F;
            p61[2].X = 0.25F; p61[2].Y = 0.5F;
            shapes[6].AddPolygon(p61);
            PointF[] p62 = new PointF[3];
            p62[0].X = 0.75F; p62[0].Y = 0.5F;
            p62[1].X = 1; p62[1].Y = 1;
            p62[2].X = 0.5F; p62[2].Y = 1;
            shapes[6].AddPolygon(p62);
            PointF[] p63 = new PointF[3];
            p63[0].X = 0.25F; p63[0].Y = 0.5F;
            p63[1].X = 0.5F; p63[1].Y = 1;
            p63[2].X = 0; p63[2].Y = 1;
            shapes[6].AddPolygon(p63);

            PointF[] p7 = new PointF[3];
            p7[0].X = 0; p7[0].Y = 0;
            p7[1].X = 1; p7[1].Y = 0.5F;
            p7[2].X = 0.5F; p7[2].Y = 1;

            #endregion

            #region untere Reihe

            PointF[] p8 = new PointF[4];
            p8[0].X = 0.25F; p8[0].Y = 0.25F;
            p8[1].X = 0.75F; p8[1].Y = 0.25F;
            p8[2].X = 0.75F; p8[2].Y = 0.75F;
            p8[3].X = 0.25F; p8[3].Y = 0.75F;
            shapes[8].AddPolygon(p8);

            PointF[] p91 = new PointF[3];
            p91[0].X = 0.5F; p91[0].Y = 0;
            p91[1].X = 1; p91[1].Y = 0;
            p91[2].X = 0.5F; p91[2].Y = 0.5F;
            shapes[9].AddPolygon(p91);
            PointF[] p92 = new PointF[3];
            p92[0].X = 0; p92[0].Y = 0.5F;
            p92[1].X = 0.5F; p92[1].Y = 0.5F;
            p92[2].X = 0; p92[2].Y = 1;
            shapes[9].AddPolygon(p92);

            PointF[] p10 = new PointF[4];
            p10[0].X = 0; p10[0].Y = 0;
            p10[1].X = 0.5F; p10[1].Y = 0;
            p10[2].X = 0.5F; p10[2].Y = 0.5F;
            p10[3].X = 0; p10[3].Y = 0.5F;
            shapes[10].AddPolygon(p10);

            PointF[] p11 = new PointF[3];
            p11[0].X = 0; p11[0].Y = 0.5F;
            p11[1].X = 1; p11[1].Y = 0.5F;
            p11[2].X = 0.5F; p11[2].Y = 1;
            shapes[11].AddPolygon(p11);

            PointF[] p12 = new PointF[3];
            p12[0].X = 0; p12[0].Y = 1;
            p12[1].X = 0.5F; p12[1].Y = 0.5F;
            p12[2].X = 1; p12[2].Y = 1;
            shapes[12].AddPolygon(p12);

            PointF[] p13 = new PointF[3];
            p13[0].X = 0.5F; p13[0].Y = 0;
            p13[1].X = 0.5F; p13[1].Y = 0.5F;
            p13[2].X = 0; p13[2].Y = 0.5F;
            shapes[13].AddPolygon(p13);

            PointF[] p14 = new PointF[3];
            p14[0].X = 0; p14[0].Y = 0;
            p14[1].X = 0.5F; p14[1].Y = 0;
            p14[2].X = 0; p14[2].Y = 0.5F;
            shapes[14].AddPolygon(p14);

            #endregion

            symshapes.Add(shapes[0]);
            symshapes.Add(shapes[4]);
            symshapes.Add(shapes[8]);
            symshapes.Add(shapes[15]);

            initialized = true;
        }

        /// <summary>
        /// creates a new Identicon
        /// </summary>
        /// <param name="source">source value, e.g. a hash created from a username, an ip address,..</param>
        /// <param name="size">side length</param>
        /// <returns>a 24 bpp bitmap</returns>
        public static byte[] CreateIdenticonData(int source, uint size, bool outline)
        {
            var logo = CreateIdenticon(source, size, outline);
            return logo.SaveToByteArray();
        }
        
        /// <summary>
        /// creates a new Identicon
        /// </summary>
        /// <param name="source">source value, e.g. a hash created from a username, an ip address,..</param>
        /// <param name="size">side length</param>
        /// <returns>a 24 bpp bitmap</returns>
        public static Bitmap CreateIdenticon(int source, uint size, bool outline)
        {
            if (size == 0) return null;
            if (!initialized) Initialize();
            Bitmap g = new Bitmap((int) size,(int) size);            
            g.FillColor = Color.FromRgba(255, 255, 255);
            g.FillRect(new Rectangle(0, 0, (int)size, (int)size));

            int centerindex = source & 3; // 2 lowest bits
            int sideindex = (source >> 2) & 15; // next 4 bits for side shapes
            int cornerindex = (source >> 6) & 15; // next 4 for corners
            int siderot = (source >> 10) & 3; // 2 bits for side offset rotation
            int cornerrot = (source >> 12) & 3; // 2 bits for corner offset rotation
           
            // inversion per shape

            // calculate color
            int red = (source >> 14) & 31;
            int green = (source >> 19) & 31;
            int blue = (source >> 24) & 31;
            Color shapecolor = Color.FromRgba(red * 8, green * 8, blue * 8);

            // remaining bits to decide shape flipping
            bool flipcenter = ((source >> 29) & 1) == 1;
            bool flipcorner = ((source >> 30) & 1) == 1;
            bool flipsides = ((source >> 31) & 1) == 1;            

            var sb = shapecolor;
            var wb = Color.FromRgba(255, 255, 255);
            var p = Color.FromRgba(0, 0, 0);
                        
            #region Transform and move shapes into position

            // comment out the "DrawPath" statements if you don't want an outline
            float shapesize = (float) size / 3.0F; // adjust the shape size to the target bitmap size
            
            Action<GraphicsPath> setSize = path => path.Scale(shapesize, shapesize);
            Action<GraphicsPath> setTlr = path => path.Translate(shapesize, 0);
            Action<GraphicsPath> setTld = path => path.Translate(0, shapesize);

            // center
            GraphicsPath g5 = (GraphicsPath)symshapes[centerindex].Clone();
            setSize(g5);
            setTlr(g5);
            setTld(g5);
            if (flipcenter)
            {
                g.FillColor = sb;
                g.FillRect(shapesize, shapesize, shapesize, shapesize);
                g.FillColor = wb;
                g.FillPath(g5);
            }
            else 
            {
                g.FillColor = sb;
                g.FillPath(g5);
            }
            if (outline) 
            {
                g.StrokeColor = p;
                g.StrokePath(g5);
            }
        
            // corner top left
            GraphicsPath g1 = shapes[cornerindex].Clone() as GraphicsPath;
            RotatePath90(g1, cornerrot);
            setSize(g1);
            if (flipcorner)
            {
                g.FillColor = sb;
                g.FillRect(0, 0, shapesize, shapesize);
                g.FillColor = wb;
                g.FillPath(g1);
            }
            else 
            {
                g.FillColor = sb;
                g.FillPath(g1);
            }
            if (outline) 
            {
                g.StrokeColor = p;
                g.StrokePath(g1);
            }

            // corner bottom left
            GraphicsPath g7 = shapes[cornerindex].Clone() as GraphicsPath;
            RotatePath90(g7, cornerrot + 1);
            setSize(g7);
            setTld(g7);
            setTld(g7);
            if (flipcorner)
            {
                g.FillColor = sb;
                g.FillRect(0, 2 * shapesize, shapesize, shapesize);
                g.FillColor = wb;
                g.FillPath(g7);
            }
            else 
            {
                g.FillColor = sb;
                g.FillPath(g7);
            }
            if (outline) 
            {
                g.StrokeColor = p;
                g.StrokePath(g7);
            }

            // corner bottom right
            GraphicsPath g9 = shapes[cornerindex].Clone() as GraphicsPath;
            RotatePath90(g9, cornerrot + 2);
            setSize(g9);
            setTld(g9);
            setTld(g9);
            setTlr(g9);
            setTlr(g9);
            if (flipcorner)
            {
                g.FillColor = sb;
                g.FillRect(2 * shapesize, 2 * shapesize, shapesize, shapesize);
                g.FillColor = wb;
                g.FillPath(g9);
            }
            else 
            {
                g.FillColor = sb;
                g.FillPath(g9);
            }
            if (outline) 
            {
                g.StrokeColor = p;
                g.StrokePath(g9);
            }

            // corner top right
            GraphicsPath g3 = shapes[cornerindex].Clone() as GraphicsPath;
            RotatePath90(g3, cornerrot + 3);
            setSize(g3);
            setTlr(g3);
            setTlr(g3);
            if (flipcorner)
            {
                g.FillColor = sb;
                g.FillRect(2 * shapesize, 0, shapesize, shapesize);
                g.FillColor = wb;
                g.FillPath(g3);
            }
            else 
            {
                g.FillColor = sb;
                g.FillPath(g3);
            }
            if (outline) 
            {
                g.StrokeColor = p;
                g.StrokePath(g3);
            }

            // top side
            GraphicsPath g2 = shapes[sideindex].Clone() as GraphicsPath;
            RotatePath90(g2, siderot);
            setSize(g2);
            setTlr(g2);
            if (flipsides)
            {
                g.FillColor = sb;
                g.FillRect(shapesize, 0, shapesize, shapesize);
                g.FillColor = wb;
                g.FillPath(g2);
            }
            else 
            {
                g.FillColor = sb;
                g.FillPath(g2);
            }
            if (outline) 
            {
                g.StrokeColor = p;
                g.StrokePath(g2);
            }

            // left side
            GraphicsPath g4 = shapes[sideindex].Clone() as GraphicsPath;
            RotatePath90(g4, siderot + 1);
            setSize(g4);
            setTld(g4);
            if (flipsides)
            {
                g.FillColor = sb;
                g.FillRect(0, shapesize, shapesize, shapesize);
                g.FillColor = wb;
                g.FillPath(g4);
            }
            else 
            {
                g.FillColor = sb;
                g.FillPath(g4);
            }
            if (outline) 
            {
                g.StrokeColor = p;
                g.StrokePath(g4);
            }

            // bottom side
            GraphicsPath g8 = shapes[sideindex].Clone() as GraphicsPath;
            RotatePath90(g8, siderot + 2);
            setSize(g8);
            setTlr(g8);
            setTld(g8);
            setTld(g8);
            if (flipsides)
            {
                g.FillColor = sb;
                g.FillRect(shapesize, 2 * shapesize, shapesize, shapesize);
                g.FillColor = wb;
                g.FillPath(g8);
            }
            else 
            {
                g.FillColor = sb;
                g.FillPath(g8);
            }
            if (outline) 
            {
                g.StrokeColor = p;
                g.StrokePath(g8);
            }
            
            // right side
            GraphicsPath g6 = shapes[sideindex].Clone() as GraphicsPath;
            RotatePath90(g6, siderot + 3);
            setSize(g6);
            setTlr(g6);
            setTlr(g6);
            setTld(g6);
            if (flipsides)
            {
                g.FillColor = sb;
                g.FillRect(2 * shapesize, shapesize, shapesize, shapesize);
                g.FillColor = wb;
                g.FillPath(g6);
            }
            else 
            {
                g.FillColor = sb;
                g.FillPath(g6);
            }
            if (outline) 
            {
                g.StrokeColor = p;
                g.StrokePath(g6);
            }

            #endregion

            return g;
        }

        /// <summary>
        /// helper func to rotate input matrix
        /// </summary>
        private static void RotatePath90(GraphicsPath gp, int times)
        {
            times = times % 4;
            for (int i = 0; i < times; i++)
                gp.RotateAt((float)(Math.PI / 2), new PointF(0.5F, 0.5f));
        }
    }
}

#endif