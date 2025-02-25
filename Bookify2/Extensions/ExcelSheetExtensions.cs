﻿using ClosedXML.Excel;

namespace Bookify.Extensions
{
    public static class ExcelSheetExtensions
    {
        private static int _startRow = 4;

        public static void AddHeader(this IXLWorksheet sheet, string[] cells)
        {
            int startRow = 4;
            for (int i = 0; i < cells.Length; i++)
                sheet.Cell(startRow, i + 1).SetValue(cells[i]);

            //var header = sheet.Range(startRow, 1, startRow, cells.Length);
            //header.Style.Fill.BackgroundColor = XLColor.Black;
            //header.Style.Font.FontColor = XLColor.White;
            //header.Style.Font.SetBold();

        }
        public static void AddTable(this IXLWorksheet sheet, int NumberOfRow, int NumberOfColumn)
        {
            var range = sheet.Range(_startRow, 1, NumberOfRow + _startRow, NumberOfColumn);
            var table = range.CreateTable();
            table.Theme = XLTableTheme.TableStyleLight9;
            table.ShowAutoFilter = false;

        }
        public static void AddImage(this IXLWorksheet sheet, string imagePath)
        {
            sheet.AddPicture(imagePath)
               .MoveTo(sheet.Cell("A1"))
               .Scale(.2);

        }
        public static void Format(this IXLWorksheet sheet)
        {

            sheet.ColumnsUsed().AdjustToContents();

            sheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.CellsUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.CellsUsed().Style.Border.OutsideBorderColor = XLColor.Black;
        }
    }
}
