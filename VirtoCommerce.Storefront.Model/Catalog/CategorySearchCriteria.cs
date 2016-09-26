using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.Catalog
{
    public class CategorySearchCriteria : PagedSearchCriteria
    {
        //For JSON deserialization 
        public CategorySearchCriteria()
            : base(new NameValueCollection())
        {
        }

        public CategorySearchCriteria(Language language)
            : this(language, new NameValueCollection())
        {
        }
        public CategorySearchCriteria(Language language, NameValueCollection queryString)
            : base(queryString)
        {
            Language = language;

            Parse(queryString);
        }

        public string Outline { get; set; }

        public Language Language { get; set; }

        public string Keyword { get; set; }

        public string SortBy { get; set; }

        public CategorySearchCriteria Clone()
        {
            var retVal = new CategorySearchCriteria(Language);
            retVal.Language = Language;
            retVal.Keyword = Keyword;
            retVal.SortBy = SortBy;
            retVal.PageNumber = PageNumber;
            retVal.PageSize = PageSize;
            return retVal;
        }

        private void Parse(NameValueCollection queryString)
        {
            Keyword = queryString.Get("q");
            SortBy = queryString.Get("sort_by");
        }


        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            // credit: http://stackoverflow.com/a/263416/677735
            unchecked // Overflow is fine, just wrap
            {
                int hash = base.GetHashCode();

                if (this.Language != null)
                    hash = hash * 59 + this.Language.CultureName.GetHashCode();

                if (this.Keyword != null)
                    hash = hash * 59 + this.Keyword.GetHashCode();

                if (this.SortBy != null)
                    hash = hash * 59 + this.SortBy.GetHashCode();

                return hash;
            }
        }

        public override string ToString()
        {
            var retVal = new List<string>();
            retVal.Add(string.Format("page={0}", PageNumber));
            retVal.Add(string.Format("page_size={0}", PageSize));
            if (Keyword != null)
            {
                retVal.Add(string.Format("q={0}", Keyword));
            }
            return string.Join("&", retVal);
        }
    }
}