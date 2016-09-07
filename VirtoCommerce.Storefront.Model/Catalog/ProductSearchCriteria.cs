using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.Catalog
{
    public class ProductSearchCriteria : PagedSearchCriteria
    {
        //For JSON deserialization 
        public ProductSearchCriteria()
            : base(new NameValueCollection())
        {
        }

        public ProductSearchCriteria(Language language, Currency currency)
            : this(language, currency, new NameValueCollection())
        {
        }
        public ProductSearchCriteria(Language language, Currency currency, NameValueCollection queryString)
            : base(queryString)
        {
            Language = language;
            Currency = currency;

            Parse(queryString);
        }

        public string Outline { get; set; }

        public Currency Currency { get; set; }

        public Language Language { get; set; }

        public string Keyword { get; set; }

        public Term[] Terms { get; set; }

        public string SortBy { get; set; }

        public ProductSearchCriteria Clone()
        {
            var retVal = new ProductSearchCriteria(Language, Currency);
            //retVal.CatalogId = CatalogId;
            retVal.Currency = Currency;
            retVal.Language = Language;
            retVal.Keyword = Keyword;
            retVal.SortBy = SortBy;
            retVal.PageNumber = PageNumber;
            retVal.PageSize = PageSize;
            if (Terms != null)
            {
                retVal.Terms = Terms.Select(x => new Term { Name = x.Name, Value = x.Value }).ToArray();
            }
            return retVal;
        }

        private void Parse(NameValueCollection queryString)
        {
            Keyword = queryString.Get("q");
            //TODO move this code to Parse or Converter method
            // tags=name1:value1,value2,value3;name2:value1,value2,value3
            SortBy = queryString.Get("sort_by");
            Terms = (queryString.GetValues("terms") ?? new string[0])
                .SelectMany(s => s.Split(';'))
                .Select(s => s.Split(':'))
                .Where(a => a.Length == 2)
                .SelectMany(a => a[1].Split(',').Select(v => new Term { Name = a[0], Value = v }))
                .ToArray();
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

                //if (this.CatalogId != null)
                //    hash = hash * 59 + this.CatalogId.GetHashCode();

                if (this.Currency != null)
                    hash = hash * 59 + this.Currency.Code.GetHashCode();

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
