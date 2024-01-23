﻿using EasePass.Extensions;
using EasePass.Helper;
using EasePass.Models;
using EasePass.Views;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasePass.Dialogs
{
    internal class Export2FADialog
    {
        public async Task ShowAsync(string qrcode)
        {
            var page = new Export2FAPage(qrcode);
            var dialog = new AutoLogoutContentDialog
            {
                Title = "Export 2FA".Localized("Dialog_Export2FA_Headline/Text"),
                PrimaryButtonText = "Done".Localized("Dialog_Button_Done/Text"),
                CloseButtonText = "Cancel".Localized("Dialog_Button_Cancel/Text"),
                XamlRoot = App.m_window.Content.XamlRoot,
                Content = page
            };

            await dialog.ShowAsync();
        }
    }
}
