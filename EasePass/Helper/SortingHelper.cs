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

using EasePass.Models;
using System;
using System.Collections.Generic;

namespace EasePass.Helper
{
    internal class SortingHelper
    {
        public static int ByDisplayName(PasswordManagerItem pmi1, PasswordManagerItem pmi2)
        {
            if (pmi1.DisplayName == null) pmi1.DisplayName = "";
            if (pmi2.DisplayName == null) pmi2.DisplayName = "";
            return pmi1.DisplayName.CompareTo(pmi2.DisplayName);
        }

        public static int ByUsername(PasswordManagerItem pmi1, PasswordManagerItem pmi2)
        {
            if (pmi1.Username == null) pmi1.Username = "";
            if (pmi2.Username == null) pmi2.Username = "";
            return pmi1.Username.CompareTo(pmi2.Username);
        }

        public static int ByNotes(PasswordManagerItem pmi1, PasswordManagerItem pmi2)
        {
            if (pmi1.Notes == null) pmi1.Notes = "";
            if (pmi2.Notes == null) pmi2.Notes = "";
            return pmi1.Notes.CompareTo(pmi2.Notes);
        }

        public static int ByWebsite(PasswordManagerItem pmi1, PasswordManagerItem pmi2)
        {
            if (pmi1.Website == null) pmi1.Website = "";
            if (pmi2.Website == null) pmi2.Website = "";
            return pmi1.Website.CompareTo(pmi2.Website);
        }

        public static int ByPopularAllTime(PasswordManagerItem pmi1, PasswordManagerItem pmi2)
        {
            if (pmi1.Clicks == null) pmi1.Clicks = new List<string>();
            if (pmi2.Clicks == null) pmi2.Clicks = new List<string>();
            int comp = -pmi1.Clicks.Count.CompareTo(pmi2.Clicks.Count);
            if (comp == 0)
            {
                if (pmi1.DisplayName == null) pmi1.DisplayName = "";
                if (pmi2.DisplayName == null) pmi2.DisplayName = "";
                return pmi1.DisplayName.CompareTo(pmi2.DisplayName);
            }
            return comp;
        }

        public static int ByPopularLast30Days(PasswordManagerItem pmi1, PasswordManagerItem pmi2)
        {
            int DaysDeadline = 30;

            DateTime now = DateTime.Now;
            int pmi1Count = 0;
            for (int i = 0; i < pmi1.Clicks.Count; i++)
            {
                string[] splitted = pmi1.Clicks[i].Split('.');
                DateTime date = new DateTime(Convert.ToInt32(splitted[2]), Convert.ToInt32(splitted[1]), Convert.ToInt32(splitted[0]));
                if (now - date < TimeSpan.FromDays(DaysDeadline)) pmi1Count++;
            }
            int pmi2Count = 0;
            for (int i = 0; i < pmi2.Clicks.Count; i++)
            {
                string[] splitted = pmi2.Clicks[i].Split('.');
                DateTime date = new DateTime(Convert.ToInt32(splitted[2]), Convert.ToInt32(splitted[1]), Convert.ToInt32(splitted[0]));
                if (now - date < TimeSpan.FromDays(DaysDeadline)) pmi2Count++;
            }
            int comp = -pmi1Count.CompareTo(pmi2Count);
            if (comp == 0)
            {
                if (pmi1.DisplayName == null) pmi1.DisplayName = "";
                if (pmi2.DisplayName == null) pmi2.DisplayName = "";
                return pmi1.DisplayName.CompareTo(pmi2.DisplayName);
            }
            return comp;
        }

        public static int ByPasswordStrength(PasswordManagerItem pmi1, PasswordManagerItem pmi2)
        {
            bool?[] strength1 = PasswordHelper.EvaluatePassword(pmi1.Password);
            bool?[] strength2 = PasswordHelper.EvaluatePassword(pmi2.Password);
            if (strength1.Length != strength2.Length) return 0;
            int pmi1_res = 0;
            int pmi2_res = 0;
            for (int i = 0; i < strength1.Length; i++)
            {
                if (strength1[i] == true) pmi1_res++;
                if (strength1[i] == false) pmi1_res--;
                if (strength2[i] == true) pmi2_res++;
                if (strength2[i] == false) pmi2_res--;
            }
            return -pmi1_res.CompareTo(pmi2_res);
        }
    }
}
