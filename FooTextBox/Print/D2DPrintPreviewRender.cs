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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SharpDX;
using SharpDX.WIC;
using DXGI = SharpDX.DXGI;
using D2D = SharpDX.Direct2D1;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using FooEditEngine.UWP;

namespace FooEditEngine
{
    sealed class D2DPrintPreviewRender : D2DRenderBase, ITextRender, IPrintableTextRender
    {
        DXGI.Surface Surface;
        D3D11.Texture2D Texture;
        D2D.Bitmap1 Bitmap;
        Size Size;
        float dpi;

        public D2DPrintPreviewRender(string fontName, double fontSize, Size size,float dpi)
            : base()
        {
            this.Size = size;
            this.dpi = dpi;

            base.ConstructRenderAndResoruce(size.Width, size.Height);
            base.InitTextFormat(fontName, (float)fontSize);
        }

        public override void GetDpi(out float dpix, out float dpiy)
        {
            dpix = this.dpi;
            dpiy = this.dpi;
        }

        public void DrawContent(PrintableView view, IPrintPreviewDxgiPackageTarget target, uint page)
        {
            base.BegineDraw();
            view.Draw(view.PageBound);
            base.EndDraw();
            target.DrawPage(page, this.Surface.NativePointer, this.dpi, this.dpi);
        }

        public void Resize(float width,float height)
        {
            this.DestructRenderAndResource();
            this.ConstructRenderAndResoruce(width, height);
        }

        public void SetScale(float scale)
        {
            this.D2DContext.Transform = Matrix3x2.Scaling(1/scale);
        }

        public override void ConstructRenderAndResoruce(double width, double height)
        {
            D3D11.Texture2DDescription desc = new D3D11.Texture2DDescription();
            desc.Width = (int)(width * this.dpi / 96);
            desc.Height = (int)(height * this.dpi / 96);
            desc.Format = DXGI.Format.B8G8R8A8_UNorm;
            desc.BindFlags = D3D11.BindFlags.RenderTarget | D3D11.BindFlags.ShaderResource;
            desc.ArraySize = 1;
            desc.MipLevels = 1;
            desc.Usage = D3D11.ResourceUsage.Default;
            desc.CpuAccessFlags = 0;
            desc.SampleDescription = new DXGI.SampleDescription(1, 0);
            this.Texture = new D3D11.Texture2D(base.D3DDevice, desc);

            this.Surface = this.Texture.QueryInterface<DXGI.Surface>();

            base.ConstructRenderAndResoruce(width, height);

            D2D.BitmapProperties1 bmpProp = new D2D.BitmapProperties1();
            bmpProp.BitmapOptions = D2D.BitmapOptions.Target | D2D.BitmapOptions.CannotDraw;
            bmpProp.PixelFormat = new D2D.PixelFormat(DXGI.Format.B8G8R8A8_UNorm, D2D.AlphaMode.Premultiplied);

            this.Bitmap = new D2D.Bitmap1(this.D2DContext, this.Surface, bmpProp);

            this.D2DContext.Target = this.Bitmap;
            this.D2DContext.DotsPerInch = new Size2F(this.dpi, this.dpi);
        }

        protected override void Dispose(bool dispose)
        {
            if (this.Bitmap != null)
                this.Bitmap.Dispose();
            base.Dispose(dispose);
            if (this.Surface != null)
                this.Surface.Dispose();
            if (this.Texture != null)
                this.Texture.Dispose();
        }

        public float HeaderHeight
        {
            get { return (float)this.emSize.Height; }
        }

        public float FooterHeight
        {
            get { return (float)this.emSize.Height; }
        }
    }
}
