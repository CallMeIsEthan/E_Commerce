namespace E_Commerce.Web.ViewModels
{
    /// <summary>
    /// Model trả về cho AJAX requests
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public string RedirectUrl { get; set; }

        public static ApiResponse SuccessResponse(string message = "Thành công", object data = null, string redirectUrl = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                Data = data,
                RedirectUrl = redirectUrl
            };
        }

        public static ApiResponse ErrorResponse(string message = "Có lỗi xảy ra", object data = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Data = data
            };
        }
    }
}


