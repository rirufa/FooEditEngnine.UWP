/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using SharpDX;
using SharpDX.WIC;
using D2D = SharpDX.Direct2D1;

namespace FooEditEngine
{
    sealed class D2DPrintRender : D2DRenderBase, ITextRender, IPrintableTextRender
    {
        D2D.PrintControl Control;
        D2D.CommandList CommandList;
        Size2F Size;
        ImagingFactory WICFactory;
        int dpi;
        ComObject docPackageTarget;

        public D2DPrintRender(string fontName,double fontSize, Size size, uint dpi, IPrintDocumentPackageTarget docPackageTarget)
            : base()
        {
            this.WICFactory = new ImagingFactory();
            base.ConstructRenderAndResoruce(size.Width, size.Height);
            base.InitTextFormat(fontName, (float)fontSize);

            this.docPackageTarget = new ComObject(docPackageTarget);    //あとで廃棄する必要がある

            this.CreateSurface(size,dpi,this.docPackageTarget);
        }

        public override void GetDpi(out float dpix, out float dpiy)
        {
            dpix = this.dpi;
            dpiy = this.dpi;
        }

        public void DrawContent(PrintableView view)
        {
            this.CommandList = new D2D.CommandList(this.D2DContext);
            this.D2DContext.Target = this.CommandList;
            base.BegineDraw();
            view.Draw(view.PageBound);
            base.EndDraw();
            this.CommandList.Close();
            this.Control.AddPage(this.CommandList, this.Size);
            this.CommandList.Dispose();
        }

        void CreateSurface(Size size, uint dpi, ComObject docPackageTarget)
        {
            D2D.PrintControlProperties printControlProperties = new D2D.PrintControlProperties();
            printControlProperties.RasterDPI = dpi;
            printControlProperties.ColorSpace = D2D.ColorSpace.SRgb;
            printControlProperties.FontSubset = D2D.PrintFontSubsetMode.Default;

            this.Control = new D2D.PrintControl(this.D2DDevice, this.WICFactory, docPackageTarget, printControlProperties);

            this.Size = new Size2F((float)size.Width, (float)size.Height);

            this.dpi = (int)dpi;
        }

        public override void DestructRenderAndResource()
        {
            if (this.Control != null)
            {
                this.Control.Close();
                this.Control.Dispose();
            }
            base.DestructRenderAndResource();
            if (this.docPackageTarget != null)
                this.docPackageTarget.Dispose();
            if (this.WICFactory != null)
                this.WICFactory.Dispose();
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
