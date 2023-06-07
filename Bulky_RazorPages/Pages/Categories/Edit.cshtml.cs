using Bulky_RazorPages.Data;
using Bulky_RazorPages.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bulky_RazorPages.Pages.Categories
{
    [BindProperties]
    public class EditModel : PageModel
    {
        private readonly ApplicationRazorPageDbContext _context;

        public Category Category { get; set; }

        public EditModel(ApplicationRazorPageDbContext context)
        {
            _context = context;
        }

        public void OnGet(int? id)
        {
            if(id != null && id != 0)
            {
                Category = _context.Categories.FirstOrDefault(c => c.Id == id);
            }
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(Category);
                _context.SaveChanges();
                TempData["success"] = "Category updated successfully";
                return RedirectToPage("Index");
            }
            return Page();

        }
    }
}
