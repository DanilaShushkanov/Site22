using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Site22.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using ClosedXML.Excel;
using System.Globalization;
using System.Drawing;
using DocumentFormat.OpenXml.Drawing;
using System.Data;
using System.Web.UI.WebControls;
using Microsoft.Owin.Security.OAuth;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.IO;

namespace Site22.Controllers
{
    public class HomeController : Controller

    {
        ThemesContext db = new ThemesContext(); // Основная бд
        ApplicationDbContext db1 = new ApplicationDbContext(); // бд авторизации
        

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        public ActionResult Admin()
        {
            return View();
        }

        
        [Authorize(Roles = "admin")]
        [HttpGet]
        public ActionResult EditInfo(int? id = 1) //Редактирование информации о сотруднике
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            Employee employee = db.Employees.Find(id);

            if (employee != null)
            {
                return View(employee);
            }
            return HttpNotFound();
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult EditInfo(Employee employee)
        {
            db.Entry(employee).State = EntityState.Modified;


            db.SaveChanges();
            return RedirectToAction("Index");
        }
        //добавление нового сотрудника в базу данных
        [Authorize(Roles = "admin")]
        [HttpGet]
        public ActionResult create() 
        {
            Employee employee = new Employee();
            return View(employee);
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult create(Employee employee)
        {
            int maxID = db.Employees.Select(m => m.ID).Max();
            employee.ID = maxID + 1;
            db.Entry(employee).State = EntityState.Added;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        //Удаление сотрудника из базы данных
        [Authorize(Roles = "admin")]
        public ActionResult Delete(int id)
        {
            Employee b = db.Employees.Find(id);
            if (b != null)
            {
                db.Employees.Remove(b);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "admin")]
        [HttpGet]
        public ActionResult Delete(int? ID = 0)
        {
            Employee employee = db.Employees.Find(ID);
            if (employee != null)
                return View(employee);
            else
                return HttpNotFound();
        }

        //создание новой новости
        [HttpGet]
        [Authorize(Roles = "user")]
        public ActionResult CreateNews()
        {
            return View();
        }

        [Authorize(Roles = "user")]
        [HttpPost]
        public ActionResult CreateNews(News news)
        {
            ApplicationUserManager userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = userManager.FindByName(User.Identity.Name); //User.Identity.Name - имя текущего пользователя
            int maxID = db.News.Select(m => m.ID).Max();
            news.ID = maxID + 1;
            news.ID_Author = db.Employees.Where(m => m.Name == user.Email).Select(m => m.ID).Single();
            db.Entry(news).State = EntityState.Added;
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
        //подробная инф. о сотруднике
        [HttpGet]
        public ActionResult AboutEmployee(int? ID = 2)
        {
            IQueryable<Employee> employees = db.Employees;
            Employee e = employees.Where(p => p.ID == ID).FirstOrDefault();

            IList<Them> thems = db.Thems.Where(p => p.ID_Employee == ID).ToList();
            IList<Scientific_works> Scient_w = db.Scientific_works.Where(p => p.ID_employee == ID).ToList();

            ViewBag.ID = ID;
            ViewBag.Name = e.Name;

            if (Scient_w == null || Scient_w.Count == 0)
                Scient_w.Add(new Scientific_works() { Name = "Работ нет" });

            ViewData["Scient"] = Scient_w;
            if (thems == null || thems.Count == 0)
                thems.Add(new Them() { Name = "Тем курсовых нет" });
            ViewData["Thems"] = thems;
            ViewBag.Academ = e.Academic_degree;
            ViewBag.Position = e.Position;
            ViewBag.Description = e.Description;
            ViewBag.Time = e.WorkingTime;

            return View();
        }
        //вывод новостей
        [HttpGet]
        public ActionResult News(int page = 1)
        {
            int pageSize = 5; // количество объектов на страницу            
            IEnumerable<News> NewsPerPages = db.News.OrderByDescending(p => p.ID).Skip((page - 1) * pageSize).Take(pageSize);
            PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = db.News.Count() };
            NewsHelp nw = new NewsHelp
            {
                news = NewsPerPages,
                PageInfo = pageInfo
            };
            return View(nw);
        }

        //вывод сотрудников
        public ActionResult FilteredBrowse(string Position, string Academ, int page = 1)
        {
            IQueryable<Employee> employees = db.Employees;
            if (!String.IsNullOrEmpty(Academ) && !Academ.Equals("Все"))
            {
                employees = employees.Where(p => p.Academic_degree == Academ);
            }
            if (!String.IsNullOrEmpty(Position) && !Position.Equals("Все"))
            {
                employees = employees.Where(p => p.Position == Position);
            }

            List<Employee> teachers = db.Employees.ToList();
            int pageSize = 10; // количество объектов на страницу            
            IEnumerable<Employee> emoloyeesPerPages = employees.OrderBy(p => p.ID).Skip((page - 1) * pageSize).Take(pageSize);
            PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = employees.Count() };
            var postions = db.Employees.Select(m => m.Position).Distinct().ToList();
            postions.Add("Все");
            var dergee = db.Employees.Select(m => m.Academic_degree).Distinct().ToList();
            dergee.Add("Все");
            FilteredWorks fw = new FilteredWorks
            {
                Employees = emoloyeesPerPages.ToList(),
                
                Positions = new SelectList(postions),
                Scien = new SelectList(dergee),
                SelectedPosition = Position,
                isAdmin = IsAdmin(),
                SelectedAcadem_degree = Academ,
                PageInfo = pageInfo,
            };
            return View(fw);
        }

       

        [HttpGet]
        public ActionResult DownloadFile()
        {
            return View();
        }


        

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase fileExcel, int semCount, List<EmpAndHours> Itemlist)
        {
            using (XLWorkbook workbook = new XLWorkbook(fileExcel.InputStream, XLEventTracking.Disabled))
            {

                List<String> Errors = new List<string>();
                List <EmpAndHours> empAndHours = new List<EmpAndHours>();
                IXLWorksheet worksheet = workbook.Worksheet("План");
                int count = 0;

                int semestrCount = worksheet.Row(2).CellsUsed().Count();
                int VidyControlya = 6;
                Dictionary<String, List<Semestr>> Subject = new Dictionary<string, List<Semestr>>();
                int startPosition = 15;
                foreach (IXLRow row in worksheet.RowsUsed())
                {
                    if (row.Cell("BL").Value.ToString().Contains("0605"))
                    {
                        count++;
                        List<Semestr> Clocks = new List<Semestr>();
                        String SubName = row.Cell(3).Value.ToString();
                        Semestr semestr = new Semestr();
                        semestr.Clock = new float[6];
                        for (int i = 0; i < semestrCount * VidyControlya; i++)
                        {


                            if (!row.Cell(i + startPosition).IsEmpty())
                            {
                                semestr.number = i / 6 + 1;
                                while (semestr.number == i / 6 + 1)
                                {
                                    String temp = row.Cell(i + startPosition).Value.ToString();
                                    if (temp == "")
                                        semestr.Clock[i % 6] = 0;
                                    else
                                        semestr.Clock[i % 6] = float.Parse(temp, CultureInfo.InvariantCulture);
                                    i++;
                                }
                                i--;
                                Clocks.Add(semestr);
                                semestr = new Semestr();
                                semestr.Clock = new float[6];

                            }



                        }
                        Subject.Add(SubName, Clocks);

                    }
                    /*foreach(var item in Subject)
                    {
                        item.
                    }*/

                }
                
                /*foreach(var item in Subject)
                {
                    var temp = db.Subjects.Where(x => x.Name == item.Key).SingleOrDefault();
                    if (temp == null)
                    {
                        Subjects subjects = new Subjects();
                        subjects.Name = item.Key;
                        subjects.ID_employee = new Random().Next(1, 6);
                        subjects.Coef = Convert.ToSingle(Math.Round( new Random().NextDouble() * (1-0.1)+0.1, 1));
                        //int Last_ID - db.Subjects.Select(e=>e.ID).OrderByDescending(i)
                        try
                        {
                            db.Subjects.Add(subjects);
                            db.SaveChanges();
                            ViewBag.Subjects += subjects;
                        }
                        catch(Exception)
                        {

                        }

                    }
                   
                }*/
                foreach (var item in Subject)
                {
                    List<Subjects> employes4thissub = new List<Subjects>() ;
                    
                    employes4thissub = db.Subjects.Where(n => n.Name == item.Key).OrderByDescending(z => z.Coef).ToList();
                    
                    if (employes4thissub.Count != 0)
                    {
                        float timeForLect = 0;
                        int countWorkers = 0;
                        int timeForLab = 0;
                        int timeForPract = 0;
                        foreach (var sem in item.Value)
                            timeForLect += sem.Clock[1];
                        foreach (var sem in item.Value)
                            timeForPract += (int)sem.Clock[3];
                        foreach (var sem in item.Value)
                            timeForLab += (int)sem.Clock[2];
                        timeForLab *= semCount;
                        foreach (var employee in employes4thissub)
                        {
                            countWorkers++;
                            Employee employee1 = employee.Employee;
                            EmpAndHours empAndHoursRes = new EmpAndHours();
                            Employee Employee123 = db.Employees.Where(e => e.ID == employee.ID_employee).Where(m => (m.Position == "Профессор" || m.Position == "Доцент") && m.WorkingTime >0).SingleOrDefault();
                            //Single.TryParse(Employee.ToString(), out time);
                            
                                

                           

                            if (employee1 != null   && (employee1.Position == "Профессор" || employee1.Position == "Доцент") && employee1.WorkingTime>0 )
                            {
                                if (timeForLab == 0 && timeForLect == 0 && timeForPract == 0)
                                    break;
                                empAndHoursRes.subject = item.Key;
                                empAndHoursRes.EmpName = employee1.Name;

                                if (timeForLect > employee1.WorkingTime)
                                {
                                    timeForLect -= (int)employee1.WorkingTime;
                                    empAndHoursRes.hoursForLect = (int)employee1.WorkingTime;
                                    employee1.WorkingTime = 0;
                                }
                                else
                                {
                                    empAndHoursRes.hoursForLect = (int)timeForLect;
                                    employee1.WorkingTime -= (int)timeForLect;
                                    timeForLect = 0;

                                    if (timeForPract > employee1.WorkingTime)
                                    {
                                        timeForPract -= (int)employee1.WorkingTime;
                                        empAndHoursRes.hoursForPractice = (int)employee1.WorkingTime;
                                        employee1.WorkingTime = 0;
                                    }
                                    else
                                    {
                                        employee1.WorkingTime -= timeForPract;
                                        empAndHoursRes.hoursForPractice = timeForPract;
                                        timeForPract = 0;

                                        if (timeForLab > employee1.WorkingTime)
                                        {
                                            timeForLab -= (int)employee1.WorkingTime;
                                            empAndHoursRes.hoursForLab = (int)employee1.WorkingTime;
                                            employee1.WorkingTime = 0;
                                        }
                                        else
                                        {
                                            employee1.WorkingTime -= timeForLab;
                                            empAndHoursRes.hoursForLab = timeForLab;
                                            timeForLab = 0;
                                        }

                                    }

                                    /*db.Entry(Employee123).State = EntityState.Added;
                                    db.SaveChanges();*/
                                }

                                db.Entry(employee1).State = EntityState.Modified;
                                db.SaveChanges();
                                empAndHours.Add(empAndHoursRes);
                            }
                            else if ( employee1.WorkingTime > 0)
                            {
                                if (timeForLab == 0 && timeForPract == 0)
                                    break;
                                Employee Employee12 = db.Employees.Where(e => e.ID == employee.ID_employee).Where(m => m.WorkingTime > 0).SingleOrDefault();
                                                               /*if (Employee12 == null)
                                {
                                    var temp = employee.Name;
                                    String error1 = (temp.ToString() + " - не нашлось свободного ? преподавателя");
                                    Errors.Add(error1);
                                    //ViewBag.Bad += employes4thissub.Select(m => m.Name).SingleOrDefault().ToString() + " - не нашлось преподавателя"  + "\n";
                                    break;
                                }*/
                                
                                empAndHoursRes.subject = item.Key;
                                empAndHoursRes.EmpName = employee1.Name;
                                

                                if (timeForPract > employee1.WorkingTime)
                                {
                                    timeForPract -= (int)employee1.WorkingTime;
                                    empAndHoursRes.hoursForPractice = (int)employee1.WorkingTime;
                                    employee1.WorkingTime = 0;
                                 
                                }
                                else
                                {
                                    employee1.WorkingTime -= timeForPract;
                                    empAndHoursRes.hoursForPractice = timeForPract;
                                    timeForPract = 0;
                                    
                                    if (timeForLab > employee1.WorkingTime)
                                    {
                                        timeForLab -= (int)employee1.WorkingTime;
                                        empAndHoursRes.hoursForLab = (int)employee1.WorkingTime;
                                        employee1.WorkingTime = 0;
                                      
                                        

                                    }
                                    else
                                    {

                                        employee1.WorkingTime -= timeForLab;
                                        empAndHoursRes.hoursForLab = timeForLab;
                                        timeForLab = 0;
                                        


                                    }

                                }

                                empAndHours.Add(empAndHoursRes);
                                db.Entry(employee1).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            

                        }
                        if (countWorkers == employes4thissub.Count && (timeForLect != 0 || timeForLab != 0 || timeForPract != 0))
                        {
                            String error = ("Не распределен педмет: " + item.Key + " " + "лаб.: " + timeForLab.ToString() + " лекц.: " + timeForLect.ToString() + " практич.: " + timeForPract.ToString());
                            Errors.Add(error);
                            //ViewBag.Bad += ("Не распределен " + employes4thissub.Select(m => m.Name).SingleOrDefault().ToString() + " " + timeForLab.ToString() + " " + timeForLect.ToString() + " " + timeForPract.ToString()+ "\n" );
                        }
                    }
                    else
                    {
                            String error1 = (item.Key + " - не нашлось свободного  преподавателя");
                            Errors.Add(error1);
                            //ViewBag.Bad += employes4thissub.Select(m => m.Name).SingleOrDefault().ToString() + " - не нашлось преподавателя"  + "\n";
                    }

                
                
                }



                ViewBag.Bad = Errors;
                
                    return View(empAndHours);
            }

            

        }

        public ActionResult Export(List<EmpAndHours> Itemlist)
        {

            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var worksheet = workbook.Worksheets.Add("Нагрузка");

                worksheet.Cell("A1").Value = "Преподаватель";
                worksheet.Cell("B1").Value = "Часы";
                worksheet.Cell("D1").Value = "Лекции";
                worksheet.Cell("F1").Value = "Практика";
                worksheet.Cell("H1").Value = "Лабораторные";
                worksheet.Row(1).Style.Font.Bold = true;
               

                //нумерация строк/столбцов начинается с индекса 1 (не 0)
                for (int i = 0; i < Itemlist.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = Itemlist[i].EmpName;
                    worksheet.Cell(i + 2, 2).Value = Itemlist[i].subject ;
                    worksheet.Cell(i + 2, 4).Value = Itemlist[i].hoursForLect;
                    worksheet.Cell(i + 2, 6).Value = Itemlist[i].hoursForPractice;
                    worksheet.Cell(i + 2, 8).Value = Itemlist[i].hoursForLab;
                    worksheet.Row(i + 2).AdjustToContents();
                }
               
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();

                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"план_от{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }
        }

        //вспомогательная функция
        private bool IsAdmin()
        {
            return User.IsInRole("admin");
        }
    }
}