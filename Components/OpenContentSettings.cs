﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Satrabel.OpenContent.Components.Manifest;


namespace Satrabel.OpenContent.Components
{
    public class OpenContentSettings
    {

        private JObject _json = null;

        public OpenContentSettings(Hashtable moduleSettings)
        {
            var template = moduleSettings["template"] as string;    //templatepath+file  or  //manifestpath+key
            if (!string.IsNullOrEmpty(template))
            {
                var templateUri = new FileUri(template);
                TemplateKey = new TemplateKey(templateUri);
                TemplateManifest templateManifest;
                Manifest = ManifestUtils.GetManifest(TemplateKey, out templateManifest);
                Template = templateManifest;
            }
            var sTabId = moduleSettings["tabid"] as string;
            var sModuleId = moduleSettings["moduleid"] as string;
            TabId = -1;
            ModuleId = -1;

            if (sTabId != null && sModuleId != null)
            {
                TabId = int.Parse(sTabId);
                ModuleId = int.Parse(sModuleId);
            }

            Data = moduleSettings["data"] as string;
        }

        internal TemplateKey TemplateKey { get; set; }


        public int TabId { get; set; }
        internal int ModuleId { get; set; }

        public TemplateManifest Template { get; private set; }
        public Manifest.Manifest Manifest { get; private set; }

        public FolderUri TemplateDir { get { return TemplateKey.TemplateDir; } }
        //public TemplateKey TemplateKey { get { return Template == null ? "" : Template.FileNameWithoutExtension; } }

        //internal FileUri Template { get; private set; }

        public string Data { get; private set; }
        public bool IsOtherModule
        {
            get
            {
                return TabId > 0 && ModuleId > 0;
            }
        }

        public bool TemplateAvailable { get { return TemplateKey != null; } }

        public JObject DataAsJson
        {
            get
            {
                if (_json == null)
                {
                    _json = JObject.Parse(Data);

                }
                return _json;
            }
        }

        public string LuceneFilter
        {
            get
            {
                var filter = DataAsJson["LuceneFilter"];
                if (filter != null)
                    return filter.ToString();
                else
                    return "";
            }
        }
        public string LuceneSort
        {
            get
            {
                var filter = DataAsJson["LuceneSort"];
                if (filter != null)
                    return filter.ToString();
                else
                    return "";
            }
        }

        public int? LuceneMaxResults
        {
            get
            {
                int maxResults = 0;
                var sMaxResults = DataAsJson["LuceneMaxResults"];

                if (sMaxResults != null && int.TryParse(sMaxResults.ToString(), out maxResults))
                    return maxResults;
                else
                    return null;
            }
        }
    }
}