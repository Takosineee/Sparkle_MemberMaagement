using Gym.Models;
using Gym.Models.CDictionary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Experimental.ProjectCache;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Schema;

namespace Gym.Controllers
{
    public class MemberController : Controller
    {
        private readonly GymContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MemberController(GymContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        //get memberId
        public int? getCurrentMemberId()
        {
            var memberInfoJson = _httpContextAccessor.HttpContext?.Session.GetString(CDictionary.SK_LOINGED_USER);
            if (memberInfoJson != null)
            {
                var memberInfo = JsonSerializer.Deserialize<MemberSummaryTable>(memberInfoJson);
                return memberInfo.Id;
            }

            return null;
        }
        public int? getMemberRemainingClass()
        {
            var data = _context.MemberSummaryTable.Where(m => m.Id == getCurrentMemberId())
                .Select(m => m.RemainingCourse).FirstOrDefault();
            return data;
        }
        public IActionResult Profile()
        {
            string userName = HttpContext.Session.GetString("UserName");
            string id = HttpContext.Session.GetString("id");
            ViewBag.UserName = userName;//send member's name to _Layout
            ViewBag.Id = id;
            var data = getMemberInfo();
            return View(data);
        }

        public IActionResult CrouseRecord()
        {
            string userName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = userName;
            ViewBag.StatusList = getStatusList();
            ViewBag.RemainingClass = getMemberRemainingClass();
            return View();
        }
        public IActionResult Contract()
        {
            string userName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = userName;
            return View();
        }
        public IActionResult Term()
        {
            string userName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = userName;
            return View();
        }
        public IActionResult Purchase()
        {
            string userName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = userName;
            var data = getClassList();
            return View(data);
        }
        #region APIs 
        [HttpPost]
        public IActionResult Checkout(IFormCollection form)
        {
            string userName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = userName;

            //check if user check any item
            if (form.Keys.Any(k => k != "__RequestVerificationToken"))
            {
                //filter out __RequestVerificationToken
                var keyValues = form.Keys
                                .Where(k => k != "__RequestVerificationToken")
                                .Select(k => int.Parse(form[k]))
                                .ToList();
                //check Client-Side Tampering(if all values sent in are real cIds)
                bool test = keyValues.All(k => getClassList().Select(c => c.cId).Contains(k));
                if (test == false)
                        return RedirectToAction("Purchase");
                else
                {
                    COrderRequest orderRequest = new COrderRequest();
                    orderRequest.oClassId.AddRange(keyValues);
                    var result = getClassList().Where(c => orderRequest.oClassId.Any(o => o == c.cId))
                                                .Select(o => new COrderRequestViewModel
                                                {   
                                                    dProductId=o.cId,
                                                    dPrice = o.cPerClassPrice,
                                                    dClassCount = o.cClassCount,
                                                })
                                                .ToList();

                    orderRequest.oOrderVM = result;
                    return View(orderRequest);
                }
            }
            return RedirectToAction("Purchase");
        }

        [HttpPost]
        public IActionResult EditProfile(CMemberInfo member)
        {

            MemberSummaryTable x = _context.MemberSummaryTable.FirstOrDefault(m => m.Id == member.mId);
            if (x != null)
            {
                x.Name = member.mName;
                x.Phone = member.mPhone;
                x.Birthday = DateTime.Parse(member.mBirth);
                x.Gender = member.mSex;
                x.FamilyName = member.mContactName;
                x.FamilyPhone = member.mContactPhone;

                _context.SaveChanges();
                return Json(new { success = true, message = "修改成功" });
            }
            return Json(new { success = false, message = "錯誤" });
        }
        [HttpPost]
        public IActionResult verifyAccount(string account, COrderRequest orderRequest)
        { 
            Regex rule = new Regex(@"^\d{5}$");

            if (rule.IsMatch(account))
            {

                OrderTable newOrder = new OrderTable
                {
                    MemberId = getCurrentMemberId(),
                    OrderPrice = orderRequest.oTotalPrice,
                    Back5Number = int.Parse(account),
                    Approval = false,
                    OrderDate = DateTime.Now
                };
                _context.OrderTable.Add(newOrder);
                foreach (var i in orderRequest.oOrderVM)
                {

                    OrderDetialTable newOrderDetail = new OrderDetialTable
                    {
                        OrderId = newOrder.Id,
                        ProductId = i.dProductId,
                        //11/27 edit:+
                        ProductCapacity = i.dClassCount
                    };
                    _context.OrderDetialTable.Add(newOrderDetail);

                    newOrder.OrderDetialTable.Add(newOrderDetail);
                }

                _context.SaveChanges();

                return Json(new { success = true, message = "已收到您的訂單要求，請等待管理員核對您的匯款帳號" });
            }
            return Json(new { success = false, message = "輸入格式錯誤，請重新輸入!" });
        }
        public IActionResult getCourseRecord(DateTime start, DateTime end, string status)
        {
            var data = _context.StudentCourseTable
                .Where(c => c.MemberId == getCurrentMemberId() && c.Course.StartCourseTime.Value.Date >= start.Date && c.Course.StartCourseTime.Value.Date <= end.Date)
                .OrderByDescending(c => c.ApplyDate)
                .Select(c => new
                {
                    cId = c.CourseId,
                    cTeacher = c.Course.Teacher.Name,
                    cName = c.Course.CourseTitle,
                    cBookedDate = (DateTime?)c.ApplyDate.Value.Date,
                    cDate = (DateTime?)c.Course.StartCourseTime.Value.Date,
                    cStatus = c.Status.Status
                });
            if (status != null)
            {
                data = data.Where(s => s.cStatus == status);
            }
            return Json(data);
        }
        #endregion

        #region Methods
        public CMemberInfo getMemberInfo()
        {
            var data = _context.MemberSummaryTable
                        .Where(m => m.Id == getCurrentMemberId())
                        .Select(m => new CMemberInfo
                        {
                            mId = getCurrentMemberId(),
                            mName = m.Name,
                            mPhone = m.Phone,
                            mBirth = m.Birthday.ToString(),
                            mSex = m.Gender,
                            mContactName = m.FamilyName,
                            mContactPhone = m.FamilyPhone,
                        }).FirstOrDefault();
            return data;
        }
        private List<string> getStatusList()
        {
            var data = _context.StatusTable
                .Where(s => s.TypeId == 2)
                .Select(s => s.Status)
                .ToList();
            return data;
        }
        public List<CClassPriceList> getClassList()
        {
            var data = _context.ProductsTable
                        .Select(c => new CClassPriceList
                        {
                            cId = c.Id,
                            cCourseName = c.ProductName,
                            cClassCount = c.ProductCapacity,
                            cPerClassPrice = c.Price,
                            cDueDay=c.DueDay,
                        }).ToList();
            return data;
        }
        #endregion
    }
}
