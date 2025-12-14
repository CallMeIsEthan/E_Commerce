namespace E_Commerce.Web.ViewModels
{
    /// <summary>
    /// Base ViewModel chứa các property chung cho tất cả ViewModels
    /// </summary>
    public class BaseViewModel
    {
        public string PageTitle { get; set; }
        public string PageDescription { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }
}


