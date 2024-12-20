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
using EasePass.Helper;
using EasePass.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;

namespace EasePass.Models;

public class Database : IDisposable, INotifyPropertyChanged
{
    public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
    public string Path = "";
    public SecureString MasterPassword = null;
    public ObservableCollection<PasswordManagerItem> Items = null;
    public DateTime LastModified => File.GetLastWriteTime(Path);
    public string LastModifiedStr => Name; //LastModified.ToString("D");

    private static Database loadedInstance;
    public static Database LoadedInstance
    {
        get => loadedInstance;
        set
        {
            loadedInstance = value;
            if (loadedInstance != null)
                AppSettings.LoadedDatabaseName = loadedInstance.Name;

            if (loadedInstance.PropertyChanged != null)
            {
                loadedInstance.PropertyChanged(loadedInstance, new PropertyChangedEventArgs("Path"));
                loadedInstance.PropertyChanged(loadedInstance, new PropertyChangedEventArgs("Name"));
                loadedInstance.PropertyChanged(loadedInstance, new PropertyChangedEventArgs("MasterPassword"));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public Database(string path)
    {
        this.Path = path;
        Items = new ObservableCollection<PasswordManagerItem>();
        Items.CollectionChanged += Items_CollectionChanged;

        CallPropertyChanged("Name");
        CallPropertyChanged("Path");
        CallPropertyChanged("MasterPassword");
        CallPropertyChanged("Items");
    }

    private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        CallPropertyChanged("Items");
    }

    public static Database CreateNewDatabase(string path, SecureString password)
    {
        Database db = new Database(path);
        db.MasterPassword = password;
        db.Save();
        return db;
    }

    public static string[] GetAllDatabasePaths()
    {
        string paths = AppSettings.DatabasePaths;
        ReadOnlySpan<char> chars = paths.AsSpan();

        int length = chars.Count('|') + 1;
        Span<Range> ranges = length < 1024 ? stackalloc Range[length] : new Range[length];

        int splittedPaths = chars.Split(ranges, '|');
        
        List<string> res = new List<string>();
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < splittedPaths; i++)
        {
            string path = chars[ranges[i]].ToString();
            if (File.Exists(path))
            {
                sb.Append(path);
                sb.Append('|');
                res.Add(path);
            }
        }
        SetAllDatabasePaths(sb.ToString());
        return res.Count == 0 ? new string[] { DefaultSettingsValues.databasePaths.Trim('|') } : res.ToArray();
    }

    public static void SetAllDatabasePaths(string paths)
    {
        AppSettings.DatabasePaths = paths;
    }

    public static void AddDatabasePath(string path)
    {
        List<string> paths = [.. GetAllDatabasePaths(), path];
        SetAllDatabasePaths(string.Join('|', paths));
    }

    public static void RemoveDatabasePath(string path)
    {
        List<string> paths = new List<string>();
        paths.AddRange(GetAllDatabasePaths());
        for (int i = 0; i < paths.Count; i++)
        {
            if (paths[i].ToLower() == path.ToLower())
            {
                paths.RemoveAt(i);
                i--;
            }
        }
        SetAllDatabasePaths(string.Join('|', paths));
    }

    public static Database[] GetAllUnloadedDatabases()
    {
        return GetAllDatabasePaths().Select(x => new Database(x)).ToArray();
    }

    public static ObservableCollection<PasswordManagerItem> GetItemsFromDatabase(string path, SecureString password)
    {
        if (FileHelper.HasExtension(path,".eped"))
        {
            string pw = new System.Net.NetworkCredential(string.Empty, password).Password;
            EncryptedDatabaseItem decryptedJson = JsonConvert.DeserializeObject<EncryptedDatabaseItem>(File.ReadAllText(path));
            if (!AuthenticationHelper.VerifyPassword(decryptedJson.PasswordHash, pw))
            {
                InfoMessages.ImportDBWrongPassword();
                return null;
            }
            var res = EncryptDecryptHelper.DecryptStringAES(decryptedJson.Data, pw, decryptedJson.Salt);
            if (!res.correctPassword)
            {
                InfoMessages.ImportDBWrongPassword();
                return null;
            }

            return LoadItems(res.decryptedString);
        }

        var readFileResult = ReadFile(path, password);
        if (!readFileResult.success)
        {
            InfoMessages.ImportDBWrongPassword();
            return null;
        }

        return LoadItems(readFileResult.data);
    }

    public PasswordValidationResult ValidatePwAndLoadDB(SecureString masterPassword, bool showWrongPasswordError = true)
    {
        if (!File.Exists(Path))
            return PasswordValidationResult.DatabaseNotFound;

        if (masterPassword != null)
            return Load(masterPassword, showWrongPasswordError) ? PasswordValidationResult.Success : PasswordValidationResult.WrongPassword;
        return PasswordValidationResult.WrongPassword;
    }

    public bool Load(SecureString password, bool showWrongPasswordError = true)
    {
        if (password == null)
        {
            InfoMessages.ImportDBWrongPassword();
            return false;
        }

        if (FileHelper.HasExtension(Path, ".eped"))
        {
            string pw = new System.Net.NetworkCredential(string.Empty, password).Password;
            EncryptedDatabaseItem decryptedJson = JsonConvert.DeserializeObject<EncryptedDatabaseItem>(File.ReadAllText(Path));
            if (!AuthenticationHelper.VerifyPassword(decryptedJson.PasswordHash, pw))
            {
                if (showWrongPasswordError)
                    InfoMessages.ImportDBWrongPassword();
                return false;
            }
            var res = EncryptDecryptHelper.DecryptStringAES(decryptedJson.Data, pw, decryptedJson.Salt);
            if (!res.correctPassword)
            {
                if (showWrongPasswordError)
                    InfoMessages.ImportDBWrongPassword();

                return false;
            }
            
            Items = LoadItems(res.decryptedString);
            ClearOldClicksCache();
            CallPropertyChanged("Items");

            MasterPassword = password;

            Path = System.IO.Path.ChangeExtension(Path, "epdb");
            Save();
        }

        var (data, success) = ReadFile(Path, password, showWrongPasswordError);
        if (!success)
            return false;

        MasterPassword = password;
        CallPropertyChanged("MasterPassword");
        

        Items = LoadItems(data);
        ClearOldClicksCache();
        CallPropertyChanged("Items");
        
        LoadedInstance = this;
        return true;
    }

    public void Save(string path = null)
    {
        var data = CreateJsonstring(Items);
        WriteFile(path ?? this.Path, data, MasterPassword);
    }

    public void ClearOldClicksCache()
    {
        if (Items == null)
            return;

        for (int i = 0; i < Items.Count; i++)
        {
            for (int j = 0; j < Items[i].Clicks.Count; j++)
            {
                string[] splitted = Items[i].Clicks[j].Split('.');
                if (splitted.Length != 3) continue;
                DateTime date = new DateTime(Convert.ToInt32(splitted[2]), Convert.ToInt32(splitted[1]), Convert.ToInt32(splitted[0]));
                if (DateTime.Now - date > TimeSpan.FromDays(365))
                {
                    Items[i].Clicks.RemoveAt(j);
                    j--;
                }
            }
        }
        CallPropertyChanged("Items");
    }

    private static string CreateJsonstring(ObservableCollection<PasswordManagerItem> pwItems)
    {
        if (pwItems == null)
            return "";

        return JsonConvert.SerializeObject(pwItems, Formatting.Indented);
    }

    private static ObservableCollection<PasswordManagerItem> LoadItems(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<ObservableCollection<PasswordManagerItem>>(json);
        }
        catch
        {
            InfoMessages.DatabaseInvalidData();
            return null;
        }
    }

    public void Dispose()
    {
        if (this == Database.LoadedInstance)
        {
            Items.Clear();
        }
        else
        {
            Items = null;
        }
        MasterPassword = null;
        CallPropertyChanged("Items");
        CallPropertyChanged("MasterPassword");
    }

    public ObservableCollection<PasswordManagerItem> FindItemsByName(string name)
    {
        return new ObservableCollection<PasswordManagerItem>(Items.Where(x => x.DisplayName.Contains(name, System.StringComparison.OrdinalIgnoreCase)));
    }

    public void DeleteItem(PasswordManagerItem item)
    {
        Items.Remove(item);
        CallPropertyChanged("Items");
    }

    public void AddItem(PasswordManagerItem item)
    {
        Items.Add(item);
        CallPropertyChanged("Items");
    }

    public int PasswordAlreadyExists(string password)
    {
        int count = 0;
        int length = Items.Count;
        for (int i = 0; i < length; i++)
        {
            if (Items[i].Password == password)
            {
                count++;
            }
        }
        return count;
    }

    public void AddRange(PasswordManagerItem[] items)
    {
        foreach (var item in items)
        {
            Items.Add(item);
        }

        CallPropertyChanged("Items");
    }

    public void AddRange(ObservableCollection<PasswordManagerItem> items)
    {
        foreach (var item in items)
        {
            Items.Add(item);
        }

        CallPropertyChanged("Items");
    }

    public void SetNewPasswords(ObservableCollection<PasswordManagerItem> items)
    {
        Items.Clear();

        foreach (var item in items)
        {
            Items.Add(item);
        }

        CallPropertyChanged("Items");
    }

    public void SetNewPasswords(PasswordManagerItem[] items)
    {
        if (items == null)
            return;

        Items.Clear();

        foreach (var item in items)
        {
            Items.Add(item);
        }

        CallPropertyChanged("Items");
    }

    private static (string data, bool success) ReadFile(string path, SecureString pw, bool showWrongPasswordError = true)
    {
        byte[] fileData = null;

        try
        {
            //Alternative open old .db file:
            var oldPath = System.IO.Path.ChangeExtension(path, ".db");
            if (File.Exists(oldPath) && !File.Exists(path))
            {
                fileData = File.ReadAllBytes(oldPath);
                File.Delete(oldPath);
                File.WriteAllBytes(path, fileData);
            }
            else
                fileData = File.ReadAllBytes(path);
        }
        catch (Exception ex ) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
        {
            InfoMessages.DatabaseFileNotFoundAt(path);
        }
        catch (UnauthorizedAccessException)
        {
            InfoMessages.NoAccessToPathDatabaseNotLoaded(path);
        }

        if (fileData != null)
        {
            var res = EncryptDecryptHelper.DecryptStringAES(fileData, pw);
            if (res.correctPassword)
                return (res.decryptedString, true);
         
            if (showWrongPasswordError)
                InfoMessages.ImportDBWrongPassword();
        }

        if(showWrongPasswordError)
        {
            InfoMessages.ImportDBWrongPassword();
        }
        return ("", false);
    }

    private static void WriteFile(string path, string jsonString, SecureString pw)
    {
        if (pw == null)
            return;

        var bytes = EncryptDecryptHelper.EncryptStringAES(jsonString, pw);
        try
        {
            File.WriteAllBytes(path, bytes);
        }
        catch (UnauthorizedAccessException)
        {
            InfoMessages.NoAccessToPathDatabaseNotSaved(path);
        }
        catch (IOException)
        {
            InfoMessages.DatabaseSaveToFileError(path);
        }
    }

    private void CallPropertyChanged(string name)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}