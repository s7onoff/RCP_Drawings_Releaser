using System;
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
        }

        private class FieldColumn
        {
            public int Num { get; set; }
            public List<string> Fields { get; set; }
            public bool IsConst { get; set; }
            public string ConstValue { get; set; }
            public double HomogenityPercentage { get; set; }
            public bool IsNumber { get; set; }
            public bool IsSheetNum { get; set; }
            public bool IsRevNum { get; set; }
            public FieldColumn()
            {
                Num = 0;
                Fields = new List<string>();
                IsConst = false;
                ConstValue = "";
                HomogenityPercentage = 0.0;
                IsNumber = false;
                IsSheetNum = false;
                IsRevNum = false;
            }
        }
        
        public class ResultField
        {
            public string Value { get; set; }
            public bool IsSheetNum { get; set; }
            public bool IsRevNum { get; set; }

            public string[] AllValues { get; set; }

            public ResultField()
            {
                Value = "xx";
                IsRevNum = false;
                IsSheetNum = false;
                AllValues = new string[]{};
            }
        }

        private static List<FileString> PdfsAndDwgs = new List<FileString>();
        private static List<FieldColumn> LeftColumns = new List<FieldColumn>();
        private static List<FieldColumn> RightColumns = new List<FieldColumn>();
        public static List<ResultField> AnalyzeResults = new List<ResultField>();
        
        
        
        public static List<ResultField> AnalyzeData(List<string> fileNames)
        {
            FlushData();
            CreateFieldsFromFileNames(fileNames);
            CreateFieldColumns(Side.Left);
            CreateFieldColumns(Side.Right);
            FindSheetNumber();
            FindRev(RightColumns);
            PrepareResults();
            return AnalyzeResults;
        }

        private static void PrepareResults()
        {
            foreach (var column in LeftColumns.Concat(Enumerable.Reverse(RightColumns).ToList()))
            {
                var thisFieldResult = new ResultField();

                if (column.IsConst)
                {
                    thisFieldResult.Value = column.ConstValue;
                }
                thisFieldResult.IsSheetNum = column.IsSheetNum;
                thisFieldResult.IsRevNum = column.IsRevNum;
                thisFieldResult.AllValues = column.Fields.ToArray();
                
                AnalyzeResults.Add(thisFieldResult);
            }
        }

        private static void FlushData()
        {
            PdfsAndDwgs.Clear();
            LeftColumns.Clear();
            RightColumns.Clear();
            AnalyzeResults.Clear();
        }
        
        private static void CreateFieldsFromFileNames(List<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                var fileString = new FileString();
                
                char[] delimiters = {'.', ' ', '-'};
                
                if (Path.GetExtension(fileName) == ".pdf" || Path.GetExtension(fileName) == ".dwg" )
                {
                    fileString.IsDrawing = true;
                    fileString.Fields = Path.GetFileNameWithoutExtension(fileName).Split(delimiters);
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
                column.IsNumber = CheckIfNumber(column.Fields);
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
            
            return percentage > 0.9;
        }

        private static double CalcHomogenity(List<string> fieldList)
        {
            var values = fieldList.GroupBy(s => s).Select(g => new {Name = g.Key, Quantity = g.Count() });
            
            return values.Select(value => (double) value.Quantity / PdfsAndDwgs.Count).Prepend(0.0).Max();
        }

        private static void FindSheetNumber()
        {
            var columnHomogenityPercentage = 1.0;
            var columnNum = 0;
            var side = Side.Left;
            foreach (var column in LeftColumns)
            {
                if (column.HomogenityPercentage < columnHomogenityPercentage)
                {
                    columnHomogenityPercentage = column.HomogenityPercentage;
                    columnNum = column.Num;
                    side = Side.Left;
                }
            }
            foreach (var column in RightColumns)
            {
                if (column.HomogenityPercentage < columnHomogenityPercentage)
                {
                    columnHomogenityPercentage = column.HomogenityPercentage;
                    columnNum = column.Num;
                    side = Side.Right;
                }
            }
            (side == Side.Left? LeftColumns : RightColumns)[columnNum].IsSheetNum = true;
        }
        
        private static void FindRev(List<FieldColumn> columns)
        {
            int? columnNum = null;
            foreach (var column in columns.Where(column => column.IsNumber))
            {
                columnNum = column.Num;
            } 
            
            if (columnNum != null)
            {
                columns[(int) columnNum].IsRevNum = true;
            }
        }
    }
}
