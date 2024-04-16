using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Common
{
    public static class ViewDataHelper
    {
        public static SelectList CreateViewData<T>(
            IQueryable<T> items,
        Expression<Func<T, object>> valueField,
        Expression<Func<T, string>> textField,
        object selectedValue = null)
        {
            var orderedItems = items.OrderBy(textField);
            var selectList = new SelectList(orderedItems, valueField.ToString(), textField.ToString(), selectedValue);

            if (selectedValue == null)
            {
                selectList = new SelectList(orderedItems, valueField.ToString(), textField.ToString());
            }

            return selectList;
        }
    }
}
