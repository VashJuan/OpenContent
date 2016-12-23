﻿using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.TemplateHelpers;
using System.Web;

namespace Satrabel.OpenContent.Components
{
    public class ModelFactory
    {
        private JToken _dataJson;
        private readonly IDataItem _data;
        private readonly IEnumerable<IDataItem> _dataList = null;
        private readonly string _settingsJson;
        private readonly string _physicalTemplateFolder;
        private readonly Manifest.Manifest _manifest;
        private readonly TemplateManifest _templateManifest;
        private readonly TemplateFiles _templateFiles;
        private readonly OpenContentModuleInfo _module;
        private readonly PortalSettings _portalSettings;
        private readonly int _portalId;
        private readonly int _detailTabId;
        private readonly string _cultureCode;

        public JObject Options { get; set; } // alpaca options.json format

        public ModelFactory(JToken dataJson, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, PortalSettings portalSettings)
        {
            this._dataJson = dataJson;
            this._settingsJson = settingsJson;
            this._physicalTemplateFolder = physicalTemplateFolder;
            this._manifest = manifest;
            this._templateFiles = templateFiles;
            this._module = module;
            this._portalSettings = portalSettings;
            this._portalId = portalSettings.PortalId;
            this._templateManifest = templateManifest;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
        }

        public ModelFactory(IDataItem data, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, PortalSettings portalSettings)
        {
            this._dataJson = data.Data;
            this._data = data;
            this._settingsJson = settingsJson;
            this._physicalTemplateFolder = physicalTemplateFolder;
            this._manifest = manifest;
            this._templateFiles = templateFiles;
            this._module = module;
            this._portalSettings = portalSettings;
            this._portalId = portalSettings.PortalId;
            this._templateManifest = templateManifest;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
        }
        public ModelFactory(IDataItem data, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, int portalId, string cultureCode, int mainTabId, int mainModuleId)
        {
            this._dataJson = data.Data;
            this._data = data;
            this._settingsJson = settingsJson;
            this._physicalTemplateFolder = physicalTemplateFolder;
            this._manifest = manifest;
            this._templateFiles = templateFiles;
            this._module = module;
            this._portalId = portalId;
            this._cultureCode = cultureCode;
            this._templateManifest = templateManifest;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
        }

        public ModelFactory(IEnumerable<IDataItem> dataList, OpenContentModuleInfo module, PortalSettings portalSettings)
        {
            OpenContentSettings settings = module.Settings;
            this._dataList = dataList;
            this._settingsJson = settings.Data;
            this._physicalTemplateFolder = settings.Template.ManifestFolderUri.PhysicalFullDirectory + "\\";
            this._manifest = settings.Template.Manifest;
            this._templateFiles = settings.Template != null ? settings.Template.Main : null;
            this._module = module;
            this._portalSettings = portalSettings;
            this._portalId = portalSettings.PortalId;
            this._templateManifest = settings.Template;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
        }

        public ModelFactory(IEnumerable<IDataItem> dataList, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, PortalSettings portalSettings)
        {
            this._dataList = dataList;
            this._settingsJson = settingsJson;
            this._physicalTemplateFolder = physicalTemplateFolder;
            this._manifest = manifest;
            this._templateFiles = templateFiles;
            this._module = module;
            this._portalSettings = portalSettings;
            this._portalId = portalSettings.PortalId;
            this._templateManifest = templateManifest;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
        }


        public ModelFactory(IEnumerable<IDataItem> dataList, string settingsJson, string physicalTemplateFolder, Manifest.Manifest manifest, TemplateManifest templateManifest, TemplateFiles templateFiles, OpenContentModuleInfo module, int portalId, string cultureCode)
        {
            this._dataList = dataList;
            this._settingsJson = settingsJson;
            this._physicalTemplateFolder = physicalTemplateFolder;
            this._manifest = manifest;
            this._templateFiles = templateFiles;
            this._module = module;
            this._portalId = portalId;
            this._cultureCode = cultureCode;
            this._templateManifest = templateManifest;
            this._detailTabId = DnnUtils.GetTabByCurrentCulture(this._portalId, module.GetDetailTabId(), GetCurrentCultureCode());
        }

        public dynamic GetModelAsDynamic(bool onlyData = false)
        {
            if (_portalSettings == null) onlyData = true;

            JToken model = GetModelAsJson(onlyData);
            return JsonUtils.JsonToDynamic(model.ToString());
        }

        /// <summary>
        /// Gets the model as dynamic list, used by Url Rewriter
        /// </summary>
        /// <returns></returns>
        public IEnumerable<dynamic> GetModelAsDynamicList()
        {
            var completeModel = new JObject();
            ExtendModel(completeModel, true);
            if (_dataList != null)
            {
                foreach (var item in _dataList)
                {
                    var model = item.Data as JObject;
                    if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(model, GetCurrentCultureCode());
                    }
                    EnhanceSelect2(model, completeModel);

                    JObject context = new JObject();
                    model["Context"] = context;
                    context["Id"] = item.Id;
                    yield return JsonUtils.JsonToDynamic(model.ToString());
                }
            }
        }

        public JToken GetModelAsJson(bool onlyData = false)
        {
            if (_portalSettings == null) onlyData = true;

            if (_dataList == null)
            {
                return GetModelAsJsonFromJson(onlyData);
            }
            else
            {
                return GetModelAsJsonFromList(onlyData);
            }
        }

        private JToken GetModelAsJsonFromList(bool onlyData)
        {
            JObject model = new JObject();
            if (!onlyData)
            {
                ExtendModel(model, onlyData);
                model["Context"]["RssUrl"] = _portalSettings.PortalAlias.HTTPAlias +
                       "/DesktopModules/OpenContent/API/RssAPI/GetFeed?moduleId=" + _module.ViewModule.ModuleID + "&tabId=" + _detailTabId;

            }
            JArray items = new JArray(); ;
            model["Items"] = items;
            //string editRole = Manifest.GetEditRole();
            if (_dataList != null && _dataList.Any())
            {
                foreach (var item in _dataList)
                {
                    JObject dyn = item.Data as JObject;
                    if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(dyn, GetCurrentCultureCode());
                    }
                    EnhanceSelect2(dyn, model);

                    if (Options != null && model["Options"] != null)
                    {
                        JsonUtils.ImagesJson(dyn, Options, model["Options"] as JObject, IsEditMode);
                    }
                    JObject context = new JObject();
                    dyn["Context"] = context;
                    context["Id"] = item.Id;
                    if (onlyData)
                    {
                        if (model["Settings"] != null)
                        {
                            model.Remove("Settings");
                        }
                        if (model["Schema"] != null)
                        {
                            model.Remove("Schema");
                        }
                        if (model["Options"] != null)
                        {
                            model.Remove("Options");
                        }
                        if (model["AdditionalData"] != null)
                        {
                            model.Remove("AdditionalData");
                        }
                    }
                    else
                    {
                        string url = "";
                        if (_manifest != null && !string.IsNullOrEmpty(_manifest.DetailUrl))
                        {
                            HandlebarsEngine hbEngine = new HandlebarsEngine();
                            dynamic dynForHBS = JsonUtils.JsonToDynamic(dyn.ToString());
                            url = hbEngine.Execute(_manifest.DetailUrl, dynForHBS);
                            url = HttpUtility.HtmlDecode(url);
                        }

                        var editStatus = !_manifest.DisableEdit && IsEditAllowed(item.CreatedByUserId);
                        context["IsEditable"] = editStatus;
                        context["EditUrl"] = editStatus ? DnnUrlUtils.EditUrl("id", item.Id, _module.ViewModule.ModuleID, _portalSettings) : "";
                        context["DetailUrl"] = Globals.NavigateURL(_detailTabId, false, _portalSettings, "", GetCurrentCultureCode(), url.CleanupUrl(), "id=" + item.Id);
                        context["MainUrl"] = Globals.NavigateURL(_detailTabId, false, _portalSettings, "", GetCurrentCultureCode(), "");
                    }
                    items.Add(dyn);
                }
            }
            return model;
        }

        private JToken GetModelAsJsonFromJson(bool onlyData)
        {
            var model = _dataJson as JObject;
            if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
            {
                JsonUtils.SimplifyJson(model, GetCurrentCultureCode());
            }

            var enhancedModel = new JObject();
            ExtendModel(enhancedModel, onlyData);
            EnhanceSelect2(model, enhancedModel);

            JsonUtils.Merge(model, enhancedModel);
            return model;
        }

        private void EnhanceSelect2(JObject model, JObject enhancedModel)
        {
            if (_manifest.AdditionalDataDefined() && enhancedModel["AdditionalData"] != null && enhancedModel["Options"] != null)
            {
                JsonUtils.LookupJson(model, enhancedModel["AdditionalData"] as JObject, enhancedModel["Options"] as JObject);
            }
            JsonUtils.LookupSelect2InOtherModule(model, enhancedModel["Options"] as JObject);
        }

        private void ExtendModel(JObject model, bool onlyData)
        {
            // include SCHEMA info in the Model
            if (!onlyData && _templateFiles != null && _templateFiles.SchemaInTemplate)
            {
                // schema
                string schemaFilename = _physicalTemplateFolder + "schema.json";
                model["Schema"] = JsonUtils.GetJsonFromFile(schemaFilename);
            }

            // include OPTIONS info in the Model
            if (_templateFiles != null && _templateFiles.OptionsInTemplate)
            {
                // options
                JToken optionsJson = null;
                // default options
                string optionsFilename = _physicalTemplateFolder + "options.json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(optionsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        optionsJson = fileContent.ToJObject("Options");
                    }
                }
                // language options
                optionsFilename = _physicalTemplateFolder + "options." + GetCurrentCultureCode() + ".json";
                if (File.Exists(optionsFilename))
                {
                    string fileContent = File.ReadAllText(optionsFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        var extraJson = fileContent.ToJObject("Options cultureSpecific");
                        if (optionsJson == null)
                            optionsJson = extraJson;
                        else
                            optionsJson = optionsJson.JsonMerge(extraJson);
                    }
                }
                if (optionsJson != null)
                {
                    model["Options"] = optionsJson;
                }
            }

            // include additional data in the Model
            if (_templateFiles != null && _templateFiles.AdditionalDataInTemplate && _manifest.AdditionalDataDefined())
            {
                var additionalData = model["AdditionalData"] = new JObject();
                foreach (var item in _manifest.AdditionalDataDefinition)
                {
                    var dataManifest = item.Value;
                    var ds = DataSourceManager.GetDataSource(_manifest.DataSource);
                    var dsContext = OpenContentUtils.CreateDataContext(_module);
                    IDataItem dataItem = ds.GetData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? item.Key);
                    JToken additionalDataJson = new JObject();
                    var json = dataItem?.Data;
                    if (json != null)
                    {
                        if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
                        {
                            JsonUtils.SimplifyJson(json, GetCurrentCultureCode());
                        }
                        additionalDataJson = json;
                    }
                    additionalData[(item.Value.ModelKey ?? item.Key).ToLowerInvariant()] = additionalDataJson;
                }
            }

            // include settings in the Model
            if (!onlyData && _templateManifest.SettingsNeeded() && !string.IsNullOrEmpty(_settingsJson))
            {
                try
                {
                    _dataJson = JToken.Parse(_settingsJson);
                    if (LocaleController.Instance.GetLocales(_portalId).Count > 1)
                    {
                        JsonUtils.SimplifyJson(_dataJson, GetCurrentCultureCode());
                    }
                    model["Settings"] = _dataJson;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error parsing Json of Settings", ex);
                }
            }

            // include static localization in the Model
            if (!onlyData)
            {
                JToken localizationJson = null;
                string localizationFilename = _physicalTemplateFolder + GetCurrentCultureCode() + ".json";
                if (File.Exists(localizationFilename))
                {
                    string fileContent = File.ReadAllText(localizationFilename);
                    if (!string.IsNullOrWhiteSpace(fileContent))
                    {
                        localizationJson = fileContent.ToJObject("Localization: " + localizationFilename);
                    }
                }
                if (localizationJson != null)
                {
                    model["Localization"] = localizationJson;
                }
            }

            if (!onlyData)
            {
                // include CONTEXT in the Model
                JObject context = new JObject();
                model["Context"] = context;
                context["ModuleId"] = _module.ViewModule.ModuleID;
                context["GoogleApiKey"] = OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetGoogleApiKey();
                context["ModuleTitle"] = _module.ViewModule.ModuleTitle;
                context["AddUrl"] = DnnUrlUtils.EditUrl(_module.ViewModule.ModuleID, _portalSettings);
                var editIsAllowed = !_manifest.DisableEdit && IsEditAllowed(-1);
                context["IsEditable"] = editIsAllowed; //allowed to edit the item or list (meaning allow Add)
                context["IsEditMode"] = IsEditMode;
                context["PortalId"] = _portalId;
                context["MainUrl"] = Globals.NavigateURL(_detailTabId, false, _portalSettings, "", GetCurrentCultureCode());
                if (_data != null)
                {
                    string url = "";
                    if (!string.IsNullOrEmpty(_manifest?.DetailUrl))
                    {
                        HandlebarsEngine hbEngine = new HandlebarsEngine();
                        dynamic dynForHBS = JsonUtils.JsonToDynamic(model.ToString());
                        url = hbEngine.Execute(_manifest.DetailUrl, dynForHBS);
                        url = HttpUtility.HtmlDecode(url);
                    }
                    context["DetailUrl"] = Globals.NavigateURL(_detailTabId, false, _portalSettings, "", GetCurrentCultureCode(), url.CleanupUrl(), "id=" + _data.Id);
                    context["Id"] = _data.Id;
                    context["EditUrl"] = editIsAllowed ? DnnUrlUtils.EditUrl("id", _data.Id, _module.ViewModule.ModuleID, _portalSettings) : "";
                }
            }
        }

        private bool IsEditAllowed(int createdByUser)
        {
            string editRole = _manifest.GetEditRole();
            return (IsEditMode || OpenContentUtils.HasEditRole(_portalSettings, editRole, createdByUser)) // edit Role can edit whtout be in edit mode
                    && OpenContentUtils.HasEditPermissions(_portalSettings, _module.ViewModule, editRole, createdByUser);
        }

        private string GetCurrentCultureCode()
        {
            if (string.IsNullOrEmpty(_cultureCode))
            {
                return DnnLanguageUtils.GetCurrentCultureCode();
            }
            else
            {
                return _cultureCode;
            }
        }

        private bool? _isEditMode;
        private bool IsEditMode
        {
            get
            {
                //Perform tri-state switch check to avoid having to perform a security
                //role lookup on every property access (instead caching the result)
                if (!_isEditMode.HasValue)
                {
                    _isEditMode = _module.DataModule.CheckIfEditable(PortalSettings.Current);
                }
                return _isEditMode.Value;
            }
        }
    }
}