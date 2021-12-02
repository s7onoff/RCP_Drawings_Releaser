using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using RCP_Drawings_Releaser.ViewModels;

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

        public class FieldColumn
        {
            public int Num { get; set; }
            public int NumFromRight { get; set; }
            public List<string> Fields { get; set; }
            public bool IsConst { get; set; }
            public string ConstValue { get; set; }
            public double HomogenityPercentage { get; set; }
            public bool IsNumber { get; set; }
            public bool IsSheetNum { get; set; }
            public bool IsRevNum { get; set; }
            public Side ColumnSide { get; set; }
            public FieldColumn()
            {
                Num = 0;
                NumFromRight = 0;
                Fields = new List<string>();
                IsConst = false;
                ConstValue = "";
                HomogenityPercentage = 0.0;
                IsNumber = false;
                IsSheetNum = false;
                IsRevNum = false;
                ColumnSide = Side.Left;
            }
        }
        
        private static List<FileString> PdfsAndDwgs = new List<FileString>();
        private static List<FieldColumn> LeftColumns = new List<FieldColumn>();
        private static List<FieldColumn> RightColumns = new List<FieldColumn>();
        private static List<FieldColumn> ResultColumns = new List<FieldColumn>();

        private static Side SheetNumSide { get; set; }
        private static Side RevNumSide { get; set; }
        private static int SheetNumPosition { get; set; }
        private static int RevNumPosition { get; set; }
        private static char[] Delimiters { get; set; }
        public static List<FieldColumn> AnalyzeData(List<string> fileNames)
        {
            if (fileNames.Count == 0) return ResultColumns;
            FlushData();
            CreateFieldsFromFileNames(fileNames);
            if (PdfsAndDwgs.Count > 0)
            {
                CreateFieldColumns(Side.Left);
                FillColumnsData(Side.Left);
                CreateFieldColumns(Side.Right);
                FillColumnsData(Side.Right);
                FindSheetNumber();
                FindRev();
                PrepareResults();
            }
            return ResultColumns;
        }

        public static void ChangeDelimiters(string delimitersString)
        {
            Delimiters = delimitersString.ToCharArray();
            Delimiters = Delimiters.Append(' ').ToArray();
        }
        
        public static int GetNumberField(string sheetName, int fieldNumber, Side side)
        {
            var result = 0;
            try
            {
                var field = (side == Side.Left)
                    ? sheetName.Split(Delimiters)[fieldNumber]
                    : sheetName.Split(Delimiters).Reverse().ToArray()[fieldNumber];
                int.TryParse(Regex.Replace(field, "[^0-9]", ""), out result);
            }
            catch (IndexOutOfRangeException)
            {
                result = 0;
            }

            return result;
        }
        
        private static void FlushData()
        {
            PdfsAndDwgs.Clear();
            LeftColumns.Clear();
            RightColumns.Clear();
            ResultColumns.Clear();
        }
        
        private static void CreateFieldsFromFileNames(List<string> fileNames)
        {
            foreach (var fileName in fileNames)
            {
                var fileString = new FileString();

                if (Path.GetExtension(fileName) == ".pdf" || Path.GetExtension(fileName) == ".dwg" )
                {
                    fileString.IsDrawing = true;
                    fileString.Fields = Path.GetFileNameWithoutExtension(fileName).Split(Delimiters);
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
            if(PdfsAndDwgs.Count<=0) return;
            var minLength = PdfsAndDwgs.Select(fileString => fileString.Fields.Length).Min();

            var columns = ((side == Side.Left) ? LeftColumns : RightColumns);
            
            for (var i = 0; i < minLength; i++)
            {
                columns.Add(new FieldColumn());
                
                // All columns filling from left to right
                
                foreach (var drawing in PdfsAndDwgs)
                {
                    columns[i].Fields.Add(drawing.Fields[((side == Side.Left) ? i : drawing.Fields.Length - minLength + i)]);
                    columns[i].Num = i;
                    columns[i].NumFromRight = minLength - i - 1;
                    columns[i].ColumnSide = side;
                }
            }
        }
        private static int MinLengthOfMost(List<FileString> fileStrings)
        {
            var drawingsQuantity = fileStrings.Count;

            var orderedStrings = fileStrings.OrderBy(f => f.Fields.Length).ToList();
            
            for (var index = 0; index < decimal.Floor((decimal) (drawingsQuantity/10.0)); index++)
            {
                var fileString = orderedStrings[index];
                orderedStrings.Remove(fileString);
            }

            return orderedStrings.Count();
        }

        private static void ExcludeShortestFileStrings()
        {
            throw new NotImplementedException();
        }
        
        private static void FillColumnsData(Side side)
        {
            var columns = side == Side.Left ? LeftColumns : RightColumns;
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
            foreach (var column in LeftColumns.Where(column => column.HomogenityPercentage < columnHomogenityPercentage))
            {
                columnHomogenityPercentage = column.HomogenityPercentage;
                columnNum = column.Num;
                side = Side.Left;
            }

            foreach (var column in RightColumns.Where(column => column.HomogenityPercentage < columnHomogenityPercentage))
            {
                columnHomogenityPercentage = column.HomogenityPercentage;
                columnNum = column.Num;
                side = Side.Right;
            }

            SheetNumSide = side;
            (side == Side.Left? LeftColumns : RightColumns)[columnNum].IsSheetNum = true;
            SheetNumPosition = columnNum;
        }
        
        private static void FindRev()
        {
            
            // Checking for revision number set explicitly 
            var revPrefixes = new[] {"rev", "Р"};
            
            var bothColumnLists = new List<List<FieldColumn>>
            {
                LeftColumns,
                RightColumns
            };

            foreach (var columns in bothColumnLists)
            {
                for (var index = 0; index < columns.Count; index++)
                {
                    var column = columns[index];
                    if (column.IsSheetNum)
                        continue;
                    if (column.IsNumber && index != 0)
                    {
                        // Check if revision prefix is set in previous fields:
                        if (revPrefixes.Any(x =>
                            string.Equals(x, columns[index - 1].ConstValue, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            column.IsRevNum = true;
                            RevNumSide = (columns == LeftColumns ? Side.Left : Side.Right);
                            RevNumPosition = index;
                        }
                    }
                    else
                    {
                        // Check if revision prefix is set in revision field:
                        if (revPrefixes.Any(x => column.Fields.All(f => f.ToLower().StartsWith(x.ToLower()))))
                        {
                            var isReallyRev = false;
                            foreach (var prefix in revPrefixes)
                            {
                                try
                                {
                                    if(column.Fields.All(f => int.TryParse(f.Remove(0,prefix.Length), out _)))
                                        isReallyRev = true;
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                            
                            if( isReallyRev )
                            {
                                column.IsRevNum = true;
                                RevNumSide = (columns == LeftColumns ? Side.Left : Side.Right);
                                RevNumPosition = index;
                            }
                        }
                    }
                }
            }
            
            
            // If revision is not set explicitly, revision number is set as last one number field
            if (!(LeftColumns.Concat(RightColumns).ToList()).Any(x => x.IsRevNum))
            {
                int? columnNum = null;
                foreach (var column in RightColumns.Where(column => column.IsNumber))
                {
                    // Sheet number field cannot be a revision number:
                    if (column.IsSheetNum) continue;
                    columnNum = column.Num;
                }

                // Check if left columns are the same as right to avoid mistake of taking last field as revision number
                // if it is sheet number on the left and on the right simultaneously and it is not recognized before:
                if ((RightColumns[RightColumns.Count - 1].Fields).SequenceEqual(LeftColumns[LeftColumns.Count-1].Fields))
                {
                    for (var index = 0; index < RightColumns.Count; index++)
                    {
                        var rightColumn = RightColumns[index];
                        rightColumn.IsSheetNum = LeftColumns[index].IsSheetNum;
                    }
                }
                    
                // No number fields on the right -> we haven't found revision field:
                if (columnNum == null) return;
                
                // Number field on the right found, let's check if it now sheet number:
                if (RightColumns[(int) columnNum].IsSheetNum) return;
                
                // After all that checks we can reach here and just let the last number field to be revision
                RightColumns[(int) columnNum].IsRevNum = true;
                RevNumSide = Side.Right;
                RevNumPosition = (int)columnNum;
            }
        }
        
        private static void PrepareResults()
        {
            foreach (var column in LeftColumns)
            {
                switch (SheetNumSide)
                {
                    case Side.Left when RevNumSide == Side.Left:
                        {
                            if(column.Num <= SheetNumPosition || column.Num <= RevNumPosition)
                                ResultColumns.Add(column);
                            break;
                        }
                        case Side.Left when RevNumSide == Side.Right:
                        {
                            if(column.Num <= SheetNumPosition)
                                ResultColumns.Add(column);
                            break;
                        }
                        case Side.Right when RevNumSide == Side.Left:
                        {
                            // Unbelievable
                            if(column.Num <= RevNumPosition)
                                ResultColumns.Add(column);
                            break;
                        }
                        case Side.Right when RevNumSide == Side.Right:
                        {
                            break;
                        }
                    default:
                    {
                        // Impossible for real
                        ResultColumns.Add(column);
                        break;
                    }
                }
            }

            
            foreach (var column in RightColumns)
            {
                switch (SheetNumSide)
                {
                    case Side.Left when RevNumSide == Side.Left:
                    {
                        break;
                    }
                    case Side.Left when RevNumSide == Side.Right:
                    {
                        if(column.Num >= RevNumPosition)
                            ResultColumns.Add(column);
                        break;
                    }
                    case Side.Right when RevNumSide == Side.Left:
                    {
                        if(column.Num >= SheetNumPosition)
                            ResultColumns.Add(column);
                        break;
                    } 
                    case Side.Right when RevNumSide == Side.Right:
                    {
                        if(column.Num >= SheetNumPosition || column.Num >= RevNumPosition)
                            ResultColumns.Add(column);
                        break;
                    }
                    default:
                    {
                        // Impossible for real
                        ResultColumns.Add(column);
                        break;
                    }
                        
                }
            }
        }

    }
}
