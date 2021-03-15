using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DCandidateController : ControllerBase
    {


        private readonly DonationDBContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DCandidateController(DonationDBContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            this._hostEnvironment = hostEnvironment;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<DCandidate>>> GetDCandidates()
        {
            return await _context.DCandidates.Select(x => new DCandidate()
            {

                EmployeeID = x.EmployeeID,
                EmployeeName = x.EmployeeName,
                Occupation = x.Occupation,
                ImageName = x.ImageName,
                ImageSrc = string.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, x.ImageName)
            }).ToListAsync();
        }


        // GET: api/DCandidate/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DCandidate>> GetDCandidate(int id)
        {
            var dCandidate = await _context.DCandidates.FindAsync(id);

            if (dCandidate == null)
            {
                return NotFound();
            }

            return dCandidate;
        }


       
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDCandidate(int id, [FromForm] DCandidate dCandidate)
        {
            dCandidate.EmployeeID = id;

            _context.Entry(dCandidate).State = EntityState.Modified;

            if (id != dCandidate.EmployeeID)
            {
                return BadRequest();
            }

            if (dCandidate.ImageFile != null)
            {
                DeleteImage(dCandidate.ImageName);
                dCandidate.ImageName = await SaveImage(dCandidate.ImageFile);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DCandidateExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        [HttpPost]
        public async Task<ActionResult<DCandidate>> PostDCandidate([FromForm] DCandidate dCandidate)
        {
            dCandidate.ImageName = await SaveImage(dCandidate.ImageFile);
            _context.DCandidates.Add(dCandidate);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetDCandidate", new { id = dCandidate.EmployeeID }, dCandidate);
            return StatusCode(201);
        }

        [HttpDelete("{EmployeeID}")]
        public async Task<ActionResult<DCandidate>> DeleteDCandidate(int EmployeeID)
        {
            var dCandidate = await _context.DCandidates.FindAsync(EmployeeID);
            if (dCandidate == null)
            {
                return NotFound();
            }

            _context.DCandidates.Remove(dCandidate);
            await _context.SaveChangesAsync();

            return dCandidate;
        }



        private bool DCandidateExists(int id)
        {
            return _context.DCandidates.Any(e => e.EmployeeID == id);
        }

        [NonAction]
        public async Task<string> SaveImage(IFormFile imageFile)
        {

            string imageName = new string(Path.GetFileNameWithoutExtension(imageFile.FileName).Take(10).ToArray()).Replace(' ', '-');
            imageName = imageName + DateTime.Now.ToString("yymmssff") + Path.GetExtension(imageFile.FileName);
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);
            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return imageName;
        }


        [NonAction]
        public void DeleteImage(string imageName)
        {
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }


    }
}
