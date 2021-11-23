using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace RCP_Drawings_Releaser
{
    internal enum Side
    {
        Left,
        Right
    }
    
    internal class StringLogic
    {
        private class FileString
        {
            public string[] Fields { get; set; }
            public bool IsDrawing { get; set; }
            public string DrawingName { get; set; }
            public int SheetNumber { get; set; }
            public int Rev { get; set; }

        }
        
        private class FieldColumn
        {
            public int Num { get; set; }
            public List<string> Fields { get; set; }
            public bool ProbablyNumber { get; set; }
            public bool IsConst { get; set; }
            public string ConstValue { get; set; }
            public double HomogenityPercentage { get; set; }
            public bool IsNumber { get; set; }

            public FieldColumn()
            {
                Num = 0;
                Fields = new List<string>();
                ProbablyNumber = false;
                IsConst = false;
                ConstValue = "";
                HomogenityPercentage = 0.0;
                IsNumber = false;
            }
        }

        private static List<FileString> PdfsAndDwgs = new List<FileString>();
        private static List<FieldColumn> LeftColumns = new List<FieldColumn>();
        private static List<FieldColumn> RightColumns = new List<FieldColumn>();

        public static void AnalyzeData(List<string> fileNames, Side sheetNumSide, Side revNumSide)
        {
            FlushData();
            CreateFieldsFromFileNames(fileNames);
            CreateFieldColumns(Side.Left);
            CreateFieldColumns(Side.Right);
            FindSheetNumber(sheetNumSide == Side.Left ? LeftColumns : RightColumns);
            FindRev(revNumSide == Side.Left ? LeftColumns : RightColumns);

            
        }

        private static void FlushData()
        {
            PdfsAndDwgs.Clear();
            LeftColumns.Clear();
            RightColumns.Clear();
        }
        
        private static void CreateFieldsFromFileNames(List<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                var fileString = new FileString();
                
                char[] delimiters = {'.', ' '};
                
                if (Path.GetExtension(fileName) == ".pdf" || Path.GetExtension(fileName) == ".dwg" )
                {
                    fileString.IsDrawing = true;
                    fileString.Fields = fileName.Split(delimiters);
                    PdfsAndDwgs.Add(fileString);
                }
                else
                {
                    fileString.IsDrawing = false;
                }
            }
        }
        
        
        private static void CreateFieldColumns(Side side)
        {
            // Columns quantity calculation
            var minLength = PdfsAndDwgs.Select(fileString => fileString.Fields.Length).Min();

            var columns = ((side == Side.Left) ? LeftColumns : RightColumns);
            
            for (int i = 0; i < minLength; i++)
            {
                columns.Add(new FieldColumn());
                
                foreach (var drawing in PdfsAndDwgs)
                {
                    columns[i].Fields.Add(drawing.Fields[((side == Side.Left) ? i : minLength-1)]);
                    columns[i].Num = i;
                }
            }
            
            FillColumnData(columns);
        }
        private static void FillColumnData(List<FieldColumn> columns)
        {
            foreach (var column in columns)
            {
                column.IsConst = CheckIfConst(column.Fields);
                if (column.IsConst)
                {
                    column.ConstValue = column.Fields[0];
                }
                column.ProbablyNumber = CheckIfNumber(column.Fields);
                column.HomogenityPercentage = CalcHomogenity(column.Fields);
            }
        }

        private static bool CheckIfConst(List<string> fieldList)
        {
            return fieldList.All(field => field == fieldList[0]);
        }

        private static bool CheckIfNumber(List<string> fieldList)
        {
            var numNumber = fieldList.Sum(field => int.TryParse(field, out int _) ? 1 : 0);

            var percentage = ((double)numNumber / fieldList.Count());
            
            return (percentage > 0.9) ? true : false;
        }

        private static double CalcHomogenity(List<string> fieldList)
        {
            var values = fieldList.GroupBy(s => s).Select(g => new {Name = g.Key, Quantity = g.Count() });
            
            return values.Select(value => (double) value.Quantity / (double) PdfsAndDwgs.Count).Prepend(0.0).Max();
        }

        private static int FindSheetNumber(List<FieldColumn> columns)
        {
            var _ = 100.0;
            var i = 0;
            foreach (var column in columns)
            {
                if (column.HomogenityPercentage < _)
                    _ = column.HomogenityPercentage;
                i = column.Num;
            }
            return i;
        }
        
        private static int? FindRev(List<FieldColumn> columns)
        {
            int? result = null;
            foreach (var column in columns)
            {
                if (column.IsNumber)
                    result = column.Num;
                else
                    result = null;
            }

            return result;
        }
    }
}
