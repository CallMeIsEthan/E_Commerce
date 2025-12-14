using AutoMapper;
using E_Commerce.Data.Infrastructure;
using E_Commerce.Data.Repositories;
using E_Commerce.Dto;
using E_Commerce.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Helpers;

namespace E_Commerce.Service
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AccountService(IUserRepository userRepository, IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public UserDto Register(UserCreateDto userCreateDto)
        {
            // Validate input
            if (userCreateDto == null)
            {
                throw new Exception("Thông tin đăng ký không hợp lệ");
            }

            if (string.IsNullOrWhiteSpace(userCreateDto.Email))
            {
                throw new Exception("Email không được để trống");
            }

            // Normalize email (trim và lowercase)
            var normalizedEmail = userCreateDto.Email.Trim().ToLower();

            // Kiểm tra email đã tồn tại chưa (case-insensitive)
            if (IsEmailExists(normalizedEmail))
            {
                throw new Exception("Email đã được sử dụng. Vui lòng sử dụng email khác.");
            }

            // Map từ DTO sang Model
            var user = _mapper.Map<UserCreateDto, User>(userCreateDto);

            // Đảm bảo email được normalize
            user.Email = normalizedEmail;

            // Hash password
            user.PasswordHash = Crypto.HashPassword(userCreateDto.Password);

            // Set các giá trị mặc định
            user.IsActive = true;
            user.CreatedDate = DateTime.Now;

            // Thêm vào database
            _userRepository.Add(user);
            _unitOfWork.Commit();

            // Gán roles cho user (mặc định là Customer nếu không có role nào)
            if (userCreateDto.RoleIds == null || !userCreateDto.RoleIds.Any())
            {
                // Tìm role Customer
                var customerRole = _roleRepository.GetSingleByCondition(r => r.Name == "Customer" && r.IsActive);
                if (customerRole == null)
                {
                    throw new Exception("Không tìm thấy role 'Customer' trong hệ thống. Vui lòng liên hệ quản trị viên.");
                }
                AssignRoleToUser(user.Id, customerRole.Id);
            }
            else
            {
                // Gán các roles được chỉ định
                foreach (var roleId in userCreateDto.RoleIds)
                {
                    AssignRoleToUser(user.Id, roleId);
                }
            }

            // Commit các UserRole đã thêm
            _unitOfWork.Commit();

            // Map về DTO để trả về (có roles)
            return GetUserById(user.Id);
        }

        public UserDto Login(UserLoginDto userLoginDto)
        {
            // Tìm user theo email (userLoginDto.Username thực tế chứa email từ form login)
            var user = _userRepository.GetSingleByCondition(u => u.Email == userLoginDto.Username);

            if (user == null)
            {
                throw new Exception("Email hoặc mật khẩu không đúng");
            }

            // Kiểm tra password
            if (!Crypto.VerifyHashedPassword(user.PasswordHash, userLoginDto.Password))
            {
                throw new Exception("Email hoặc mật khẩu không đúng");
            }

            // Kiểm tra tài khoản có active không
            if (!user.IsActive)
            {
                throw new Exception("Tài khoản đã bị khóa");
            }

            // Map User sang UserDto và load roles
            return MapUserToDto(user);
        }

        public UserDto GetUserById(int userId)
        {
            var user = _userRepository.GetSingleById(userId);
            if (user == null)
            {
                return null;
            }
            return MapUserToDto(user);
        }

        public UserDto GetUserByEmail(string email)
        {
            var user = _userRepository.GetSingleByCondition(u => u.Email == email);
            if (user == null)
            {
                return null;
            }
            return MapUserToDto(user);
        }

        public UserDto UpdateUser(int userId, UserCreateDto userUpdateDto)
        {
            var user = _userRepository.GetSingleById(userId);
            if (user == null)
            {
                throw new Exception("Không tìm thấy user");
            }

            // Kiểm tra email mới có trùng với user khác không
            var normalizedEmail = userUpdateDto.Email?.Trim().ToLower() ?? user.Email;
            if (normalizedEmail != user.Email && IsEmailExists(normalizedEmail))
            {
                throw new Exception("Email đã tồn tại");
            }

            // Cập nhật thông tin (không cập nhật password ở đây)
            user.Email = normalizedEmail;
            user.FullName = userUpdateDto.FullName;
            user.Phone = userUpdateDto.Phone;
            user.Address = userUpdateDto.Address;
            user.UpdatedDate = DateTime.Now;

            _userRepository.Update(user);

            // Cập nhật roles nếu có
            if (userUpdateDto.RoleIds != null && userUpdateDto.RoleIds.Any())
            {
                // Xóa tất cả roles cũ
                var oldUserRoles = _userRoleRepository.GetMulti(ur => ur.UserId == userId);
                foreach (var oldRole in oldUserRoles)
                {
                    _userRoleRepository.Delete(oldRole);
                }

                // Thêm roles mới
                foreach (var roleId in userUpdateDto.RoleIds)
                {
                    AssignRoleToUser(userId, roleId);
                }
            }

            _unitOfWork.Commit();

            return GetUserById(userId);
        }

        public bool IsEmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            // Trim và normalize email (lowercase)
            var normalizedEmail = email.Trim().ToLower();
            
            // Lấy tất cả users có email và so sánh case-insensitive
            // Email nên được lưu dưới dạng lowercase để đảm bảo consistency
            var allUsers = _userRepository.GetMulti(u => u.Email != null).ToList();
            var existingUser = allUsers.FirstOrDefault(u => 
                !string.IsNullOrEmpty(u.Email) && 
                u.Email.Trim().ToLower() == normalizedEmail);
            
            return existingUser != null;
        }

        public bool DeleteUser(int userId)
        {
            var user = _userRepository.GetSingleById(userId);
            if (user == null)
            {
                return false;
            }

            // Soft delete - set IsActive = false
            user.IsActive = false;
            user.UpdatedDate = DateTime.Now;

            _userRepository.Update(user);
            _unitOfWork.Commit();

            return true;
        }

        public IEnumerable<UserDto> GetAllUsers()
        {
            var users = _userRepository.GetAll();
            return users.Select(u => MapUserToDto(u));
        }

        /// <summary>
        /// Map User sang UserDto và load roles
        /// </summary>
        private UserDto MapUserToDto(User user)
        {
            var userDto = _mapper.Map<User, UserDto>(user);

            // Load roles của user
            var userRoles = _userRoleRepository.GetMulti(ur => ur.UserId == user.Id, new[] { "Role" });
            userDto.Roles = userRoles.Select(ur => ur.Role?.Name).Where(r => r != null).ToList();
            userDto.RoleDetails = userRoles.Select(ur => _mapper.Map<Role, RoleDto>(ur.Role)).ToList();

            return userDto;
        }

        /// <summary>
        /// Gán role cho user
        /// </summary>
        private void AssignRoleToUser(int userId, int roleId)
        {
            // Kiểm tra role có tồn tại không
            var role = _roleRepository.GetSingleById(roleId);
            if (role == null || !role.IsActive)
            {
                throw new Exception($"Role với ID {roleId} không tồn tại hoặc đã bị vô hiệu hóa");
            }

            // Kiểm tra đã gán chưa
            var existingUserRole = _userRoleRepository.GetSingleByCondition(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (existingUserRole != null)
            {
                return; // Đã gán rồi
            }

            // Tạo UserRole mới
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedDate = DateTime.Now
            };

            _userRoleRepository.Add(userRole);
        }

        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = _userRepository.GetSingleById(userId);
            if (user == null)
            {
                throw new Exception("Không tìm thấy user");
            }

            // Kiểm tra password cũ
            if (!Crypto.VerifyHashedPassword(user.PasswordHash, oldPassword))
            {
                throw new Exception("Mật khẩu cũ không đúng");
            }

            // Hash password mới
            user.PasswordHash = Crypto.HashPassword(newPassword);
            user.UpdatedDate = DateTime.Now;

            _userRepository.Update(user);
            _unitOfWork.Commit();

            return true;
        }
    }
}