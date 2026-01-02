#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSGoogleSheetsIntegration
// integrates functionality for connecting to, sending and receiving data to Google Sheets
// created 15/4/25
// modified 15/4/25

public class CSGoogleSheetsIntegration : MonoBehaviour
{
    /*
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;

public class GoogleSheetsManager : MonoBehaviour
{
    // Replace with your spreadsheet ID and sheet ID
    public string spreadsheetId = "YOUR_SPREADSHEET_ID";
    public string sheetName = "New Sheet"; // Name of the new sheet

    // Path to your service account key file
    public string serviceAccountKeyPath = "path/to/your/serviceAccount.json";

    // API Credentials
    public string _scopes = "https://www.googleapis.com/auth/spreadsheets";

    // Use a coroutine to handle asynchronous operations
    IEnumerator AddSheetCoroutine()
    {
        // Load your service account credentials
        GoogleCredential credential = GoogleCredential.FromFile(serviceAccountKeyPath)
            .CreateScoped(_scopes);

        // Create a Google Sheets service
        SheetsService service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Your App Name"
        });

        // Define the sheet to be added
        var requestBody = new Google.Apis.Sheets.v4.Data.AddSheetRequest {
            Title = sheetName,
        };

        // Define the spreadsheet body
        var spreadsheet = new Google.Apis.Sheets.v4.Data.Spreadsheet {
            Sheets = new List<Google.Apis.Sheets.v4.Data.Sheet>(),
            Properties = new Google.Apis.Sheets.v4.Data.SheetProperties() {
                Title = sheetName
            }
        };

        // Call the add sheet API
        try
        {
            Google.Apis.Sheets.v4.Data.Sheet sheet = new Google.Apis.Sheets.v4.Data.Sheet();
            service.Spreadsheets.BatchUpdate(spreadsheet, spreadsheetId).Execute();
            sheet = service.Spreadsheets.Sheets.Get(spreadsheetId, sheetName).Execute();
            Debug.Log("Sheet added successfully: " + sheet.Properties.Title);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error adding sheet: " + e.Message);
        }

        yield return null; // Wait for the coroutine to complete
    }

    public void AddSheet()
    {
        StartCoroutine(AddSheetCoroutine());
    }
}     */
}
#endif