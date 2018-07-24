using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrystalDecisions.Shared;

namespace FluidTrade.Reporting
{
    /// <summary>
    /// Supported Report generation types. View is used for displaying report, all others are for export.
    /// </summary>
    public enum ReportGenerationType
    {
        ShowReportInDlg,
        CrystalReport,
        RichText,
        WordForWindows,
        Excel,
        PortableDocFormat,
        HTML32,
        HTML40,
        ExcelRecord
    }

	/// <summary>
	/// helper class to convert from FluidTrade.Reporting.ReportGenerationType to a crystal reports export type
	/// </summary>
    public static class ReportGenerationTypeHelper
    {
        /// <summary>
        /// Convert a ReportGenerationType to a Crystal Export type enum
        /// </summary>
        /// <param name="type">internal ReportGenerationType to be converted</param>
        /// <returns>Crystal Report export type</returns>
        public static ExportFormatType ToCrystalType(ReportGenerationType type)
        {
            switch (type)
            {
                case ReportGenerationType.CrystalReport:
                    return ExportFormatType.CrystalReport;
                case ReportGenerationType.RichText:
                    return ExportFormatType.RichText;
                case ReportGenerationType.WordForWindows:
                    return ExportFormatType.WordForWindows;
                case ReportGenerationType.PortableDocFormat:
                    return ExportFormatType.PortableDocFormat;
                case ReportGenerationType.HTML32:
                    return ExportFormatType.HTML32;
                case ReportGenerationType.HTML40:
                    return ExportFormatType.HTML40;
                case ReportGenerationType.ExcelRecord:
                    return ExportFormatType.ExcelRecord;
               default:
                    return ExportFormatType.NoFormat;        
            }
        }
    }
}
