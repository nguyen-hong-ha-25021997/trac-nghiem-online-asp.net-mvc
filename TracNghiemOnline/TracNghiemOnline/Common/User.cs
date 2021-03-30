using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TracNghiemOnline.Models;
namespace TracNghiemOnline.Common
{
    public class User
    {
        // các thông tin chính của user
        public bool ISLOGIN { get; set; } = false;
        public int ID { get; set; }
        public int PERMISSION { get; set; }
        public string USERNAME { get; set; }
        public string EMAIL { get; set; }
        public string AVATAR { get; set; }
        public string NAME { get; set; }
        public int TESTCODE { get; set; } = 0;
        public string TIME { get; set; }

        public User()
        {
            try
            {
                // lấy thông tin của user hiện tại
                ISLOGIN = (bool)HttpContext.Current.Session[UserSession.ISLOGIN];
                ID = (int)HttpContext.Current.Session[UserSession.ID];
                PERMISSION = (int)HttpContext.Current.Session[UserSession.PERMISSION];      // quyền (admin, giáo viên, học sinh)
                USERNAME = (string)HttpContext.Current.Session[UserSession.USERNAME];       // Tên đăng nhập
                EMAIL = (string)HttpContext.Current.Session[UserSession.EMAIL];
                AVATAR = (string)HttpContext.Current.Session[UserSession.AVATAR];
                NAME = (string)HttpContext.Current.Session[UserSession.NAME];               // Tên thường
                if (HttpContext.Current.Session[UserSession.TESTCODE] != null)
                {
                    TESTCODE = (int)HttpContext.Current.Session[UserSession.TESTCODE];    // lấy mã bài test đang làm
                }
                TIME = (string)HttpContext.Current.Session[UserSession.TIME];
            }
            catch (Exception) { }
        }
        public bool IsLogin()       // ktra đã đăng nhập chưa
        {
            try
            {
                if (ISLOGIN)
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public void Reset()             // Xóa dữ liệu phiên đăng nhập
        {
            HttpContext.Current.Session.Clear();
        }
        public bool IsAdmin()           // ktra có phải admin không
        {
            try
            {
                if (ISLOGIN && PERMISSION == 1)
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool IsTeacher()         // ktra có phải giáo viên không
        {
            try
            {
                if (ISLOGIN && PERMISSION == 2)
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool IsStudent()         // ktra có phải học sinh không
        {
            try
            {
                if (ISLOGIN && PERMISSION == 3)
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool IsTesting()         // ktra có phải đang làm test không
        {
            try
            {
                if (ISLOGIN && PERMISSION == 3 && TESTCODE > 0)
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}