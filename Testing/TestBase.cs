using Microsoft.EntityFrameworkCore;
using EduSyncWebApi.Data;
using NUnit.Framework;

namespace Testing
{
    public abstract class TestBase
    {
        protected AppDbContext _context;
        protected DbContextOptions<AppDbContext> _options;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{GetType().Name}")
                .Options;
        }

        [SetUp]
        public virtual void Setup()
        {
            _context = new AppDbContext(_options);
            _context.Database.EnsureCreated();
        }

        [TearDown]
        public virtual void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _context?.Dispose();
        }
    }
} 