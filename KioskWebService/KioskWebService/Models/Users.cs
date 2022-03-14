using System;

namespace KioskWebService.Models
{
    public class Users
    {
        public UserDetails userDetails { get; set; }       
        public UsersLogin usersLogin { get; set; }
        public int respcode { get; set; }
        public string respmessage { get; set; }
    }
    public class UserDetails 
    {
        public int _uID { get; set; }
        public decimal _resourceID { get; set; }
        public string _userLogin { get; set; }
        public string _userPassword { get; set; }

        public string _branchCode { get; set; }
        public string _roleID { get; set; }
        public string _divCode { get; set; }
        public int _isreliever { get; set; }
        public string _tempBranchCode { get; set; }

        public int _tempZoneCode { get; set; }
        public int _zoneCode { get; set; }
        public DateTime _startDate { get; set; }
        public DateTime _endDate { get; set; }
        public int _isActive { get; set; }

        public DateTime _syscreated { get; set; }
        public DateTime _sysmodified { get; set; }
        public string _syscreator { get; set; }
        public string _sysmodifier { get; set; }
        public int _isresign { get; set; }

        public string _cellNo { get; set; }
        public string _emailAddress { get; set; }

        public string _passwordStrength { get; set; }
    }

    public class UsersLogin 
    {
        public decimal _resourceID { get; set; }
        public string _operator { get; set; }
        public string _branchCode { get; set; }
        public string _branchName { get; set; }
        public string _role { get; set; }
        public int _zoneCode { get; set; }
    }
}
