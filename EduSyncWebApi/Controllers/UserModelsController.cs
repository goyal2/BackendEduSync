using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSyncWebApi.Data;
using EduSyncWebApi.Models;
using EduSyncWebApi.DTO;

namespace EduSyncWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserModelsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserModelsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetUserModels()
        {
            
            return await _context.UserModels.ToListAsync();
        }

        // GET: api/UserModels/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserModel>> GetUserModel(Guid id)
        {
            var userModel = await _context.UserModels.FindAsync(id);
            if (userModel == null)
            {
                return NotFound();
            }

            return userModel;
        }

        // PUT: api/UserModels/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserModel(Guid id, UserDTO userModel)
        {
            if (id != userModel.UserId)
            {
                return BadRequest("User ID mismatch.");
            }

            var existingUser = await _context.UserModels.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }

            existingUser.Name = userModel.Name;
            existingUser.Email = userModel.Email;
            existingUser.Role = userModel.Role;
            existingUser.PasswordHash = userModel.PasswordHash;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "A concurrency error occurred.");
            }

            return NoContent();
        }

        // POST: api/UserModels
        [HttpPost]
        public async Task<ActionResult<UserModel>> PostUserModel(UserDTO userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newUser = new UserModel
            {
                UserId = userModel.UserId,
                Name = userModel.Name,
                Email = userModel.Email,
                Role = userModel.Role,
                PasswordHash = userModel.PasswordHash
            };

            _context.UserModels.Add(newUser);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserModelExists(newUser.UserId))
                {
                    return Conflict("A user with this ID already exists.");
                }

                throw;
            }

            return CreatedAtAction(nameof(GetUserModel), new { id = newUser.UserId }, newUser);
        }


        // POST: api/UserModels/login
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login([FromBody] LoginInput loginData)
        {
            if (string.IsNullOrWhiteSpace(loginData.Email) || string.IsNullOrWhiteSpace(loginData.PasswordHash))
            {
                return BadRequest("Email and password are required.");
            }

            var user = await _context.UserModels
                .FirstOrDefaultAsync(u => u.Email == loginData.Email);

            if (user == null || user.PasswordHash != loginData.PasswordHash)
            {
                return Unauthorized("Invalid credentials.");
            }

            return Ok(new UserDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                PasswordHash = "" 
            });
        }

        public class LoginInput
        {
            public string Email { get; set; }
            public string PasswordHash { get; set; }
        }



        // DELETE: api/UserModels/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserModel(Guid id)
        {
            var user = await _context.UserModels.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            _context.UserModels.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserModelExists(Guid id)
        {
            return _context.UserModels.Any(e => e.UserId == id);
        }
    }
}
