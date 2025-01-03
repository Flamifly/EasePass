/*
MIT License

Copyright (c) 2023 Julius Kirsch

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
*/

using EasePass.Core.Database;
using EasePass.Extensions;
using EasePass.Helper;
using EasePass.Views;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EasePass.Dialogs
{
    internal class CreateDatabaseDialog
    {
        private CreateDatabaseDialogPage page;
        public async Task<DatabaseItem> ShowAsync()
        {
            var dialog = new AutoLogoutContentDialog
            {
                Title = "Create Database".Localized("Dialog_CreateDB_Headline/Text"),
                PrimaryButtonText = "Create".Localized("Dialog_Button_Create/Text"),
                CloseButtonText = "Close".Localized("Dialog_Button_Close/Text"),
                XamlRoot = App.m_window.Content.XamlRoot,
            };
            page = new CreateDatabaseDialogPage();
            
            dialog.Content = page;
            dialog.Closing += Dialog_Closing;

            var res = await dialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                var eval = page.Evaluate();
                return Database.CreateNewDatabase(eval.path, eval.masterPassword);
            }
            return null;
        }

        private void Dialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (page == null || args.Result != ContentDialogResult.Primary)
                return;


            if (!page.PasswordsMatch)
            {
                InfoMessages.PasswordsDoNotMatch(page.InfoMessageParent);
                args.Cancel = true;
                return;
            }

            if (!page.PathValid)
            {
                InfoMessages.InvalidDatabasePath(page.InfoMessageParent);
                args.Cancel = true;
                return;
            }

            if(!page.PasswordLengthCorrect)
            {
                InfoMessages.PasswordTooShort(page.InfoMessageParent);
                args.Cancel = true;
                return;
            }

            if (page.PathValid && page.PathAlreadyExists)
            {
                InfoMessages.DatabaseWithThatNameAlreadyExists(page.InfoMessageParent);
                args.Cancel = true;
                return;
            }
        }
    }
}
