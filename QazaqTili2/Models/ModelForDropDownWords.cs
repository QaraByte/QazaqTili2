using Microsoft.AspNetCore.Mvc.Rendering;

namespace QazaqTili2.Models
{
    public class ModelForDropDownWords
    {
        public int ParentWordId { get; set; } //изменить свойство на ParentWordId
        public int SelectedOption { get; set; }
        //public string SelectedOption { get; set; }
        public List<SelectListItem> Options { get; set; }
    }
}
