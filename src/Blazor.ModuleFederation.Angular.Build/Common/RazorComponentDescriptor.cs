using System.Collections.Generic;

namespace Blazor.ModuleFederation.Angular.Build.Common
{
    internal class RazorComponentDescriptor
    {
        public string Name { get; }

        public IReadOnlyList<BoundAttributeDescriptor> Parameters { get; }

        public RazorComponentDescriptor(TagHelperDescriptor tagHelper)
        {
            Name = GetComponentTypeName(tagHelper.Name);
            Parameters = tagHelper.BoundAttributes;
        }

        private static string GetComponentTypeName(string fullName)
        {
            return fullName.Substring(fullName.LastIndexOf('.') + 1);
        }
    }
}
