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
using System.Data.Entity;

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

        //вспомогательная функция
        private bool IsAdmin()
        {
            return User.IsInRole("admin");
        }
    }
}