﻿using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.WebPages;

namespace Satrabel.OpenContent.Components
{
    public static class TemplateHelper
    {
        static int JSOrder = (int)FileOrder.Js.DefaultPriority;
        static int CSSOrder = (int)FileOrder.Css.ModuleCss;

        public static void RegisterStyleSheet(this WebPageBase page, string filePath)
        {
            if (!filePath.StartsWith("http") && !filePath.StartsWith("/"))
                filePath = page.VirtualPath + filePath;

            ClientResourceManager.RegisterStyleSheet((Page)HttpContext.Current.CurrentHandler, filePath, CSSOrder);
            CSSOrder++;
        }

        public static void RegisterScript(this WebPageBase page, string filePath)
        {
            if (!filePath.StartsWith("http") && !filePath.StartsWith("/"))
                filePath = page.VirtualPath + filePath;

            ClientResourceManager.RegisterScript((Page)HttpContext.Current.CurrentHandler, filePath, JSOrder);
            JSOrder++;
        }


        #region NormalizeDynamic
        /// <summary>
        /// Normalizes a setting from a Alpaca form field
        /// </summary>
        /// <para>
        /// An Alpaca field that has been defined as Number and is not filled in (has no value)
        /// will return a dynamic null value that is seen as a int with value null (not a int? but a real int with value null). 
        /// C# manual and Resharper says an int can never be Null. But alpaca forms manages to give us a int that is null
        /// That is very strange and akward and needs to be normalized.
        /// </para>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static int NormalizeDynamic(dynamic value, int defaultValue)
        {
            if (value == null) return defaultValue;
            if (value.GetType() == 0.GetType()) return value ?? defaultValue; //Resharper says value is never Null. 

            int retVal = 0;
            if (!Int32.TryParse(value, out retVal))
            {
                retVal = defaultValue;
            }
            return retVal;
        }
        public static bool NormalizeDynamic(dynamic value, bool defaultValue)
        {
            if (value == null) return defaultValue;
            if (value.GetType() == true.GetType()) return value ?? defaultValue; //Resharper says value is never Null. 

            bool retVal;
            if (!bool.TryParse(value, out retVal))
            {
                retVal = defaultValue;
            }
            return retVal;
        }
        public static string NormalizeDynamic(dynamic value, string defaultValue)
        {
            if (value == null) return defaultValue;
            if (value.GetType() == "".GetType()) return value ?? defaultValue; //Resharper says value is never Null. 
            return value.ToString();
        }
        #endregion

    }
}