using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace E_Commerce.Web.ViewModels
{
    /// <summary>
    /// ViewModel cho trang sửa sản phẩm
    /// </summary>
    public class ProductEditViewModel : ProductCreateViewModel
    {
        [Required]
        public int Id { get; set; }
    }
}


