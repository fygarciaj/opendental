﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OpenDentBusiness.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("OpenDentBusiness.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to The primary key could not be overriden, because either the object is not new and has been loaded from the database or the object does not contain an Identity key..
        /// </summary>
        internal static string CannotOverridePrimaryKey {
            get {
                return ResourceManager.GetString("CannotOverridePrimaryKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The object has been deleted, and can not be saved again to the database..
        /// </summary>
        internal static string CannotSaveDeletedObject {
            get {
                return ResourceManager.GetString("CannotSaveDeletedObject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The current data type is not supported by the PIn class..
        /// </summary>
        internal static string DataTypeNotSupportedByPIn {
            get {
                return ResourceManager.GetString("DataTypeNotSupportedByPIn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The current data type ({0}) is not supported by the POut class..
        /// </summary>
        internal static string DataTypeNotSupportedByPOut {
            get {
                return ResourceManager.GetString("DataTypeNotSupportedByPOut", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A DTO of type {0} is not supported..
        /// </summary>
        internal static string DtoNotSupportedException {
            get {
                return ResourceManager.GetString("DtoNotSupportedException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The current object does not contain any data fields..
        /// </summary>
        internal static string NoFields {
            get {
                return ResourceManager.GetString("NoFields", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This object has a primary key that consists of multiple columns..
        /// </summary>
        internal static string NotASinglePrimaryKey {
            get {
                return ResourceManager.GetString("NotASinglePrimaryKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The object has already been deleted, and cannot be deleted twice..
        /// </summary>
        internal static string ObjectAlreadyDeleted {
            get {
                return ResourceManager.GetString("ObjectAlreadyDeleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The object does not exist in the database, and can therefore not be deleted..
        /// </summary>
        internal static string ObjectNotSaved {
            get {
                return ResourceManager.GetString("ObjectNotSaved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The primary key is not of the type Integer..
        /// </summary>
        internal static string PrimaryKeyNotAnInteger {
            get {
                return ResourceManager.GetString("PrimaryKeyNotAnInteger", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An attribute of type &quot;DataFieldAttribute&quot; can only be specified once..
        /// </summary>
        internal static string TooManyDataFieldAttributes {
            get {
                return ResourceManager.GetString("TooManyDataFieldAttributes", resourceCulture);
            }
        }
    }
}
