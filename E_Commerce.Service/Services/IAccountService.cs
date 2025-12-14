using E_Commerce.Dto;
using System.Collections.Generic;

namespace E_Commerce.Service
{
    public interface IAccountService
    {
        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        UserDto Register(UserCreateDto userCreateDto);

        /// <summary>
        /// Đăng nhập
        /// </summary>
        UserDto Login(UserLoginDto userLoginDto);

        /// <summary>
        /// Lấy thông tin user theo ID
        /// </summary>
        UserDto GetUserById(int userId);

        /// <summary>
        /// Lấy thông tin user theo email
        /// </summary>
        UserDto GetUserByEmail(string email);

        /// <summary>
        /// Cập nhật thông tin user
        /// </summary>
        UserDto UpdateUser(int userId, UserCreateDto userUpdateDto);

        /// <summary>
        /// Kiểm tra email đã tồn tại chưa
        /// </summary>
        bool IsEmailExists(string email);

        /// <summary>
        /// Xóa tài khoản (soft delete - set IsActive = false)
        /// </summary>
        bool DeleteUser(int userId);

        /// <summary>
        /// Lấy danh sách tất cả users
        /// </summary>
        IEnumerable<UserDto> GetAllUsers();

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        bool ChangePassword(int userId, string oldPassword, string newPassword);
    }
}


