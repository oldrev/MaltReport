using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

using Malt.Reporting.OfficeXml;
using Malt.Reporting.OpenDocument;

namespace Malt.Reporting
{
    public static class TemplateUtility
    {
        private static void RenderTemplateFile<DocType, TempType>(
                IDictionary<string, object> ctx,
                string templateFileName, string resultFileName)
            where DocType : ITemplate, new()
            where TempType : ITemplate
        {
            var doc = new DocType();
            using (var ts = File.OpenRead(templateFileName))
            {
                doc.Load(ts);
                var t = doc.Compile();
                t.Render(ctx);
                using (var resultFile3 = File.Open(
                    resultFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    t.Save(resultFile3);
                }
            }
        }

        public static void RenderOdfTemplate(
                IDictionary<string, object> ctx,
                string templateFileName, string resultFileName)
        {
            RenderTemplateFile<OdfTemplate, OdfTemplate>(
                ctx, templateFileName, resultFileName);
        }

        public static void RenderExcelTemplate(
                IDictionary<string, object> ctx,
                string templateFileName, string resultFileName)
        {
            RenderTemplateFile<ExcelMLTemplate, ExcelMLTemplate>(
                ctx, templateFileName, resultFileName);
        }
    }
}
