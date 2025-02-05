using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

public class ExcelParser
{
    private readonly ServiceClient _serviceClient;

    public ExcelParser(ServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    public void ProcessExcelFromCRM(string recordId)
    {
        byte[] byteFile = DownloadFileFromCRM(recordId);
        if (byteFile != null)
        {
            ProcessExcel(byteFile);
        }
    }

    private byte[] DownloadFileFromCRM(string recordId)
    {
        EntityReference fileReference = new EntityReference("your_entity_name", new Guid(recordId));
        return ConmanFunctions.DownloadFile(_serviceClient, fileReference, "your_file_attribute");
    }

    public void ProcessExcel(byte[] byteFile)
    {
        using (MemoryStream memStream = new MemoryStream(byteFile))
        using (SpreadsheetDocument document = SpreadsheetDocument.Open(memStream, false))
        {
            WorkbookPart workbookPart = document.WorkbookPart;
            if (workbookPart == null)
            {
                Console.WriteLine("Invalid Excel file.");
                return;
            }

            Dictionary<string, Action<WorksheetPart>> sheetProcessors = new Dictionary<string, Action<WorksheetPart>>
            {
                { "Business Summary", ProcessBusinessSummarySheet },
                // { "Another Sheet", ProcessAnotherSheet },
                // { "Yet Another Sheet", ProcessYetAnotherSheet }
            };

            foreach (Sheet sheet in workbookPart.Workbook.Descendants<Sheet>())
            {
                if (sheetProcessors.ContainsKey(sheet.Name))
                {
                    WorksheetPart wsPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                    sheetProcessors[sheet.Name](wsPart);
                }
            }
        }
    }

    private void ProcessBusinessSummarySheet(WorksheetPart wsPart)
    {
        Console.WriteLine("Processing Business Summary sheet...");
        
        // Process first table C3:C15, D3:D15
        Dictionary<string, string> firstTableData = ExtractTable(wsPart, "C3", "D15");
        PushToDataverse(firstTableData);
        
        // Process second table (commented out for now)
        // Dictionary<string, string> secondTableData = ExtractTable(wsPart, "C17", "D22");
        // PushToDataverse(secondTableData);

        // Process third table (commented out for now)
        // Dictionary<string, string> thirdTableData = ExtractMultiColumnTable(wsPart, "A29", "D34");
        // PushToDataverse(thirdTableData);
        
        // Process fourth table (commented out for now)
        // Dictionary<string, string> fourthTableData = ExtractMultiColumnTable(wsPart, "A38", "D42");
        // PushToDataverse(fourthTableData);
    }

    private Dictionary<string, string> ExtractTable(WorksheetPart wsPart, string startCell, string endCell)
    {
        Dictionary<string, string> tableData = new Dictionary<string, string>();
        SheetData sheetData = wsPart.Worksheet.Elements<SheetData>().FirstOrDefault();
        if (sheetData == null) return tableData;
        
        foreach (Row row in sheetData.Elements<Row>())
        {
            foreach (Cell cell in row.Elements<Cell>())
            {
                string cellRef = cell.CellReference;
                if (IsCellWithinRange(cellRef, startCell, endCell))
                {
                    tableData[cellRef] = GetCellValue(wsPart, cell);
                }
            }
        }
        return tableData;
    }

    private string GetCellValue(WorksheetPart wsPart, Cell cell)
    {
        if (cell.CellValue == null) return string.Empty;
        return cell.CellValue.InnerText;
    }

    private bool IsCellWithinRange(string cellRef, string startCell, string endCell)
    {
        int startRow = int.Parse(startCell.Substring(1));
        int endRow = int.Parse(endCell.Substring(1));
        int cellRow = int.Parse(cellRef.Substring(1));
        
        char startCol = startCell[0];
        char endCol = endCell[0];
        char cellCol = cellRef[0];
        
        return cellRow >= startRow && cellRow <= endRow && cellCol >= startCol && cellCol <= endCol;
    }

    private void PushToDataverse(Dictionary<string, string> tableData)
    {
        Console.WriteLine("Pushing data to Dataverse:");
        foreach (var entry in tableData)
        {
            Console.WriteLine($"{entry.Key}: {entry.Value}");
        }
        // Actual Dataverse integration will be added later
    }
}
