﻿using CustomerManagement.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace CustomerManagement.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class CustomerManagementPageModel : AbpPageModel
{
    protected CustomerManagementPageModel()
    {
        LocalizationResourceType = typeof(CustomerManagementResource);
        ObjectMapperContext = typeof(CustomerManagementWebModule);
    }
}
