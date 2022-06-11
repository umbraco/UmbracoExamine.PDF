using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

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
            if (baseValidate.Status == ValueSetValidationStatus.Failed)
            {
                return new ValueSetValidationResult(ValueSetValidationStatus.Failed, valueSet);
            }

            //must have a 'path'
            if (!valueSet.Values.TryGetValue(PathKey, out var pathValues)
                || pathValues.Count == 0
                || pathValues[0] == null
                || pathValues[0].ToString().IsNullOrWhiteSpace())
            {
                return new ValueSetValidationResult(ValueSetValidationStatus.Failed, valueSet);
            }
            var path = pathValues[0].ToString();

            var filteredValues = valueSet.Values.ToDictionary(x => x.Key, x => x.Value.ToList());

            bool isFiltered = !ValidatePath(path);

            var filteredValueSet = new ValueSet(valueSet.Id, valueSet.Category, valueSet.ItemType, filteredValues.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value));
            return new ValueSetValidationResult(isFiltered ? ValueSetValidationStatus.Filtered : ValueSetValidationStatus.Valid, filteredValueSet);
        }
    }
}
