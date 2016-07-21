using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using DotLiquid;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.LiquidThemeEngine.Filters
{
    public class ArrayFilters
    {
        /// <summary>
        /// Sorts the elements of an array by a given attribute of an element in the array.
        /// {% assign sorted = pages | sort:"date:desc;name" %}
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static object Sort(object input, string sort)
        {
            var retVal = input;
            IEnumerable enumerable = retVal as IEnumerable;
            IMutablePagedList muttablePagedList = input as IMutablePagedList;
            var sortInfos = SortInfo.Parse(sort).ToList();            
            if (muttablePagedList != null)
            {
                muttablePagedList.Slice(muttablePagedList.PageNumber, muttablePagedList.PageSize, sortInfos);
            }
            if (enumerable != null)
            {
                //Queryable.Cast<T>(input).OrderBySortInfos(sortInfos) call by reflection
                var queryable = enumerable.AsQueryable();
                var elementType = enumerable.GetType().GetEnumerableType();
                MethodInfo castMethodInfo = typeof(Queryable).GetMethods().Where(x => x.Name == "Cast" && x.IsGenericMethod).First();
                castMethodInfo = castMethodInfo.MakeGenericMethod(new Type[] { elementType });

                 var genericQueryable = castMethodInfo.Invoke(null, new object[] { queryable });

                var orderBySortInfosMethodInfo = typeof(IQueryableExtensions).GetMethod("OrderBySortInfos");
                orderBySortInfosMethodInfo = orderBySortInfosMethodInfo.MakeGenericMethod(new Type[] { elementType });
                retVal = orderBySortInfosMethodInfo.Invoke(null, new object[] { genericQueryable, sortInfos.ToArray() });
            }

            return retVal;
        }
    }
}
