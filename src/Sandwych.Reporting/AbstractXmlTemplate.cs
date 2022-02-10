using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using Fluid;
using Fluid.Parser;

namespace Sandwych.Reporting
{
    public abstract class AbstractXmlTemplate<TDocument> : AbstractTemplate<TDocument>
        where TDocument : AbstractXmlDocument<TDocument>, new()
    {

        protected IFluidTemplate TextTemplate { get; private set; }

        public AbstractXmlTemplate(TDocument document) : base(document)
        {
            this.PrepareTemplate();

            FluidParser parser = new FluidParser();
            var stringTemplate = this.GetStringTemplate();
            if (!parser.TryParse(stringTemplate, out var fluidTemplate, out var errors))
            {
                throw new SyntaxErrorException(errors);
            }
            this.TextTemplate = fluidTemplate;
        }

        private string GetStringTemplate()
        {
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                this.TemplateDocument.XmlDocument.WriteTo(writer);
            }
            return sb.ToString();
        }

    }
}
