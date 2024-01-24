﻿using EasePass.Models;
using EasePass.Settings;
using Microsoft.Windows.ApplicationModel.Resources;
using System.Collections.Generic;
using System.Diagnostics;

namespace EasePass.Helper;

public class LocalizationHelper
{
    private ResourceManager resourceManager = new();
    public ResourceContext resourceContext;
    private LanguageItem currentLanguageItem = null;
    public ResourceMap resourceMap = null;

    public List<LanguageItem> languages = new();

    public LocalizationHelper()
    {
        resourceContext = resourceManager.CreateResourceContext();
        resourceMap = new ResourceManager().MainResourceMap;

    }

    public void SetLanguage(LanguageItem languageItem)
    {
        if (languages.Contains(languageItem))
        {
            currentLanguageItem = languageItem;
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = languageItem.Tag;

            AppSettings.SaveSettings(AppSettingsValues.language, languageItem.Tag);
        }
    }

    public void Initialize()
    {
        RegisterLanguageFromResource();

        var languageTag = AppSettings.GetSettings(AppSettingsValues.language, "en-US");
        var res = languages.Find(x => x.Tag == languageTag);
        if (res == null)
            return;

        SetLanguage(languages.Find(x => x.Tag == "de-DE"));
        //SetLanguage(res);
    }

    private void RegisterLanguageFromResource()
    {
        ResourceMap resourceMap = new ResourceManager().MainResourceMap.GetSubtree("LanguageList");
        for(uint i = 0; i<resourceMap.ResourceCount; i++)
        {
            var resource = resourceMap.GetValueByIndex(i);
            languages.Add(new LanguageItem(resource.Key, resource.Value.ValueAsString));
        }
    }
}
