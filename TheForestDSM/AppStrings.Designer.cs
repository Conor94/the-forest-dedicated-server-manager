﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TheForestDSM {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class AppStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AppStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TheForestDSM.AppStrings", typeof(AppStrings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to \TheForestDSM;component\Images.
        /// </summary>
        internal static string ImagesPath {
            get {
                return ResourceManager.GetString("ImagesPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The refresh interval sets how frequently this application will refresh the user interface. Examples of refreshing the user interface are disabling/enabling buttons and changing the server status.
        ///
        ///Increase this value if you&apos;re having performance issues and decrease the value to ensure the application always displays the most up-to-date information..
        /// </summary>
        internal static string RefreshIntervalDialog_Content {
            get {
                return ResourceManager.GetString("RefreshIntervalDialog_Content", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Refresh Interval Information.
        /// </summary>
        internal static string RefreshIntervalDialog_Title {
            get {
                return ResourceManager.GetString("RefreshIntervalDialog_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TheForestDSMShutdownService.
        /// </summary>
        internal static string ServiceName {
            get {
                return ResourceManager.GetString("ServiceName", resourceCulture);
            }
        }
    }
}
