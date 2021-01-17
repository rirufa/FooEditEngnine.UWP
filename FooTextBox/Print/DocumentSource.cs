/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using Windows.Graphics.Display;
using Windows.Graphics.Printing;
using System.Runtime.InteropServices;
using Windows.UI;
using FooEditEngine.UWP;

namespace FooEditEngine.UWP
{
    /// <summary>
    /// イベントデータ
    /// </summary>
    public sealed class ParseCommandEventArgs
    {
        /// <summary>
        /// 印刷中のページ番号
        /// </summary>
        public int PageNumber;
        /// <summary>
        /// ページ範囲内で許容されている最大の番号
        /// </summary>
        public int MaxPageNumber;
        /// <summary>
        /// 処理前の文字列
        /// </summary>
        public string Original;
        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="nowPage">印刷中のページ番号</param>
        /// <param name="maxPage">印刷すべき最大のページ番号</param>
        /// <param name="org">処理前の文字列</param>
        public ParseCommandEventArgs(int nowPage, int maxPage, string org)
        {
            this.PageNumber = nowPage;
            this.MaxPageNumber = maxPage;
            this.Original = org;
        }
    }

    /// <summary>
    /// コマンド処理用デリゲート
    /// </summary>
    /// <param name="sender">送信元のクラス</param>
    /// <param name="e">イベントデータ</param>
    /// <returns>処理後の文字列</returns>
    public delegate string ParseCommandHandler(object sender, ParseCommandEventArgs e);


    sealed class PrintableViewFactory
    {
        public Windows.UI.Color foreground, comment, keyword1, keyword2, literal, url;

        string fontName;
        double fontSize;
        float displayDpi;
        Padding padding;

        public PrintableViewFactory(Padding padding, string FontName,double fontSize)
        {
            this.fontName = FontName;
            this.fontSize = fontSize;
            this.displayDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
            this.padding = padding;
        }
        public D2DPrintRender CreateRender(PrintPageDescription pagedesc, IPrintDocumentPackageTarget docPackageTarget)
        {
            D2DPrintRender render;
            Size size = new Size(pagedesc.ImageableRect.Width, pagedesc.ImageableRect.Height);
            render = new D2DPrintRender(this.fontName, this.fontSize, size, Math.Min(pagedesc.DpiX, pagedesc.DpiY), docPackageTarget);
            render.Foreground = D2DRenderBase.ToColor4(this.foreground);
            render.Comment = D2DRenderBase.ToColor4(this.comment);
            render.Keyword1 = D2DRenderBase.ToColor4(this.keyword1);
            render.Keyword2 = D2DRenderBase.ToColor4(this.keyword2);
            render.Literal = D2DRenderBase.ToColor4(this.literal);
            render.Url = D2DRenderBase.ToColor4(this.url);
            return render;
        }

        public D2DPrintPreviewRender CreateRender(PrintPageDescription pagedesc)
        {
            D2DPrintPreviewRender render;
            Size size = new Size(pagedesc.ImageableRect.Width, pagedesc.ImageableRect.Height);
            render = new D2DPrintPreviewRender(this.fontName, this.fontSize, size, this.displayDpi);
            render.Foreground = D2DRenderBase.ToColor4(this.foreground);
            render.Comment = D2DRenderBase.ToColor4(this.comment);
            render.Keyword1 = D2DRenderBase.ToColor4(this.keyword1);
            render.Keyword2 = D2DRenderBase.ToColor4(this.keyword2);
            render.Literal = D2DRenderBase.ToColor4(this.literal);
            render.Url = D2DRenderBase.ToColor4(this.url);
            return render;
        }

        public PrintableView CreateView(Document document,PrintPageDescription pagedesc, IPrintableTextRender render, string header, string footer)
        {
            document.LayoutLines.Render = render;
            PrintableView view = new PrintableView(document, render,padding);
            view.Header = header;
            view.Footer = footer;
            view.PageBound = new Rectangle(pagedesc.ImageableRect.X, pagedesc.ImageableRect.Y, pagedesc.ImageableRect.Width, pagedesc.ImageableRect.Height);
            document.PerformLayout(false);

            return view;
        }

    }

    /// <summary>
    /// 印刷用ソース
    /// </summary>
    public sealed class DocumentSource : IPrintDocumentPageSource, IPrintPreviewPageCollection, IPrintDocumentSource,IPrintPreviewSource
    {
        IPrintPreviewDxgiPackageTarget dxgiPreviewTarget;
        bool paginateCalled = false;
        Size imageRect;
        PrintableViewFactory factory;
        D2DPrintPreviewRender previewRender;
        PrintableView previewView;
        int maxPreviePageCount;
        Document doc;
        IHilighter hilighter;

        public ParseCommandHandler ParseHF;
        public string Header = string.Empty;
        public string Fotter = string.Empty;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="textbox"></param>
        /// <param name="padding"></param>
        /// <param name="fontName"></param>
        /// <param name="fontSize"></param>
        public DocumentSource(Document doc,Padding padding, string fontName, double fontSize)
        {
            this.factory = new PrintableViewFactory(padding, fontName, fontSize);
            this.doc = new Document(doc);
            this.hilighter = doc.LayoutLines.Hilighter;
        }

        public enum SyntaxHilightApplibility
        {
            Apply,
            NoApply,
        }

        public Windows.UI.Color Forground
        {
            get
            {
                return this.factory.foreground;
            }
            set
            {
                this.factory.foreground = value;
            }
        }

        public Windows.UI.Color Comment
        {
            get
            {
                return this.factory.comment;
            }
            set
            {
                this.factory.comment = value;
            }
        }

        public Windows.UI.Color Keyword1
        {
            get
            {
                return this.factory.keyword1;
            }
            set
            {
                this.factory.keyword1 = value;
            }
        }

        public Windows.UI.Color Keyword2
        {
            get
            {
                return this.factory.keyword2;
            }
            set
            {
                this.factory.keyword2 = value;
            }
        }

        public Windows.UI.Color Literal
        {
            get
            {
                return this.factory.literal;
            }
            set
            {
                this.factory.literal = value;
            }
        }

        public Windows.UI.Color Url
        {
            get
            {
                return this.factory.url;
            }
            set
            {
                this.factory.url = value;
            }
        }

        public LineBreakMethod LineBreak
        {
            get;
            set;
        }

        public int LineBreakCount
        {
            get;
            set;
        }

        [DisplayPrintOptionResourceID("SyntaxHilight")]
        public SyntaxHilightApplibility EnableHilight
        {
            get;
            set;
        }

        public enum LineNumberVisiblity
        {
            Visible,
            Hidden
        }

        [DisplayPrintOptionResourceID("ShowLineNumber")]
        public LineNumberVisiblity ShowLineNumber
        {
            get;
            set;
        }

        public void GetPreviewPageCollection(IPrintDocumentPackageTarget docPackageTarget, out IPrintPreviewPageCollection docPageCollection)
        {
            Guid guid = new Guid(PreviewPackageIds.IID_PREVIEWPACKAGETARGET_DXGI);
            IntPtr target;
            docPackageTarget.GetPackageTarget(guid, guid, out target);
            this.dxgiPreviewTarget = (IPrintPreviewDxgiPackageTarget)Marshal.GetObjectForIUnknown(target);
            docPageCollection = (IPrintPreviewPageCollection)this;
        }

        public void MakeDocument(object printTaskOptions, IPrintDocumentPackageTarget docPackageTarget)
        {
            PrintTaskOptions options = (PrintTaskOptions)printTaskOptions;
            PrintPageDescription pagedesc = options.GetPageDescription(1);

            D2DPrintRender render = this.factory.CreateRender(pagedesc, docPackageTarget);
            this.doc.DrawLineNumber = this.ShowLineNumber == LineNumberVisiblity.Visible;
            this.doc.LayoutLines.Hilighter = this.EnableHilight == SyntaxHilightApplibility.Apply ? this.hilighter : null;
            PrintableView view = this.factory.CreateView(this.doc, pagedesc, render, this.Header, this.Fotter);
            this.doc.LineBreak = this.LineBreak;
            this.doc.LineBreakCharCount = this.LineBreakCount;

            bool result = false;
            int currentPage = 0;

            while (!result)
            {
                if(!string.IsNullOrEmpty(this.Header))
                    view.Header = this.ParseHF(this, new ParseCommandEventArgs(currentPage, this.maxPreviePageCount, this.Header));
                if (!string.IsNullOrEmpty(this.Fotter))
                    view.Footer = this.ParseHF(this, new ParseCommandEventArgs(currentPage, this.maxPreviePageCount, this.Fotter));

                render.DrawContent(view);

                result = view.TryPageDown();
                currentPage++;
            }

            render.Dispose();
            view.Dispose();
        }

        public void Paginate(uint currentJobPage, object printTaskOptions)
        {
            PrintTaskOptions options = (PrintTaskOptions)printTaskOptions;
            PrintPageDescription pagedesc = options.GetPageDescription(currentJobPage);

            this.imageRect = new Size(pagedesc.ImageableRect.Width, pagedesc.ImageableRect.Height);

            //何度か呼ばれることがある
            if (this.previewView != null)
                this.previewView.Dispose();
            if (this.previewRender != null)
                this.previewRender.Dispose();

            this.previewRender = this.factory.CreateRender(pagedesc);
            this.doc.DrawLineNumber = this.ShowLineNumber == LineNumberVisiblity.Visible;
            this.doc.LayoutLines.Hilighter = this.EnableHilight == SyntaxHilightApplibility.Apply ? this.hilighter : null;
            this.previewView = this.factory.CreateView(this.doc, pagedesc, this.previewRender, this.Header, this.Fotter);
            this.doc.LineBreak = this.LineBreak;
            this.doc.LineBreakCharCount = this.LineBreakCount;

            int maxPage = 1;
            while (!this.previewView.TryPageDown())
                maxPage++;

            this.dxgiPreviewTarget.SetJobPageCount(PageCountType.FinalPageCount, (uint)maxPage);

            this.dxgiPreviewTarget.InvalidatePreview();

            this.maxPreviePageCount = maxPage;

            this.paginateCalled = true;
        }

        public void InvalidatePreview()
        {
            this.dxgiPreviewTarget.InvalidatePreview();
        }

        public void MakePage(uint desiredJobPage, float width, float height)
        {
            if (width <= 0 || height <= 0)
                throw new COMException("", 0x70057/*E_INVALIDARG*/);
            if (!this.paginateCalled)
                return;
            if (desiredJobPage == 0xFFFFFFFF)
                desiredJobPage = 1;

            this.previewView.TryScroll(0, 0);   //元に戻さないとページ番号が変わった時に正しく動作しない

            for (int i = 1; i < desiredJobPage; i++)
                this.previewView.TryPageDown();

            if (!string.IsNullOrEmpty(this.Header))
                this.previewView.Header = this.ParseHF(this, new ParseCommandEventArgs((int)desiredJobPage, this.maxPreviePageCount, this.Header));
            if (!string.IsNullOrEmpty(this.Fotter))
                this.previewView.Footer = this.ParseHF(this, new ParseCommandEventArgs((int)desiredJobPage, this.maxPreviePageCount, this.Fotter));

            this.previewRender.Resize(width, height);
            this.previewRender.SetScale((float)(this.previewView.PageBound.Width / width)); //BeginDraw()で倍率が1に戻る

            this.previewRender.DrawContent(this.previewView, this.dxgiPreviewTarget, desiredJobPage);
        }

        public void Dispose()
        {
            if (this.previewView != null)
                this.previewView.Dispose();
            if (this.previewRender != null)
                this.previewRender.Dispose();
            if (this.doc != null)
                this.doc.Dispose();
            Marshal.ReleaseComObject(this.dxgiPreviewTarget);
        }
    }
}
