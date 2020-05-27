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
        public ActionResult News(int? ID = 1, int page = 1)
        {

            IQueryable<News> news = db.News;
            News n = news.Where(p => p.ID == ID).FirstOrDefault();
            List<Employee> employees = db.Employees.Where(p => p.ID == n.ID_Author).ToList();
            Employee e = employees.FirstOrDefault();
            int pageSize = 2; // количество объектов на страницу            
            IEnumerable<News> NewsPerPages = news.OrderByDescending(p => p.ID).Skip((page - 1) * pageSize).Take(pageSize);
            PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = news.Count() };
            NewsHelp nw = new NewsHelp
            {
                news = NewsPerPages,

                PageInfo = pageInfo

            };
            ViewBag.Author = e.Name;


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
            int pageSize = 5; // количество объектов на страницу            
            IEnumerable<Employee> emoloyeesPerPages = employees.OrderBy(p => p.ID).Skip((page - 1) * pageSize).Take(pageSize);
            PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = employees.Count() };

            FilteredWorks fw = new FilteredWorks
            {
                Employees = emoloyeesPerPages.ToList(),
                Positions = new SelectList(new List<string>() { "Все", "Секретарь", "Зав. кафедры", "Заместитель зав. кафедры", "Преподаватель", "Доцент" }),
                Scien = new SelectList(new List<string>() { "Все", "Бакалавр", "Магистр", "Кандидат наук", "Доктор наук" }),
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
        public ActionResult Import(HttpPostedFileBase fileExcel)
        {
            using (XLWorkbook workbook = new XLWorkbook(fileExcel.InputStream, XLEventTracking.Disabled))
            {


                List <EmpAndHours> empAndHours = new List<EmpAndHours>();
                IXLWorksheet worksheet = workbook.Worksheet("План");
                int count = 0;

                int semestrCount = worksheet.Row(2).CellsUsed().Count();
                int VidyControlya = 6;

                Dictionary<String, List<Semestr>> Subject = new Dictionary<string, List<Semestr>>();
                int startPosition = 15;

                foreach (IXLRow row in worksheet.RowsUsed())
                {
                    if (/*row.Cell(1).Value.ToString() == "+" && */row.Cell("BL").Value.ToString().Contains("0605"))
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
                    EmpAndHours empAndHoursRes = new EmpAndHours();
                    using (ThemesContext db = new ThemesContext())
                    {
                        employes4thissub = db.Subjects.Where(n => n.Name == item.Key).OrderByDescending(z => z.Coef).ToList();
                    }
                    if (employes4thissub != null)
                    {
                        float timeForLect = 0;
                        int countWorkers = 0;
                        int timeForLab = 0;
                        int timeForPract = 0;
                        foreach (var employee in employes4thissub)
                        {
                            countWorkers++;
                            Employee Employee123 = db.Employees.Where(e => e.ID == employee.ID_employee).Where(m => (m.Position == "Профессор" || m.Position == "Доцент") && m.WorkingTime >0).SingleOrDefault();
                            //Single.TryParse(Employee.ToString(), out time);



                            if (Employee123 != null)
                            {
                                empAndHoursRes.subject = item.Key;
                                empAndHoursRes.EmpName = Employee123.Name;

                                foreach (var sem in item.Value)
                                    timeForLect += sem.Clock[1];

                                if (timeForLect > Employee123.WorkingTime)
                                {

                                    timeForLect -= (int)Employee123.WorkingTime;
                                    empAndHoursRes.hoursForLect = (int)Employee123.WorkingTime;
                                    Employee123.WorkingTime = 0;
                                    db.Entry(Employee123).State = EntityState.Modified;
                                    db.SaveChanges();
                                    empAndHours.Add(empAndHoursRes);


                                }
                                else
                                {
                                    empAndHoursRes.hoursForLect = (int)timeForLect;
                                    Employee123.WorkingTime -= (int)timeForLect;

                                    foreach (var sem in item.Value)
                                        timeForPract += (int)sem.Clock[3];

                                    if (timeForPract > Employee123.WorkingTime)
                                    {
                                        timeForPract -= (int)Employee123.WorkingTime;
                                        empAndHoursRes.hoursForPractice = (int)Employee123.WorkingTime;
                                        Employee123.WorkingTime = 0;
                                        db.Entry(Employee123).State = EntityState.Modified;
                                        db.SaveChanges();
                                        empAndHours.Add(empAndHoursRes);
                                    }
                                    else
                                    {
                                        Employee123.WorkingTime -= timeForPract;
                                        empAndHoursRes.hoursForPractice = timeForPract;

                                        foreach (var sem in item.Value)
                                            timeForLab += (int)sem.Clock[2];
                                        if (timeForLab > Employee123.WorkingTime)
                                        {
                                            timeForLab -= (int)Employee123.WorkingTime;
                                            empAndHoursRes.hoursForLab = (int)Employee123.WorkingTime;
                                            Employee123.WorkingTime = 0;
                                            db.Entry(Employee123).State = EntityState.Modified;
                                            db.SaveChanges();
                                            empAndHours.Add(empAndHoursRes);
                                            

                                        }
                                        else
                                        {

                                            Employee123.WorkingTime -= timeForLab;
                                            empAndHoursRes.hoursForLab = timeForLab;
                                            empAndHours.Add(empAndHoursRes);
                                            db.Entry(Employee123).State = EntityState.Modified;
                                            db.SaveChanges();


                                        }

                                    }

                                    /*db.Entry(Employee123).State = EntityState.Added;
                                    db.SaveChanges();*/
                                }


                            }
                            else
                            {
                                Employee Employee12 = db.Employees.Where(e => e.ID == employee.ID_employee).Where(m => m.WorkingTime > 0).SingleOrDefault();
                                
                                if (Employee12 == null)
                                {
                                    ViewBag.Bad += employes4thissub.Select(m => m.Name).SingleOrDefault().ToString() + " - не нашлось преподавателя"  + "\n";
                                    break;
                                }
                                
                                empAndHoursRes.subject = item.Key;
                                empAndHoursRes.EmpName = Employee12.Name;
                                foreach (var sem in item.Value)
                                    timeForPract += (int)sem.Clock[3];

                                if (timeForPract > Employee12.WorkingTime)
                                {
                                    timeForPract -= (int)Employee12.WorkingTime;
                                    empAndHoursRes.hoursForPractice = (int)Employee12.WorkingTime;
                                    Employee12.WorkingTime = 0;
                                    db.Entry(Employee12).State = EntityState.Modified;
                                    db.SaveChanges();
                                    empAndHours.Add(empAndHoursRes);
                                }
                                else
                                {
                                    Employee12.WorkingTime -= timeForPract;
                                    empAndHoursRes.hoursForPractice = timeForPract;

                                    foreach (var sem in item.Value)
                                        timeForLab += (int)sem.Clock[2];
                                    if (timeForLab > Employee12.WorkingTime)
                                    {
                                        timeForLab -= (int)Employee12.WorkingTime;
                                        empAndHoursRes.hoursForLab = (int)Employee12.WorkingTime;
                                        Employee12.WorkingTime = 0;
                                        db.Entry(Employee12).State = EntityState.Modified;
                                        db.SaveChanges();
                                        empAndHours.Add(empAndHoursRes);
                                        

                                    }
                                    else
                                    {

                                        Employee12.WorkingTime -= timeForLab;
                                        empAndHoursRes.hoursForLab = timeForLab;
                                        empAndHours.Add(empAndHoursRes);
                                        db.Entry(Employee12).State = EntityState.Modified;
                                        db.SaveChanges();


                                    }

                                }
                            }
                            if (countWorkers == employes4thissub.Count)
                            {
                                ViewBag.Bad += ("Не распределен " + employes4thissub.Select(m => m.Name).SingleOrDefault().ToString() + " " + timeForLab.ToString() + " " + timeForLect.ToString() + " " + timeForPract.ToString()+ "\n" );
                            }

                        }
                    }

                
                
                }




                    return View(empAndHours);
            }

            

        }

        //вспомогательная функция
        private bool IsAdmin()
        {
            return User.IsInRole("admin");
        }
    }
}