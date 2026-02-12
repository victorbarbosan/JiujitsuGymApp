using Microsoft.AspNetCore.Mvc.Rendering;

namespace JiujitsuGymApp.Helpers
{
    public static class EnumHelpers
    {
        public static List<SelectListItem> ToSelectList<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString()
                })
                .ToList();
        }
    }
}
