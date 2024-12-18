﻿/*
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

using EasePass.Dialogs;
using EasePass.Models;
using EasePass.Settings;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

/*This is not the AutomaticBackupHelper. This is for a database backup which WILL happen. You can not turn it off.*/

namespace EasePass.Helper;

public enum BackupCycle
{
    Weekly, Daily, Never
}

public class DatabaseBackupHelper
{
    private readonly Database database;
    private BackupCycle backupCycle;

    public DatabaseBackupHelper(Database database, BackupCycle backupCycle)
    {
        this.database = database;
        this.backupCycle = backupCycle;
    }

    private int CurrentDay => DateTime.Now.DayOfYear;
    private async Task<StorageFolder> GetBackupFolder()
    {
        try
        {
            return await ApplicationData.Current.LocalFolder.CreateFolderAsync("Backups", CreationCollisionOption.OpenIfExists);
        }
        catch (Exception ex)
        {
            InfoMessages.CouldNotCreateDatabaseBackupFolder(ex);
            return null;
        }
    }


    public async Task<bool> CheckAndDoBackup()
    {
        if (backupCycle == BackupCycle.Never)
            return true;

        int lastBackupDay = AppSettings.GetSettingsAsInt(AppSettingsValues.lastBackupDay, 0);

        //still same day or same week no backup needed:
        if ((backupCycle == BackupCycle.Weekly && lastBackupDay + 7 != CurrentDay) ||
            (backupCycle == BackupCycle.Daily && lastBackupDay == CurrentDay))
        {
            return true;
        }

        //create the backup with the current date in the name:
        StorageFolder folder = await GetBackupFolder();
        if (folder == null)
            return false;

        var backupPath = folder.Path + "\\" + DateTime.Now.ToString("dd_MM_yyyy_") + database.Name + "_Backup.epdb";
        database.Save(backupPath);
        AppSettings.SaveSettings(AppSettingsValues.lastBackupDay, CurrentDay);

        return true;
    }

    public async Task<string[]> GetAllBackupFiles()
    {
        var backupFolder = await GetBackupFolder();
        try
        {
            return Directory.GetFiles(backupFolder.Path, "*.epdb");
        }
        catch
        {
            //TODO: better error handling
            return null;
        }
    }

    public async Task<bool> LoadBackupFromFile(string path)
    {
        Database backup = new Database(path);
        if (backup.ValidatePwAndLoadDB(database.MasterPassword) != PasswordValidationResult.Success)
            return false;

        ImportPasswordsDialog dialog = new ImportPasswordsDialog();
        dialog.SetPagePasswords(backup.Items);

        var res = await dialog.ShowAsync(false);
        if (res.Items == null)
            return false;

        if (res.Override)
        {
            database.Items.Clear();
        }

        database.AddRange(res.Items);
        return true;
    }
}
