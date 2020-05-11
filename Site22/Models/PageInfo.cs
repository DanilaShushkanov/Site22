using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
namespace Site22.Models
{
    public class PageInfo
    {

        public int PageNumber { get; set; } // номер текущей страницы        
        public int PageSize { get; set; } // кол-во объектов на странице        
        public int TotalItems { get; set; } // всего объектов        
        public int TotalPages  // всего страниц        
        {
            get { return (int)Math.Ceiling((decimal)TotalItems / PageSize); }
        }


    }
    public class FilteredWorks
    {        // Список тем        
        public IEnumerable<Employee> Employees { get; set; }        // Выбранные условия фильтра        
        public string SelectedPosition { get; set; }
        public string SelectedAcadem_degree { get; set; }
        public int ID { get; set; }
        public bool isAdmin { get; set; }

        // Элементы формы        

        public SelectList Positions { get; set; }
        public SelectList Scien { get; set; }
        // Инфа для пагинации        
        public PageInfo PageInfo { get; set; }
    }
    public class NewsHelp
    {     
        public IEnumerable<News> news { get; set; }              
        // Инфа для пагинации        
        public PageInfo PageInfo { get; set; }
    }
}