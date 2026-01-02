#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEditor.Localization.Plugins.Google.Columns;
using UnityEngine;
using UnityEngine.Localization;

// Generic package for streamlining integration with the Unity localization package
// created 14/4/25
// modified 14/4/25


public class CSLocalizationUtlls : MonoBehaviour
{
    const string LOCFOLDER = "Assets/Localisation/StringTables";
    [Tooltip("the spreadsheet ID that should be set")]
    [SerializeField] private string spreadSheetId;
    [Tooltip("The ending suffix of each locale name")]
    [SerializeField] private string[] localeNameEndings;
    [Tooltip("The identifiers for each locale")]
    [SerializeField]private LocaleIdentifier[] localeIdentifiers;
    [Tooltip("The asset to try to find and add Google sheets localization integration for")]
    [SerializeField] private string assetTry;
    [SerializeField] private string assetTryUnspecified;
    [SerializeField] private SheetsServiceProvider sheetsProvider;

    public string[] GetAssetGUIDS()
    {
        return GetAssetGUIDS(assetTryUnspecified);
    }
    public string[] GetAssetGUIDS(string nameFilter)
    {
        Debug.Log("It's alive: " + assetTry);

        if (Directory.Exists(LOCFOLDER))
        {
            string[] stringGUIDs = AssetDatabase.FindAssets(nameFilter, new string[1] { LOCFOLDER });

            Debug.Log("stringGUIDs found: " + stringGUIDs.Length);

            return stringGUIDs;
        }
        return null;
    }

    public void AnalyseAssetGUIDS()
    {
        string[] assetGUIDS = GetAssetGUIDS();
        int stringTableCollections = 0;
        int stringTableCollectionsEmpty = 0;

        if (assetGUIDS != null && assetGUIDS.Length > 0)
        {
            foreach (string guid in assetGUIDS)
            {
                string stringPath = AssetDatabase.GUIDToAssetPath(guid);

                LocalizationTableCollection stringTable = AssetDatabase.LoadAssetAtPath<LocalizationTableCollection>(stringPath);

                if (stringTable)
                {
                    stringTableCollections++;

                    if (stringTable.Extensions.Count == 0)
                    {
                        stringTableCollectionsEmpty++;
                    }
                }
            }
        }

        Debug.Log("Found " + stringTableCollections + " which are LocalizationTableCollection assets");
        Debug.Log("Of these, " + stringTableCollectionsEmpty + " have no extension sheets");
    }

    public void GenerateSheetExtensions()
    {
        string[] assetGUIDS = GetAssetGUIDS(assetTry);

        if (assetGUIDS != null && assetGUIDS.Length > 0)
        {
            foreach (string guid in assetGUIDS)
            {
                string stringPath = AssetDatabase.GUIDToAssetPath(guid);

                LocalizationTableCollection stringTable = AssetDatabase.LoadAssetAtPath<LocalizationTableCollection>(stringPath);

                if (stringTable)
                {
                    Debug.Log("string table successfully found!!!!" + stringTable.name);

                    if (stringTable.Extensions.Count == 0)
                    {
                        GoogleSheetsExtension sheetsExtension = new GoogleSheetsExtension();
                        SheetColumn sheetColumnKey = new KeyColumn();

                        sheetColumnKey.ColumnIndex = 0;
                        stringTable.AddExtension(sheetsExtension);

                        sheetsExtension.SheetsServiceProvider = sheetsProvider;
                        sheetsExtension.SpreadsheetId = spreadSheetId;
                        sheetsExtension.Columns.Add(sheetColumnKey);

                        for (int i = 0; i < localeIdentifiers.Length; i++)
                        {
                            LocaleColumn sheetLocale = new LocaleColumn();

                            sheetLocale.ColumnIndex = i + 1;
                            sheetLocale.LocaleIdentifier = localeIdentifiers[i];

                            sheetsExtension.Columns.Add(sheetLocale);
                        }
                    }
                }
            }
        }
    }

    public void GenerateSheetExtensionsAll()
    {
        string[] assetGUIDS = GetAssetGUIDS(assetTryUnspecified);
        int countDone = 0;

        if (assetGUIDS != null && assetGUIDS.Length > 0)
        {
            foreach (string guid in assetGUIDS)
            {
                string stringPath = AssetDatabase.GUIDToAssetPath(guid);

                LocalizationTableCollection stringTable = AssetDatabase.LoadAssetAtPath<LocalizationTableCollection>(stringPath);

                if (stringTable)
                {
                    Debug.Log("string table successfully found!!!!" + stringTable.name);

                    if (stringTable.Extensions.Count == 0)
                    {
                        GoogleSheetsExtension sheetsExtension = new GoogleSheetsExtension();
                        SheetColumn sheetColumnKey = new KeyColumn();

                        sheetColumnKey.ColumnIndex = 0;
                        stringTable.AddExtension(sheetsExtension);

                        sheetsExtension.SheetsServiceProvider = sheetsProvider;
                        sheetsExtension.SpreadsheetId = spreadSheetId;
                        sheetsExtension.Columns.Add(sheetColumnKey);

                        for (int i = 0; i < localeIdentifiers.Length; i++)
                        {
                            LocaleColumn sheetLocale = new LocaleColumn();

                            sheetLocale.ColumnIndex = i + 1;
                            sheetLocale.LocaleIdentifier = localeIdentifiers[i];

                            sheetsExtension.Columns.Add(sheetLocale);
                        }
                        countDone++;
                    }
                }
            }
        }
        Debug.Log("Added " + countDone + " google sheets extension - HAVE FUN FINALIZING THAT");
    }
}

[CustomEditor(typeof(CSLocalizationUtlls))]
class DecalMeshHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Get GUIDS " + target))
        {
            CSLocalizationUtlls utils = target.GetComponent<CSLocalizationUtlls>();
            if (utils)
            {
                utils.GetAssetGUIDS();
            }
        }
        if (GUILayout.Button("Analyse GUIDs " + target))
        {
            CSLocalizationUtlls utils = target.GetComponent<CSLocalizationUtlls>();
            if (utils)
            {
                utils.AnalyseAssetGUIDS();
            }
        }
        if (GUILayout.Button("Attempt " + target))
        {
            CSLocalizationUtlls utils = target.GetComponent<CSLocalizationUtlls>();
            if (utils)
            {
                utils.GenerateSheetExtensions();
            }
        }
        if (GUILayout.Button("BIG RED BUTTON"))
        {
            CSLocalizationUtlls utils = target.GetComponent<CSLocalizationUtlls>();
            if (utils)
            {
                utils.GenerateSheetExtensionsAll();
            }
        }
    }
}

#endif