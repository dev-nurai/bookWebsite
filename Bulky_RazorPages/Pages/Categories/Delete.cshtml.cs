using Bulky_RazorPages.Data;
using Bulky_RazorPages.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bulky_RazorPages.Pages.Categories
{
    [BindProperties]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationRazorPageDbContext _context;

        public Category Category { get; set; }

        public DeleteModel(ApplicationRazorPageDbContext context)
        {
            _context = context;
        }
        public void OnGet(int? id)
        {
            if(id != null && id > 0)
            {
                Category = _context.Categories.FirstOrDefault(x => x.Id == id);
            }
        }

        public IActionResult OnPost()
        {
            if(!ModelState.IsValid)
            {
                _context.Categories.Remove(Category);
                _context.SaveChanges();
                TempData["success"] = "Category deleted successfully";
                return RedirectToPage("Index");
            }
            return Page();
        }
    }
}
