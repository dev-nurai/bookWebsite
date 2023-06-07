using Bulky_RazorPages.Data;
using Bulky_RazorPages.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bulky_RazorPages.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationRazorPageDbContext _context;

        public List<Category> CategoryList { get; set; }

        public IndexModel(ApplicationRazorPageDbContext context)
        {
            _context = context;
        }
        public void OnGet()
        {
            CategoryList = _context.Categories.ToList();
        }
    }
}
