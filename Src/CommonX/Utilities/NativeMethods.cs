using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace CommonX.Utilities
{
    /// <summary>
    /// Platform Invocation methods used to support Tracer.
    /// 
    /// </summary>
    internal sealed class NativeMethods
    {
        internal const uint OWNER_SECURITY_INFORMATION = 1U;
        internal const uint GROUP_SECURITY_INFORMATION = 2U;
        internal const uint DACL_SECURITY_INFORMATION = 4U;
        internal const uint SACL_SECURITY_INFORMATION = 8U;
        internal const int CONTEXT_E_NOCONTEXT = -2147164156;
        internal const int E_NOINTERFACE = -2147467262;

        private NativeMethods()
        {
        }

        [DllImport("kernel32.dll")]
        internal static extern int QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll")]
        internal static extern int QueryPerformanceFrequency(out long lpPerformanceCount);

        [DllImport("mtxex.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetObjectContext([MarshalAs(UnmanagedType.Interface)] out NativeMethods.IObjectContext pCtx);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetCurrentProcess();

        /// <summary>
        /// Made public for testing purposes.
        /// 
        /// </summary>
        /// 
        /// <returns/>
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentProcessId();

        /// <summary>
        /// Made public for testing purposes.
        /// 
        /// </summary>
        /// 
        /// <returns/>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetModuleFileName([In] IntPtr hModule, [Out] StringBuilder lpFilename, [MarshalAs(UnmanagedType.U4), In] int nSize);

        /// <summary>
        /// Made public for testing purposes.
        /// 
        /// </summary>
        /// 
        /// <returns/>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("secur32.dll", EntryPoint = "GetUserNameExW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool GetUserNameEx([In] NativeMethods.ExtendedNameFormat nameFormat, StringBuilder nameBuffer, ref uint size);

        [DllImport("advapi32.dll")]
        internal static extern int GetSecurityInfo(IntPtr handle, NativeMethods.SE_OBJECT_TYPE objectType, uint securityInformation, ref IntPtr ppSidOwner, ref IntPtr ppSidGroup, ref IntPtr ppDacl, ref IntPtr ppSacl, out IntPtr ppSecurityDescriptor);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern bool LookupAccountSid(IntPtr systemName, IntPtr sid, StringBuilder accountName, ref uint accountNameLength, StringBuilder domainName, ref uint domainNameLength, out int sidType);

        /// <summary>
        /// Made public for testing purposes.
        /// 
        /// </summary>
        /// 
        /// <returns/>
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        [Guid("51372AE0-CAE7-11CF-BE81-00AA00A2FA25")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        internal interface IObjectContext
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            object CreateInstance([MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, [MarshalAs(UnmanagedType.LPStruct)] Guid riid);

            void SetComplete();

            void SetAbort();

            void EnableCommit();

            void DisableCommit();

            [MethodImpl(MethodImplOptions.PreserveSig)]
            [return: MarshalAs(UnmanagedType.Bool)]
            bool IsInTransaction();

            [MethodImpl(MethodImplOptions.PreserveSig)]
            [return: MarshalAs(UnmanagedType.Bool)]
            bool IsSecurityEnabled();

            [return: MarshalAs(UnmanagedType.Bool)]
            bool IsCallerInRole([MarshalAs(UnmanagedType.BStr), In] string role);
        }

        internal enum ExtendedNameFormat
        {
            NameUnknown = 0,
            NameFullyQualifiedDN = 1,
            NameSamCompatible = 2,
            NameDisplay = 3,
            NameUniqueId = 6,
            NameCanonical = 7,
            NameUserPrincipal = 8,
            NameCanonicalEx = 9,
            NameServicePrincipal = 10,
            NameDnsDomain = 12,
        }

        internal enum SE_OBJECT_TYPE
        {
            SE_UNKNOWN_OBJECT_TYPE,
            SE_FILE_OBJECT,
            SE_SERVICE,
            SE_PRINTER,
            SE_REGISTRY_KEY,
            SE_LMSHARE,
            SE_KERNEL_OBJECT,
            SE_WINDOW_OBJECT,
            SE_DS_OBJECT,
            SE_DS_OBJECT_ALL,
            SE_PROVIDER_DEFINED_OBJECT,
            SE_WMIGUID_OBJECT,
        }
    }
}
