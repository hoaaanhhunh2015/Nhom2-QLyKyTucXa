﻿using System;
using System.Linq;
using System.Web.Mvc;
using Web.Middlewares;
using Web.Models;

namespace Web.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        private Context db = new Context();

        [CheckUserSession]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, Route("Error")]
        public ActionResult Error(string msg)
        {
            Session["Msg"] = ViewBag.Msg = msg;
            return View("~/View/Shared/Error.cshtml");
        }

        [HttpGet, Route("LogIn")]
        public ActionResult Login()
        {
            try
            {
                return View("Login");
            }
            catch (Exception x)
            {
                ViewBag.Msg = x.Message;
                return View("~/View/Shared/Error.cshtml");
            }
        }

        [HttpPost, Route("Login")]
        public JsonResult Login(string email, string password)
        {
            var return_id = 0;
            var stt = "error";
            var stt_code = 400;
            var messeage = "Tên tài khoản hoặc mật khẩu không đúng!";

            if (string.IsNullOrEmpty(email))
            {
                messeage = "Tên tài khoản không được để trống!";
            }
            else if (!IsValidEmail(email))
            {
                messeage = "Địa chỉ email không hợp lệ!";
            }
            else if (string.IsNullOrEmpty(password))
            {
                messeage = "Mật khẩu không được để trống!";
            }
            else
            {
                var u = db.TAIKHOANs.FirstOrDefault(x => x.email == email && x.pass == password);
                if (u != null)
                {
                    stt = "OK";
                    stt_code = 200;
                    return_id = u.matk;
                    messeage = "Đăng nhập thành công! Đang chuyển hướng!";
                    Session["user_id"] = u.matk;
                }
                else
                {
                    stt_code = 404;
                }
            }

            var _j = new
            {
                return_id = return_id,
                stt = stt,
                stt_code = stt_code,
                messeage = messeage
            };
            return Json(_j);
        }

        // Hàm kiểm tra email hợp lệ
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }


        [HttpPost, Route("SignUp")]
        public JsonResult DangKy(TAIKHOAN e)
        {
            var return_id = 0;
            var stt = "error";
            var stt_code = 400;
            var messeage = "!";

            if (string.IsNullOrEmpty(e.email))
            {
                messeage = "Email tài khoản không được để trống!";
            }
            else if (!IsValidEmail(e.email))
            {
                messeage = "Địa chỉ email không hợp lệ!";
            }
            else if (string.IsNullOrEmpty(e.pass))
            {
                messeage = "Mật khẩu không được để trống!";
            }
            else
            {
                var u = db.TAIKHOANs.FirstOrDefault(x => x.email == e.email);
                if (u == null)
                {
                    try
                    {
                        e.cvu = "Học Sinh";
                        db.TAIKHOANs.Add(e);
                        db.SaveChanges();
                        stt = "OK";
                        stt_code = 200;
                        return_id = e.matk;
                        messeage = "Tạo tài khoản thành công! Vui lòng đăng nhập!";
                    }
                    catch (Exception x)
                    {
                        messeage = x.Message;
                    }
                }
                else
                {
                    stt_code = 202;
                    messeage = "Email đã tồn tại, vui lòng chọn email khác!";
                }
            }

            var _j = new
            {
                return_id = return_id,
                stt = stt,
                stt_code = stt_code,
                messeage = messeage
            };
            return Json(_j);
        }


        [HttpGet, Route("LogOut")]
        public ActionResult LogOut()
        {
            Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        [CheckUserSession]
        [HttpGet, Route("ThongTinTaiKhoan")]
        public ActionResult ThongTinTaiKhoan()
        {
            var _id = int.Parse(Session["user_id"].ToString());
            var e = db.TAIKHOANs.FirstOrDefault(x => x.matk == _id);
            if (e != null)
            {
                return View(e);
            }
            else
            {
                ViewBag.Msg = "Session không hợp lệ!";
                return View("~/Shared/Error.cshtml");
            }
        }

        [CheckUserSession]
        [HttpPost, Route("ThongTinTaiKhoan")]
        public ActionResult ThongTinTaiKhoan(TAIKHOAN e)
        {
            if (e != null)
            {
                var o = db.TAIKHOANs.FirstOrDefault(x => x.matk == e.matk);
                if (o != null) 
                {
                    if (!string.IsNullOrEmpty(e.hoten) && e.hoten != o.hoten)
                        o.hoten = e.hoten;

                    if (!string.IsNullOrEmpty(o.email))
                        e.email = o.email;  

                    if (!string.IsNullOrEmpty(o.cvu))
                        e.cvu = o.cvu;  
                    
                    if (!string.IsNullOrEmpty(e.pass) && e.pass != o.pass)
                        o.pass = e.pass; 

                    var stt = db.SaveChanges();
                    if (stt > 0)
                        ViewBag.Msg = "Lưu thành công!";
                    else
                        ViewBag.Msg = "Lỗi!";
                }
            }
            else
            {
                ViewBag.Msg = "Tham số không hợp lệ";
            }
            return View(e); 
        }
    }
}
