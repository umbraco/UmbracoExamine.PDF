using System.Collections.Generic;
using Examine;
using Umbraco.Core;
using Umbraco.Examine;

namespace UmbracoExamine.PDF
{
    public class PdfValueSetValidator : ValueSetValidator
    {
        public int? ParentId { get; }

        private const string PathKey = "path";

        public PdfValueSetValidator(int? parentId,
            IEnumerable<string> includeItemTypes = null, IEnumerable<string> excludeItemTypes = null)
            : base(includeItemTypes, excludeItemTypes, null, null)
        {
            ParentId = parentId;
        }

        public bool ValidatePath(string path)
        {
            //check if this document is a descendent of the parent
            if (ParentId.HasValue && ParentId.Value > 0)
            {
                // we cannot return FAILED here because we need the value set to get into the indexer and then deal with it from there
                // because we need to remove anything that doesn't pass by parent Id in the cases that umbraco data is moved to an illegal parent.
                if (!path.Contains(string.Concat(",", ParentId.Value, ",")))
                    return false;
            }

            return true;
        }

        public override ValueSetValidationResult Validate(ValueSet valueSet)
        {
            var baseValidate = base.Validate(valueSet);
            if (baseValidate == ValueSetValidationResult.Failed)
                return ValueSetValidationResult.Failed;

            //must have a 'path'
            if (!valueSet.Values.TryGetValue(PathKey, out var pathValues)) return ValueSetValidationResult.Failed;
            if (pathValues.Count == 0) return ValueSetValidationResult.Failed;
            if (pathValues[0] == null) return ValueSetValidationResult.Failed;
            if (pathValues[0].ToString().IsNullOrWhiteSpace()) return ValueSetValidationResult.Failed;
            var path = pathValues[0].ToString();

            if (!ValidatePath(path))
                return ValueSetValidationResult.Filtered;

            return ValueSetValidationResult.Valid;
        }
    }
}
