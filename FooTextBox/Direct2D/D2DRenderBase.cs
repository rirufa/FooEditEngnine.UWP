/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using Windows.Graphics.Display;
using Windows.UI.Text;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SharpDX;
using DXGI = SharpDX.DXGI;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using FooEditEngine.UWP;

namespace FooEditEngine
{
    class D2DRenderBase : D2DRenderCommon, IDisposable
    {
        public new const int MiniumeWidth = 40;    //これ以上ないと誤操作が起こる

        protected DXGI.Device DXGIDevice;
        protected D3D11.Device1 D3DDevice;
        protected D2D.Device D2DDevice;
        protected D2D.DeviceContext D2DContext;
        FontFamily fontFamily;
        FontStyle fontStyle = FontStyle.Normal;
        FontWeight fontWeigth;
        double fontSize;

        public D2DRenderBase()
        {
            var creationFlags = SharpDX.Direct3D11.DeviceCreationFlags.VideoSupport | SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport;
#if DEBUG
            creationFlags |= SharpDX.Direct3D11.DeviceCreationFlags.Debug;
#endif
            D3D.FeatureLevel[] featureLevels ={D3D.FeatureLevel.Level_11_1,
                                                 D3D.FeatureLevel.Level_11_0,
                                                 D3D.FeatureLevel.Level_10_1,
                                                 D3D.FeatureLevel.Level_10_0,
                                                 D3D.FeatureLevel.Level_9_3,
                                                 D3D.FeatureLevel.Level_9_2,
                                                 D3D.FeatureLevel.Level_9_1};
            using (var device = new D3D11.Device(D3D.DriverType.Hardware, creationFlags, featureLevels))
            {
                this.D3DDevice = device.QueryInterface<D3D11.Device1>();
            }
            this.DXGIDevice = this.D3DDevice.QueryInterface<DXGI.Device>();
        }

        public void InitTextFormat(string fontName, double size)
        {
            base.InitTextFormat(fontName, (float)size);
            this.fontSize = size;
        }

        public void InitTextFormat(FontFamily font, double size)
        {
            base.InitTextFormat(font.Source, (float)size);
            this.fontFamily = font;
            this.fontSize = size;
        }

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);
            this.DestructRenderAndResource();
            if (this.DXGIDevice != null)
                this.DXGIDevice.Dispose();
            if (this.D3DDevice != null)
                this.D3DDevice.Dispose();
        }

        public virtual void ConstructRenderAndResoruce(double width, double height)
        {
            this.D2DDevice = new D2D.Device(D2DRenderShared.D2DFactory, this.DXGIDevice);
            this.D2DContext = new D2D.DeviceContext(this.D2DDevice, D2D.DeviceContextOptions.None);
            float dpiX, dpiY;
            this.GetDpi(out dpiX, out dpiY);
            this.D2DContext.DotsPerInch = new Size2F(dpiX, dpiY);
            this.render = this.D2DContext;
            this.renderSize = new Size(width, height);
            this.textRender = new CustomTextRenderer(this.Brushes, this.Strokes, this.Foreground);
        }

        public void ReinitTextFormat()
        {
            this.InitTextFormat(this.fontFamily.Source, (float)this.fontSize, this.GetDWFontWeigth(this.fontWeigth), this.GetDWFontStyle(this.fontStyle));
        }

        public virtual void DestructRenderAndResource()
        {
            this.Brushes.Clear();
            this.Strokes.Clear();
            if (this.textRender != null)
                this.textRender.Dispose();
            if (this.D2DDevice != null)
                this.D2DDevice.Dispose();
            if (this.D2DContext != null)
                this.D2DContext.Dispose();
        }

        public FontFamily FontFamily
        {
            get { return this.fontFamily; }
            set
            {
                this.fontFamily = value;
                this.InitTextFormat(this.fontFamily.Source, (float)this.fontSize, this.GetDWFontWeigth(this.fontWeigth), this.GetDWFontStyle(this.fontStyle));
                this.TabWidthChar = this.TabWidthChar;
            }
        }

        public double FontSize
        {
            get { return this.fontSize; }
            set
            {
                this.fontSize = value;
                this.InitTextFormat(this.fontFamily.Source, (float)this.fontSize, this.GetDWFontWeigth(this.fontWeigth), this.GetDWFontStyle(this.fontStyle));
                this.TabWidthChar = this.TabWidthChar;
            }
        }

        public FontWeight FontWeigth
        {
            get
            {
                return this.fontWeigth;
            }
            set
            {
                this.fontWeigth = value;
                this.InitTextFormat(this.fontFamily.Source, (float)this.fontSize, this.GetDWFontWeigth(value), this.GetDWFontStyle(this.fontStyle));
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                return this.fontStyle;
            }
            set
            {
                this.fontStyle = value;
                this.InitTextFormat(this.fontFamily.Source, (float)this.fontSize, this.GetDWFontWeigth(this.fontWeigth), this.GetDWFontStyle(this.fontStyle));
            }
        }

        DW.FontStyle GetDWFontStyle(FontStyle style)
        {
            return (DW.FontStyle)Enum.Parse(typeof(DW.FontStyle), style.ToString());
        }

        DW.FontWeight GetDWFontWeigth(FontWeight weigth)
        {
            if (weigth.Weight == 0)
                return (DW.FontWeight)400;
            else
                return (DW.FontWeight)weigth.Weight;
        }

        public static Color4 ToColor4(Windows.UI.Color color)
        {
            return new Color4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }

        public override void CacheContent()
        {
        }

        public override void DrawCachedBitmap(Rectangle rect)
        {
        }

        public override bool IsVaildCache()
        {
            return false;
        }

        public void DrawOneLine(Document doc, LineToIndexTable lti, int row, double x, double y)
        {
            this.DrawOneLine(doc,
                lti,
                row,
                x,
                y,
                null);
        }
    }
}
