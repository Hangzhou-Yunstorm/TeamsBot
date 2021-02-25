using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.LanguageGeneration
{
    public class TemplateManager : MultiLanguageLG
    {
        public TemplateManager(Dictionary<string, string> filePerLocale, string defaultLanguage) : base(filePerLocale, defaultLanguage)
        {
        }

        public virtual Activity GenerateActivity(string templateName, object data = null)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                throw new ArgumentNullException(nameof(templateName));
            }

            return ActivityFactory.FromObject(Generate(templateName, data, null));
        }
    }
}
