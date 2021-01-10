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
using DXGI = SharpDX.DXGI;
using D2D = SharpDX.Direct2D1;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using FooEditEngine.UWP;
using Windows.UI.Text.Core;
using Windows.UI.Text;

namespace FooEditEngine
{
    sealed class D2DRender : D2DRenderBase, IEditorRender
    {
        SurfaceImageSource SurfaceImage;
        DXGI.ISurfaceImageSourceNative SurfaceImageNative;
        D2D.Bitmap1 Bitmap;
        Size Size = new Size();
        DXGI.Surface Surface;

        //変換中の書式

        public D2DRender(FooTextBox textbox, Windows.UI.Xaml.Shapes.Rectangle rect)
            : base()
        {
            this.ConstructRenderAndResoruce(rect.ActualWidth, rect.ActualHeight);
            this.InitTextFormat(textbox.FontFamily, textbox.FontSize);

            this.Size.Width = rect.ActualWidth;
            this.Size.Height = rect.ActualHeight;

            this.CreateSurface(rect, rect.ActualWidth, rect.ActualHeight);

            this.Foreground = D2DRenderBase.ToColor4(textbox.Foreground);
            this.Background = D2DRenderBase.ToColor4(textbox.Background);
            this.HilightForeground = D2DRenderBase.ToColor4(textbox.HilightForeground);
            this.Hilight = D2DRenderBase.ToColor4(textbox.Hilight);
            this.Keyword1 = D2DRenderBase.ToColor4(textbox.Keyword1);
            this.Keyword2 = D2DRenderBase.ToColor4(textbox.Keyword2);
            this.Literal = D2DRenderBase.ToColor4(textbox.Literal);
            this.Url = D2DRenderBase.ToColor4(textbox.URL);
            this.ControlChar = D2DRenderBase.ToColor4(textbox.ControlChar);
            this.Comment = D2DRenderBase.ToColor4(textbox.Comment);
            this.InsertCaret = D2DRenderBase.ToColor4(textbox.InsertCaret);
            this.OverwriteCaret = D2DRenderBase.ToColor4(textbox.OverwriteCaret);
            this.LineMarker = D2DRenderBase.ToColor4(textbox.LineMarker);
            this.UpdateArea = D2DRenderBase.ToColor4(textbox.UpdateArea);
            this.LineNumber = D2DRenderBase.ToColor4(textbox.LineNumber);
        }

        public override void GetDpi(out float dpix, out float dpiy)
        {
            Util.GetDpi(out dpix, out dpiy);
        }

        public bool Resize(Windows.UI.Xaml.Shapes.Rectangle rect, double width, double height)
        {
            if (this.Size.Width == width && this.Size.Height == height)
                return false;
            this.ReConstructRenderAndResource(width, height);
            this.CreateSurface(rect, width, height);
            return true;
        }

        public bool IsCanDraw()
        {
            return this.Size.Height != 0 && this.Size.Width != 0;
        }

        public void DrawContent(EditView view,bool IsEnabled,Rectangle updateRect)
        {
            SharpDX.Mathematics.Interop.RawPoint offset;
            //デバイス依存の座標を渡さないといけない
            this.Surface = this.SurfaceImageNative.BeginDraw(
                new SharpDX.Rectangle(0, 0, (int)(this.Size.Width * this.GetScale()), (int)(this.Size.Height * this.GetScale())), out offset);
            this.Bitmap = new D2D.Bitmap1(this.D2DContext, this.Surface, null);
            this.D2DContext.Target = this.Bitmap;
            //こっちはDirect2Dなのでデバイス非依存の座標を渡す
            float dipOffsetX = (float)(offset.X / this.GetScale());
            float dipOffsetY = (float)(offset.Y / this.GetScale());
            this.D2DContext.Transform = Matrix3x2.Translation(dipOffsetX, dipOffsetY);
            base.BegineDraw();

            if (IsEnabled)
                view.Draw(updateRect);
            else
                this.FillBackground(updateRect);

            base.EndDraw();
            this.Surface.Dispose();
            this.Bitmap.Dispose();
            this.SurfaceImageNative.EndDraw();
        }

        void CreateSurface(Windows.UI.Xaml.Shapes.Rectangle rect, double width, double height)
        {
            if (this.SurfaceImageNative != null)
                this.SurfaceImageNative.Dispose();
            //デバイス依存の座標を渡さないといけない
            this.SurfaceImage = new SurfaceImageSource((int)(width * this.GetScale()), (int)(height * this.GetScale()), false);
            this.SurfaceImageNative = ComObject.As<DXGI.ISurfaceImageSourceNative>(this.SurfaceImage);
            this.SurfaceImageNative.Device = this.DXGIDevice;
            this.Size.Width = width;
            this.Size.Height = height;
            ImageBrush brush = new ImageBrush();
            brush.ImageSource = this.SurfaceImage;
            rect.Fill = brush;
        }

        public void ReConstructRenderAndResource(double width, double height)
        {
            this.DestructRenderAndResource();
            this.ConstructRenderAndResoruce(width, height);
        }
    }
}
