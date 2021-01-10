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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Runtime.InteropServices;

namespace FooEditEngine
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("1b8efec4-3019-4c27-964e-367202156906")]
    [SecurityCritical]
    public interface IPrintDocumentPackageTarget
    {
        void GetPackageTargetTypes([Out] out UInt32 targetCount, [Out]　out Guid targetTypes);
        void GetPackageTarget([In]ref Guid guidTargetType,[In]ref Guid riid,[Out] out IntPtr ppvTarget);
        void Cancel();
        
    };

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0b31cc62-d7ec-4747-9d6e-f2537d870f2b")]
    [SecurityCritical]
    public interface IPrintPreviewPageCollection
    {
        void Paginate(
            [In] UInt32 currentJobPage,
            [In, MarshalAs(UnmanagedType.Interface)] object printTaskOptions);
        void MakePage(
            [In] UInt32 desiredJobPage,
            [In] float width,
            [In]float height);
        
    };

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("a96bb1db-172e-4667-82b5-ad97a252318f")]
    [SecurityCritical]
    public interface IPrintDocumentPageSource
    {
        void GetPreviewPageCollection([In, MarshalAs(UnmanagedType.Interface)]IPrintDocumentPackageTarget docPackageTarget, [Out, MarshalAs(UnmanagedType.Interface)] out IPrintPreviewPageCollection docPageCollection);
        void MakeDocument([In, MarshalAs(UnmanagedType.Interface)] object printTaskOptions, [In, MarshalAs(UnmanagedType.Interface)]IPrintDocumentPackageTarget docPackageTarget);
    }

    public enum PageCountType
    {
        FinalPageCount	= 0,
        IntermediatePageCount	= 1 
    };

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("1a6dd0ad-1e2a-4e99-a5ba-91f17818290e")]  //IID_PREVIEWPACKAGETARGET_DXGIと同じ
    [SecurityCritical]
    public interface IPrintPreviewDxgiPackageTarget
    {
        void SetJobPageCount( 
            [In] PageCountType countType,
            UInt32 count);

        void DrawPage( 
            [In]UInt32 jobPageNumber,
            [In] IntPtr dxgiSurface,
            float dpiX,
            float dpiY);
        
        void InvalidatePreview();
        
    };

    public static class PreviewPackageIds
    {
        public static string IID_PREVIEWPACKAGETARGET_DXGI = "1a6dd0ad-1e2a-4e99-a5ba-91f17818290e";
    }
}
